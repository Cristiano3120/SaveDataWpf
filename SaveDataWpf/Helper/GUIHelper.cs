using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

namespace SaveDataWpf.Helper
{
    internal sealed partial class GUIHelper
    {
        public static void SetBasicWindowUI(Window window, Grid parentGrid)
        {
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            WindowChrome windowChrome = new()
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(42),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(3),
                UseAeroCaptionButtons = false,
            };
            window.SetValue(WindowChrome.WindowChromeProperty, windowChrome);

            StackPanel stackPanel = CreateWindowinteractionBtns(parentGrid);
            CreateMoveGrid(window, parentGrid, stackPanel);
            if (window is MainWindow mainWindow)
            {
                mainWindow.CreateBtns();
            }

            CreateBorder(parentGrid);
        }

        private static StackPanel CreateWindowinteractionBtns(Grid parentGrid)
        {
            const byte fontSize = 18;
            const byte height = 30;
            const byte width = 30;

            StackPanel stackPanel = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Horizontal,
                Background = Brushes.Transparent,
                Width = width * 3,
                Height = height,
            };

            Button closeBtn = new()
            {
                FontSize = fontSize - 2,
                Padding = new(0, 3.5, 7.5, 0),
                Width = width,
                Content = "X",
                Style = (Style)Application.Current.FindResource("RightBtn")
            };

            Button maximizeBtn = new()
            {
                FontWeight = FontWeights.Bold,
                Width = width,
                Content = "🗖",
                Style = (Style)Application.Current.FindResource("MidBtn")
            };

            Button minimizeBtn = new()
            {
                FontWeight = FontWeights.Bold,
                Padding = new(0, 3, 0, 0),
                Width = width,
                Content = "―",
                Style = (Style)Application.Current.FindResource("MidBtn")
            };

            closeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window.GetWindow((Button)sender).Close();
                });
            };

            maximizeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window window = Window.GetWindow((Button)sender);
                    switch (window.WindowState)
                    {
                        case WindowState.Maximized:
                            window.WindowState = WindowState.Normal;
                            break;
                        case WindowState.Normal
                            when window.MaxHeight != window.Height && window.MaxWidth != window.Width:
                            window.WindowState = WindowState.Maximized;
                            break;
                    }
                });
            };

            minimizeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window.GetWindow((Button)sender).WindowState = WindowState.Minimized;
                });
            };

            stackPanel.Children.Add(minimizeBtn);
            stackPanel.Children.Add(maximizeBtn);
            stackPanel.Children.Add(closeBtn);

            parentGrid.Children.Add(stackPanel);
            return stackPanel;
        }

        private static void CreateMoveGrid(Window window, Grid parentGrid, StackPanel stackPanel)
        {
            Grid moveGrid = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = window.Width - stackPanel.Width,
                Background = Brushes.Transparent,
                Height = stackPanel.Height,
            };

            window.SizeChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    moveGrid.Width = args.NewSize.Width - stackPanel.Width;
                });
            };

            moveGrid.PreviewMouseLeftButtonDown += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.DragMove();
                });
            };

            parentGrid.Children.Add(moveGrid);
        }

        private static void CreateBorder(Grid parentGrid)
        {
            Border border = new()
            {
                CornerRadius = new(20),
                BorderBrush = HexToBrush("#2e2d2e"),
                BorderThickness = new Thickness(2)
            };

            parentGrid.Children.Add(border);
        }

        public static SolidColorBrush HexToBrush(string value)
        {
            object brush = new BrushConverter().ConvertFromString(value)
                ?? throw new ArgumentException($"{nameof(value)} was an invalid HEX-Color");

            return (SolidColorBrush)brush;
        }
    }
}
