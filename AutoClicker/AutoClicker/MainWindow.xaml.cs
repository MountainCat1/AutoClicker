﻿using System;
using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoClicker.Configuration;
using AutoClicker.Services;
using AutoClicker.Input.Services;

namespace AutoClicker
{
    public partial class MainWindow : Window
    {
        private const string ClickActiveSound = "Audio/On.wav";
        private const string ClickInactiveSound = "Audio/Off.wav";

        private readonly IWindowsApiService _windowsApiService;
        private readonly IAutoClickingService _autoClickingService;

        public MainWindow()
        {
            InitializeComponent();
            _windowsApiService = new WindowsApiService();
            _autoClickingService = new AutoClickingService(_windowsApiService)
            {
                Filter = () => !this.IsMouseOver
            };
            _autoClickingService.Toggled += OnAutoClickingToggled;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            RegisterKeys();
            InitializeUserInput();
            ToggleMaximize();
        }

        private void RegisterKeys()
        {
            KeyboardHook.KeyPressed += OnKeyPressed;
        }

        private void OnKeyPressed(Key e)
        {
            var keybinds = Settings.Singleton.KeyBinds;
            
            if (e == keybinds.Toggle)
            {
                ToggleAutoClicker();
            }

            if (e == keybinds.Show)
            {
                ToggleMaximize();
            }
        }

        private void InitializeUserInput()
        {
            UserInput.Text = "5";
            IntervalRandomnessInput.Text = "3";
        }

        private void ToggleMaximize()
        {
            if (!Topmost)// ACTIVATE
            {
                Console.WriteLine("Maximizing window");
                WindowState = WindowState.Normal;
                SetCursorToWindowCenter();
                Topmost = true;
                return;
            }

            if (this.IsMouseOver == false) // 
            {
                Console.WriteLine("Moving window to center");
                SetCursorToWindowCenter();
                return;
            }
            
            if (Topmost) // DEACTIVATE
            {
                Console.WriteLine("Minimizing window");
                Activate();
                Topmost = false;
                WindowState = WindowState.Minimized;
                return;
            }
        }

        private void OnAutoClickingToggled(bool isActive)
        {
            UpdateStatus(isActive);
            PlayToggleSound(isActive);
        }

        private void UpdateStatus(bool isActive)
        {
            StatusLabel.Content = isActive ? "Active" : "Inactive";
            if(isActive)
                StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
            else
                StatusLabel.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void PlayToggleSound(bool isActive)
        {
            var soundPath = isActive ? ClickActiveSound : ClickInactiveSound;
            using var player = new SoundPlayer(soundPath);
            player.Play();
        }

        private void ToggleAutoClicker()
        {
            Console.WriteLine("Toggling auto clicker");
            _autoClickingService.Toggle();
            Console.WriteLine($"Auto clicker is now {(IsActive ? "active" : "inactive")}");
        }

        private async void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _autoClickingService.Toggle();
        }

        private bool IsTextNumeric(string text)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private void UserInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(UserInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out float clicksPerSecond))
            {
                var clickInterval = CalculateClickInterval(clicksPerSecond);
                _autoClickingService.ClickInterval = clickInterval;
                Console.WriteLine($"Click interval set to {clickInterval} ms");
            }
        }
        
        private void IntervalRandomness_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void IntervalRandomness_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(UserInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out float value))
            {
                var input = CalculateClickInterval(value);
                _autoClickingService.IntervalRandomness = input;
                Console.WriteLine($"Click interval randomness set to {input} ms");
            }
        }

        private int CalculateClickInterval(float clicksPerSecond)
        {
            return (int)(1000 / clicksPerSecond);
        }
        
        private void SetCursorToWindowCenter()
        {
            var windowPosition = PointToScreen(new Point(0, 0));
            int windowCenterX = (int)(windowPosition.X + Width / 2);
            int windowCenterY = (int)(windowPosition.Y + Height / 2);

            _windowsApiService.SetCursorPosition(windowCenterX, windowCenterY);
        }
    }
}