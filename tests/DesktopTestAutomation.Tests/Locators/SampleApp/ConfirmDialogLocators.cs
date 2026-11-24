using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.SampleApp;

/// <summary>
/// Standart Windows MessageBox. Buton metinleri OS diline göre değişir ("Yes" / "Evet"),
/// bu yüzden dilden bağımsız Win32 kontrol Id'leri kullanılır: OK=1, Cancel=2, Yes=6, No=7.
/// </summary>
public static class ConfirmDialogLocators
{
    // Yedekli locator örneği: önce masaüstünde üst seviye pencere olarak, bulamazsa
    // sahibi olan pencerenin içinde aranır.
    public static readonly Locator Window = new(
        "Onay penceresi",
        "/Window[@Name='Onay']",
        "//Window[@Name='Onay']");

    public static readonly Locator YesButton = new("Evet butonu", "//Button[@AutomationId='6']");
    public static readonly Locator NoButton = new("Hayır butonu", "//Button[@AutomationId='7']");

     public static readonly Locator UsernameTextBoxEdit = new(
        "UsernameTextBox (Edit)",
        "//Edit[@AutomationId='UsernameTextBox']",
    "/Window[@AutomationId='LoginWindow']/Edit[1]");

        public static readonly Locator GirisYapButton = new(
        "Giriş yap (Button)",
        "//Button[@AutomationId='LoginButton']",
        "//Button[@Name='Giriş yap']",
        "/Window[@AutomationId='LoginWindow']/Button");


}
