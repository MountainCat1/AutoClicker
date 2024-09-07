using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using AutoClicker.Configuration;

namespace AutoClicker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Load settings
            var settingsText = File.ReadAllText("settings.json");
            
            Settings.Load(settingsText);
        }
    }
}