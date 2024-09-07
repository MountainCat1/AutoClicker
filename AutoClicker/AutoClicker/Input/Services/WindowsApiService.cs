using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using AutoClicker.Input.Enums;

namespace AutoClicker.Input.Services
{
    public interface IWindowsApiService
    {
        void RegisterKey(IntPtr windowHandle, ModifierKeys modifiers, KeyCode key, int hotkeyId, Action hotkeyCallback);
        void UnregisterKey(IntPtr windowHandle, int hotkeyId);
        void SimulateMouseClick();
        void SetCursorPosition(int x, int y);
    }

    public class WindowsApiService : IWindowsApiService
    {
        // DLL Imports for interacting with Windows API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Dictionary to store multiple hotkey callbacks by ID
        private Dictionary<int, Action> _hotkeyCallbacks = new Dictionary<int, Action>();
        private HwndSource _source;

        /// <summary>
        /// Registers a global hotkey with a specified callback to trigger when the hotkey is pressed.
        /// </summary>
        /// <param name="windowHandle">The handle of the window registering the hotkey.</param>
        /// <param name="modifiers">The modifier keys (Ctrl, Alt, Shift, Win).</param>
        /// <param name="key">The key to be used for the hotkey.</param>
        /// <param name="hotkeyId">A unique hotkey ID for the hotkey.</param>
        /// <param name="hotkeyCallback">The callback to invoke when the hotkey is pressed.</param>
        public void RegisterKey(IntPtr windowHandle, ModifierKeys modifiers, KeyCode key, int hotkeyId, Action hotkeyCallback)
        {
            // Store the callback for the specific hotkey ID
            if (!_hotkeyCallbacks.ContainsKey(hotkeyId))
            {
                _hotkeyCallbacks.Add(hotkeyId, hotkeyCallback);
            }

            // Register the hotkey
            bool success = RegisterHotKey(windowHandle, hotkeyId, (uint)modifiers, (uint)key);
            if (!success)
            {
                var error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Could not register hotkey {key} with ID {hotkeyId}. Error code: {error}");
            }

            // Set up the message loop if it's not already set up
            if (_source == null)
            {
                _source = HwndSource.FromHwnd(windowHandle);
                _source.AddHook(HwndHook);
            }
        }


        /// <summary>
        /// Unregisters the previously registered hotkey.
        /// </summary>
        /// <param name="windowHandle">The handle of the window unregistering the hotkey.</param>
        /// <param name="hotkeyId">The unique ID of the hotkey to unregister.</param>
        public void UnregisterKey(IntPtr windowHandle, int hotkeyId)
        {
            if (UnregisterHotKey(windowHandle, hotkeyId))
            {
                _hotkeyCallbacks.Remove(hotkeyId);
            }
        }

        /// <summary>
        /// Set the cursor position to specified coordinates.
        /// </summary>
        public void SetCursorPosition(int x, int y)
        {
            Console.WriteLine($"Setting cursor position to ({x}, {y})");
            SetCursorPos(x, y);
        }

        /// <summary>
        /// Simulates a left mouse click.
        /// </summary>
        public void SimulateMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// Windows message hook to handle the hotkey events.
        /// </summary>
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (_hotkeyCallbacks.ContainsKey(hotkeyId))
                {
                    // Invoke the corresponding callback for the hotkey
                    _hotkeyCallbacks[hotkeyId]?.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }
    }
}
