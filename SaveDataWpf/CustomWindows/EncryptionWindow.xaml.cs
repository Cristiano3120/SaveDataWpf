using System.Windows;

namespace SaveDataWpf.CustomWindows
{
    public partial class EncryptionWindow : Window
    {
        private readonly TaskCompletionSource<string> _enteredPasswordTcs;

        internal EncryptionWindow(bool encrypting, TaskCompletionSource<string> enteredPasswordTcs)
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, ParentGrid);
            Show();

            Owner = Application.Current.MainWindow;
            string prefix = encrypting
                ? "Encrypting all..."
                : "Decrypting all...";
            Title = $"{prefix}";
            _enteredPasswordTcs = enteredPasswordTcs;
            SaveBtn.Content = prefix[..7];
            SaveBtn.Click += SaveBtn_Click;
        }

        internal EncryptionWindow(KeyValuePair<string, SavedContent> pair, TaskCompletionSource<string> enteredPasswordTcs)
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, ParentGrid);
            Show();

            Owner = Application.Current.MainWindow;
            string prefix = pair.Value.IsEncrypted
                ? "Decrypt" 
                : "Encrypt";
            Title = $"{prefix}: {pair.Key}";
            _enteredPasswordTcs = enteredPasswordTcs;

            SaveBtn.Content = prefix;
            SaveBtn.Click += SaveBtn_Click;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordTextBox.Text.Trim();
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _enteredPasswordTcs.SetResult(password);
            Close();
        }
    }
}
