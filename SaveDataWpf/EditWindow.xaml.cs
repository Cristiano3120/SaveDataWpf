using System.Windows;

namespace SaveDataWpf
{
    /// <summary>
    /// Only needs to be instanciated. Closes when work is done.
    /// Constructor without params for adding.
    /// Constructor with params for editing
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly KeyValuePair<string, SavedContent> _pair;

        internal EditWindow()
        {
            InitializeComponent();
            GUIHelper.RegisterWindowButtons(MinimizeBtn, MaximizeBtn, CloseBtn);
            Owner = Application.Current.MainWindow;
            Title = $"Adding";

            SaveBtn.Click += SaveBtn_Click;
            Show();
        }

        internal EditWindow(KeyValuePair<string, SavedContent> pair)
        {
            InitializeComponent();
            GUIHelper.RegisterWindowButtons(MinimizeBtn, MaximizeBtn, CloseBtn);

            if (pair.Value.IsEncrypted)
            {
                MessageBox.Show("This content is encrypted. Please decrypt it before editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _pair = pair;
            Owner = Application.Current.MainWindow;
            Title = $"Edit: {pair.Key}";
            
            SaveBtn.Click += SaveBtn_Click;
            NameTextBox.Text = pair.Key;

            Show();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string content = ContentTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Name and content cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            KeyValuePair<string, SavedContent> newPair = new(name, new(content, false));
            ManageSavedData.UpdateData(oldPair: null, newPair);
            Close();
        }
    }
}
