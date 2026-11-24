using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.SampleApp;

/// <summary>Örnek uygulamanın giriş penceresi.</summary>
public static class LoginLocators
{
    // Tek "/" ile başladığı için masaüstünden aranır (üst seviye pencere).
    public static readonly Locator Window = new("Giriş penceresi", "/Window[@AutomationId='LoginWindow']");

    public static readonly Locator UsernameTextBox = new("Kullanıcı adı", "//Edit[@AutomationId='UsernameTextBox']");
    public static readonly Locator PasswordBox = new("Şifre", "//Edit[@AutomationId='PasswordBox']");
    public static readonly Locator LoginButton = new("Giriş yap butonu", "//Button[@AutomationId='LoginButton']");
    public static readonly Locator ErrorText = new("Giriş hata mesajı", "//Text[@AutomationId='LoginErrorText']");
}
