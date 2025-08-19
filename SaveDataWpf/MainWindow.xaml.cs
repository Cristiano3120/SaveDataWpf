using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SaveDataWpf
{
    public sealed partial class MainWindow : Window
    {
        private Dictionary<string, SavedContent> _savedData = ManageSavedData.LoadData();

        public MainWindow()
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, ParentGrid);
            LoadListBox();
        }

        public void CreateBtns()
        {
            StackPanel stackPanel = new()
            {
                Background = Brushes.Transparent,
                Height = 30,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            Button encryptAllBtn = new()
            {
                Style = (Style)Application.Current.FindResource("LeftBtn"),
                Name = "EncryptAllBtn",
                Height = 30,
                Width = 85,
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "Encrypt all"
            };
            encryptAllBtn.Click += EncryptAllBtn_Click;

            Button decryptAllBtn = new()
            {
                Style = (Style)Application.Current.FindResource("MidBtn"),
                Name = "DecryptAllBtn",
                Height = 30,
                Width = 85,
                Content = "Decrypt all"
            };
            decryptAllBtn.Click += DecryptAllBtn_Click;

            Button deleteAllBtn = new()
            {
                Style = (Style)Application.Current.FindResource("MidBtn"),
                Name = "DeleteAllBtn",
                Height = 30,
                Width = 75,
                Content = "Delete all"
            };
            deleteAllBtn.Click += DeleteAllBtn_Click;

            Button addBtn = new()
            {
                Style = (Style)Application.Current.FindResource("MidBtn"),
                Name = "AddBtn",
                Height = 30,
                Width = 45,
                Content = "Add"
            };
            addBtn.Click += AddBtn_Click;

            stackPanel.Children.Add(encryptAllBtn);
            stackPanel.Children.Add(decryptAllBtn);
            stackPanel.Children.Add(deleteAllBtn);
            stackPanel.Children.Add(addBtn);

            ParentGrid.Children.Add(stackPanel);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;

            base.OnSourceInitialized(e);
        }

        private void EncryptAllBtn_Click(object sender, RoutedEventArgs e)
            => EncryptOrDecryptAll(true);

        private void DecryptAllBtn_Click(object sender, RoutedEventArgs e)
            => EncryptOrDecryptAll(false);

        private void DeleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_savedData.Count == 0)
                return;

            const string question = "Are you sure that you wanna permanently delete all the notes?";
            MessageBoxResult result = MessageBox.Show(question, "You sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                FileSystemHelper.ResetFile();
                _savedData.Clear();
                LoadListBox();
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
            => _ = new EditWindow();

        private async void EncryptOrDecryptAll(bool encrypt)
        {
            if (_savedData.Count == 0)
                return;

            Dictionary<string, SavedContent> oldDict = _savedData.ToDictionary(x => x.Key, x => x.Value);
            TaskCompletionSource<string> taskCompletionSource = new();
            EncryptionWindow window = new(true, taskCompletionSource);

            string password = await taskCompletionSource.Task;
            uint failedDecryptions = 0;

            Cursor = Cursors.AppStarting;
            foreach (var pair in oldDict)
            {
                if (pair.Value.IsEncrypted == encrypt)
                    continue;

                
                string? encryptedContent = encrypt 
                    ? await CryptoHelper.EncryptAsync(pair.Value.Content, password)
                    : await CryptoHelper.DecryptAsync(pair.Value.Content, password, true);

                if (encryptedContent == null)
                {
                    failedDecryptions++;
                    continue;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    KeyValuePair<string, SavedContent> newPair = new(pair.Key, new(encryptedContent!, encrypt));
                    ManageSavedData.UpdateData(pair, newPair);
                });
            }

            Cursor = Cursors.Arrow;
            if (failedDecryptions > 0)
            {
                MessageBox.Show($"{failedDecryptions} decryption(s) failed cause the password didn´t match", "Decryption error...", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadListBox()
        {
            LoadedDataList.Items.Clear();
            for (int i = 0; i < _savedData.Count; i++)
            {
                KeyValuePair<string, SavedContent> pair = _savedData.ElementAt(i);

                Grid grid = CreateGrid();
                CreateTextBoxes(grid, pair);
                CreateBtns(grid, pair);

                LoadedDataList.Items.Add(CreateListBoxItem(grid));
            }
        }

        internal void ReloadListBox(Dictionary<string, SavedContent> dictionary)
        {
            _savedData = dictionary;
            LoadListBox();
        }

        private static Grid CreateGrid()
        {
            Grid grid = new()
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            return grid;
        }

        private static void CreateTextBoxes(Grid grid, KeyValuePair<string, SavedContent> pair)
        {
            grid.Children.Add(new TextBlock
            {
                Text = pair.Key,
                FontWeight = FontWeights.Bold,
                TextDecorations = TextDecorations.Underline
            });
            Grid.SetRow(grid.Children[^1], 0);
            Grid.SetColumnSpan(grid.Children[^1], 2);

            grid.Children.Add(new TextBlock
            {
                Text = pair.Value.IsEncrypted 
                    ? "ENCRYPTED!" : pair.Value.Content,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetRow(grid.Children[^1], 1);
            Grid.SetColumn(grid.Children[^1], 0);
        }

        private void CreateBtns(Grid grid, KeyValuePair<string, SavedContent> pair)
        {
            Style style = (Style)Application.Current.FindResource("NoHoverBlueBtn");
            StackPanel buttonPanel = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Button editBtn;
            buttonPanel.Children.Add(editBtn = new Button
            {
                Content = IconHelper.EditBtnIcon,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Margin = new Thickness(0, 0, 10, 0),
                Style = style,
            });
            editBtn.Click += (sender, e) =>
            {
                EditWindow editWindow = new(pair);
            };

            Button deleteBtn;
            buttonPanel.Children.Add(deleteBtn = new Button
            {
                Content = IconHelper.DeleteBtnIcon,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Margin = new Thickness(0, 0, 10, 0),
                Style = style,
            });
            deleteBtn.Click += (sender, e) =>
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete: {pair.Key}?",
                    "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ManageSavedData.DeleteData(pair.Key);
                }
            };

            Image image = pair.Value.IsEncrypted
                ? IconHelper.DecryptBtnIcon
                : IconHelper.EncryptBtnIcon;

            Button encryptBtn;
            buttonPanel.Children.Add(encryptBtn = new Button
            {
                Content = image,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Margin = new Thickness(0, 0, 10, 0),
                Style = style,
            });
            encryptBtn.Click += async (sender, e) =>
            {
                TaskCompletionSource<string> taskCompletionSource = new();
                EncryptionWindow encryptionWindow = new(pair, taskCompletionSource);
                string password = await taskCompletionSource.Task;

                Cursor = Cursors.AppStarting;

                string? content = pair.Value.IsEncrypted 
                    ? await CryptoHelper.DecryptAsync(pair.Value.Content, password)
                    : await CryptoHelper.EncryptAsync(pair.Value.Content, password);

                if (content == null)
                {
                    Cursor = Cursors.Arrow;
                    return;
                }

                KeyValuePair<string, SavedContent> newPair = new(pair.Key, new(content, !pair.Value.IsEncrypted));
                ManageSavedData.UpdateData(pair, newPair);
                LoadListBox();

                Cursor = Cursors.Arrow;
            };

            Button copyBtn;
            buttonPanel.Children.Add(copyBtn = new Button
            {
                Content = IconHelper.CopyBtnIcon,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Margin = new Thickness(0, 0, 10, 0),
                Style = style,
            });
            copyBtn.Click += (sender, e) => Clipboard.SetText(pair.Value.Content);

            Grid.SetRow(buttonPanel, 1);
            Grid.SetColumn(buttonPanel, 1);
            grid.Children.Add(buttonPanel);
        }

        private static ListBoxItem CreateListBoxItem(Grid grid)
        {
            ListBoxItem item = new()
            {
                Content = grid,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            return item;
        }
    }
}