using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.IsAkisi;

/// <summary>"İş Akışı Takip" ekranındaki elementler.</summary>
public static class IsAkisiTakipLocators
{
    // TODO: Butonun gerçek adını / XPath'ini FlaUI Inspector'dan alıp güncelleyin.
    public static readonly Locator FiltreleButonu = new(
        "Filtrele butonu",
        "//Button[@Name='Filtrele']");
        // Yedek eklemek için: "/Window[2]/Custom[2]/Button[1]"
}
