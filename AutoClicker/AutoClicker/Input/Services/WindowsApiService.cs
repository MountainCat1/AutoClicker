using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AutoClicker.Input.Services
{
    public interface IWindowsApiService
    {
        void SimulateMouseClick();
        void SetCursorPosition(int x, int y);
    }

    public class WindowsApiService : IWindowsApiService
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private HwndSource _source;
        

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
    }
}
