using System.Windows;

namespace SampleWpfApp;

public partial class LoginWindow : Window
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "admin123";

    public LoginWindow()
    {
        InitializeComponent();
        UsernameTextBox.Focus();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (username.Length == 0 || password.Length == 0)
        {
            LoginErrorText.Text = "Kullanıcı adı ve şifre zorunludur.";
            return;
        }

        if (username != ValidUsername || password != ValidPassword)
        {
            LoginErrorText.Text = "Kullanıcı adı veya şifre hatalı.";
            return;
        }

        var mainWindow = new MainWindow(username);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }
}
