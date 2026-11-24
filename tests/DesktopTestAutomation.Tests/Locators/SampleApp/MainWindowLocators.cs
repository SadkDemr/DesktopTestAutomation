using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.SampleApp;

/// <summary>Örnek uygulamanın müşteri yönetimi (ana) penceresi.</summary>
public static class MainWindowLocators
{
    public static readonly Locator Window = new("Ana pencere", "/Window[@AutomationId='MainWindow']");

    public static readonly Locator WelcomeText = new("Hoş geldin mesajı", "//Text[@AutomationId='WelcomeText']");
    public static readonly Locator StatusText = new("Durum mesajı", "//Text[@AutomationId='StatusText']");
    public static readonly Locator CustomerCountText = new("Müşteri sayısı", "//Text[@AutomationId='CustomerCountText']");

    public static readonly Locator NameTextBox = new("Ad Soyad", "//Edit[@AutomationId='NameTextBox']");
    public static readonly Locator EmailTextBox = new("E-posta", "//Edit[@AutomationId='EmailTextBox']");
    public static readonly Locator CityComboBox = new("Şehir", "//ComboBox[@AutomationId='CityComboBox']");
    public static readonly Locator IsActiveCheckBox = new("Aktif müşteri", "//CheckBox[@AutomationId='IsActiveCheckBox']");

    public static readonly Locator AddButton = new("Ekle butonu", "//Button[@AutomationId='AddButton']");
    public static readonly Locator DeleteButton = new("Seçiliyi sil butonu", "//Button[@AutomationId='DeleteButton']");
    public static readonly Locator CustomersGrid = new("Müşteri listesi", "//DataGrid[@AutomationId='CustomersGrid']");
}
