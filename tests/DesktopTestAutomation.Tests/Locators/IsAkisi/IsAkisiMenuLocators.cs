using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.IsAkisi;

/// <summary>
/// Sol menüdeki (ağaç) İş Akışı öğeleri.
/// Her locator'da XPath'ler sırayla denenir: ilk XPath bulunamazsa sonraki (yedek) denenir.
/// XPath'leri FlaUI Inspector'dan alıp buraya ekleyin; testlere dokunmanıza gerek kalmaz.
/// </summary>
public static class IsAkisiMenuLocators
{
    public static readonly Locator IsAkisi = new(
        "İş Akışı menüsü",
        "//Text[@Name='İş Akışı']",
        "/Window[2]/Custom[1]/Custom[1]/Tree/TreeItem[1]"); // Inspector mutlak yolu (masaüstünden) - yedek

    public static readonly Locator IsAkisiTakip = new(
        "İş Akışı Takip menü öğesi",
        "//Text[@Name='İş Akışı Takip']",
        "/Window[2]/Custom[1]/Custom[1]/Tree/TreeItem[1]/TreeItem[1]");

    /// <summary>
    /// Parametreli locator örneği: menüdeki herhangi bir öğeyi adıyla bulur.
    /// Kullanım: Click(IsAkisiMenuLocators.MenuItem("Raporlar"));
    /// </summary>
    public static Locator MenuItem(string name) => new(
        $"'{name}' menü öğesi",
        $"//TreeItem[@Name={Locator.Literal(name)}]",
        $"//Text[@Name={Locator.Literal(name)}]");
}
