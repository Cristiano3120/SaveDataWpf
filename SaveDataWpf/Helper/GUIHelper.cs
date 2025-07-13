using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace SaveDataWpf
{
    internal sealed partial class GUIHelper
    {
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_CLOSE = 0xF060;

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void RegisterWindowButtons(Button minimizeButton, Button maximizeButton, Button closeButton)
        {
            minimizeButton.Click += MinimizeButton_Click;
            closeButton.Click += CloseButton_Click;
        }

        private static void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow((Button)sender);
            nint windowHandle = new WindowInteropHelper(window).Handle;
            SendMessage(windowHandle, WM_SYSCOMMAND, SC_MINIMIZE, IntPtr.Zero);
        }

        private static void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow((Button)sender);
            nint windowHandle = new WindowInteropHelper(window).Handle;
            SendMessage(windowHandle, WM_SYSCOMMAND, SC_CLOSE, IntPtr.Zero);
        }
    }
}
