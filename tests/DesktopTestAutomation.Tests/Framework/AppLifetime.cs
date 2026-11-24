namespace DesktopTestAutomation.Tests.Framework;

/// <summary>Uygulamanın test fixture'ı içinde ne zaman açılıp kapanacağı.</summary>
public enum AppLifetime
{
    /// <summary>Her test kendi temiz uygulama örneğiyle çalışır (varsayılan, en izole yöntem).</summary>
    PerTest,

    /// <summary>
    /// Uygulama fixture başında bir kez açılır, tüm testler aynı oturumda sırayla çalışır.
    /// Adım adım ilerleyen senaryolar için uygundur; sırayı [Order] ile belirleyin.
    /// </summary>
    PerFixture,
}
