using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SampleWpfApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Customer> _customers =
    [
        new Customer { Name = "Ayşe Yılmaz", Email = "ayse@example.com", City = "İstanbul", IsActive = true },
        new Customer { Name = "Mehmet Kaya", Email = "mehmet@example.com", City = "Ankara", IsActive = false },
    ];

    public MainWindow(string username)
    {
        InitializeComponent();
        WelcomeText.Text = $"Hoş geldin, {username}";
        CustomersGrid.ItemsSource = _customers;
        UpdateCount();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var email = EmailTextBox.Text.Trim();

        if (name.Length == 0)
        {
            StatusText.Text = "Ad Soyad alanı zorunludur.";
            return;
        }

        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
        {
            StatusText.Text = "Geçerli bir e-posta adresi girin.";
            return;
        }

        _customers.Add(new Customer
        {
            Name = name,
            Email = email,
            City = (CityComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty,
            IsActive = IsActiveCheckBox.IsChecked == true,
        });

        StatusText.Text = $"Müşteri eklendi: {name}";
        NameTextBox.Clear();
        EmailTextBox.Clear();
        UpdateCount();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomersGrid.SelectedItem is not Customer selected)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"'{selected.Name}' silinsin mi?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
        {
            StatusText.Text = "Silme işlemi iptal edildi.";
            return;
        }

        _customers.Remove(selected);
        StatusText.Text = $"Müşteri silindi: {selected.Name}";
        UpdateCount();
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DeleteButton.IsEnabled = CustomersGrid.SelectedItem is Customer;
    }

    private void UpdateCount()
    {
        CustomerCountText.Text = $"Toplam müşteri: {_customers.Count}";
    }
}
