namespace DesktopTestAutomation.Tests;

/// <summary>Testlerde kullanılan sabit veriler (örnek uygulamanın demo kullanıcısı).</summary>
public static class TestData
{
    public const string ValidUsername = "admin";
    public const string ValidPassword = "admin123";

    /// <summary>Örnek uygulama açılışta bu müşterilerle gelir.</summary>
    public static readonly IReadOnlyList<string> SeedCustomerNames = ["Ayşe Yılmaz", "Mehmet Kaya"];
}
