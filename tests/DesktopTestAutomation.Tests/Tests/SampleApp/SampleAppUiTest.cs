using DesktopTestAutomation.Tests.Framework;
using DesktopTestAutomation.Tests.Locators.SampleApp;
using FlaUI.Core.AutomationElements;

namespace DesktopTestAutomation.Tests.Tests.SampleApp;

/// <summary>
/// Örnek uygulama testlerinin temel sınıfı. appsettings.json'daki AppPath yerine solution içindeki
/// SampleWpfApp'i kullanır ve birden fazla testte tekrar eden adımları (giriş, liste okuma) içerir.
/// </summary>
public abstract class SampleAppUiTest : BaseUiTest
{
    protected override string? ResolveAppPath() => TestSettings.ResolveSampleAppPath();

    protected override bool AttachToRunningApp => false;

    // ---------------------------------------------------------------- Tekrar kullanılan adımlar

    protected void EnterCredentials(string username, string password)
    {
        Find(LoginLocators.Window, Settings.StartupTimeout);
        SetText(LoginLocators.UsernameTextBox, username);

        // PasswordBox'a değer atanamaz; klavyeyle yazılır.
        if (password.Length > 0)
        {
            TypeText(LoginLocators.PasswordBox, password);
        }
    }

    protected void LoginAs(string username, string password)
    {
        EnterCredentials(username, password);
        Invoke(LoginLocators.LoginButton);
        Find(MainWindowLocators.Window);
    }

    protected IReadOnlyList<CustomerRow> GetCustomers()
    {
        return Find(MainWindowLocators.CustomersGrid).AsGrid().Rows
            .Select(row => row.Cells)
            .Select(cells => new CustomerRow(
                Name: cells[0].GetText(),
                Email: cells[1].GetText(),
                City: cells[2].GetText(),
                Active: cells[3].GetText()))
            .ToList();
    }

    protected void SelectCustomer(string name)
    {
        var row = Find(MainWindowLocators.CustomersGrid).AsGrid().Rows
                      .FirstOrDefault(r => r.Cells[0].GetText() == name)
                  ?? throw new InvalidOperationException($"'{name}' adlı müşteri listede yok.");

        row.Patterns.SelectionItem.Pattern.Select();
        Wait.Until(() => Find(MainWindowLocators.DeleteButton).IsEnabled, Settings.DefaultTimeout, "Sil butonu aktif olmadı");
        Log($"Seçildi: {name}");
    }
}

/// <summary>Giriş yapılmış ana pencereyle başlayan testler.</summary>
public abstract class LoggedInSampleAppUiTest : SampleAppUiTest
{
    [SetUp]
    public void LogIn() => LoginAs(TestData.ValidUsername, TestData.ValidPassword);
}

public sealed record CustomerRow(string Name, string Email, string City, string Active);
