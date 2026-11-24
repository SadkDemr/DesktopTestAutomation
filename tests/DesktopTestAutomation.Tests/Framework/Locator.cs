namespace DesktopTestAutomation.Tests.Framework;

/// <summary>
/// Bir UI elementinin adı ve onu bulmak için denenecek XPath'ler (sırayla, ilk bulunan kazanır).
/// Locator dosyalarında (Locators klasörü) tanımlanır, testlerde ve sayfa sınıflarında kullanılır.
///
/// XPath kuralları:
///  - "//" ile başlayan XPath'ler uygulamanın açık pencerelerinin içinde aranır.
///    Örnek: //Button[@Name='Filtrele'], //Edit[@AutomationId='UsernameTextBox']
///  - Tek "/" ile başlayan XPath'ler MASAÜSTÜNDEN itibaren aranır. FlaUI Inspector'ın
///    kopyaladığı mutlak yollar bu biçimdedir. Örnek: /Window[2]/Custom[1]/Tree/TreeItem[1]
///    Uyarı: Window[2] gibi index'ler masaüstündeki pencere sırasına bağlıdır ve kolay kırılır.
///    Mümkünse Name veya AutomationId içeren "//" XPath'leri tercih edin, mutlak yolu yedek olarak tutun.
/// </summary>
public sealed class Locator
{
    public Locator(string name, params string[] xPaths)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Locator adı boş olamaz.", nameof(name));
        }

        if (xPaths.Length == 0 || xPaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException($"'{name}' locator'ı için en az bir geçerli XPath verilmeli.", nameof(xPaths));
        }

        Name = name;
        XPaths = xPaths;
    }

    /// <summary>Log ve hata mesajlarında görünen okunabilir ad.</summary>
    public string Name { get; }

    public IReadOnlyList<string> XPaths { get; }

    public override string ToString() => Name;

    /// <summary>
    /// Parametreli locator'larda değeri XPath'e güvenle gömmek için kullanılır.
    /// Tırnak işareti içeren metinleri ("Müşteri'nin kaydı") doğru şekilde kaçırır.
    /// </summary>
    public static string Literal(string value)
    {
        if (!value.Contains('\''))
        {
            return $"'{value}'";
        }

        if (!value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        return "concat('" + value.Replace("'", "',\"'\",'") + "')";
    }

    internal static bool IsDesktopPath(string xPath) => xPath.StartsWith('/') && !xPath.StartsWith("//");
}
