using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Input;
using NUnit.Framework.Interfaces;

namespace DesktopTestAutomation.Tests.Framework;

/// <summary>
/// Tüm UI testlerinin temel sınıfı.
///  - Uygulamayı açar/kapatır (<see cref="Lifetime"/> ile test başına veya fixture başına).
///  - Test başarısız olursa ekran görüntüsü alıp test sonucuna ekler.
///  - Locator dosyalarındaki XPath'lerle çalışan adım metotlarını sağlar: Click, SetText, GetText...
///    Bu metotlar elementi bulana kadar bekler; Thread.Sleep kullanmaya gerek yoktur.
/// </summary>
public abstract class BaseUiTest
{
    protected static TestSettings Settings => TestSettings.Current;

    protected AppSession Session { get; private set; } = null!;

    /// <summary>Varsayılan: her test temiz bir uygulama örneğiyle başlar.</summary>
    protected virtual AppLifetime Lifetime => AppLifetime.PerTest;

    /// <summary>Test edilecek exe. Varsayılan olarak appsettings.json'daki AppPath kullanılır.</summary>
    protected virtual string? ResolveAppPath() => Settings.ResolveAppPath();

    /// <summary>
    /// true ise uygulama açıksa ona bağlanılır, kapalıysa bir kez açılır ve test sonunda açık bırakılır
    /// (appsettings.json: AttachToRunningApp).
    /// </summary>
    protected virtual bool AttachToRunningApp => Settings.AttachToRunningApp;

    // ---------------------------------------------------------------- Yaşam döngüsü

    [OneTimeSetUp]
    public void StartApplicationForFixture()
    {
        if (Lifetime == AppLifetime.PerFixture)
        {
            StartApplication();
        }
    }

    [SetUp]
    public void StartApplicationForTest()
    {
        if (Lifetime == AppLifetime.PerTest)
        {
            StartApplication();
        }
    }

    [TearDown]
    public void AfterEachTest()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                TakeScreenshot();
            }
        }
        finally
        {
            if (Lifetime == AppLifetime.PerTest)
            {
                Session?.Dispose();
            }
        }
    }

    [OneTimeTearDown]
    public void StopApplicationForFixture()
    {
        if (Lifetime == AppLifetime.PerFixture)
        {
            Session?.Dispose();
        }
    }

    private void StartApplication()
    {
        var appPath = ResolveAppPath();

        if (appPath is null)
        {
            Assert.Ignore(
                $"Test edilecek uygulama ayarlanmadı. appsettings.json içindeki AppPath değerini " +
                $"veya {TestSettings.AppPathEnvironmentVariable} ortam değişkenini doldurun.");
        }

        Session = AppSession.Start(appPath, Settings.StartupTimeout, AttachToRunningApp);
        Log(Session.WasAttached
            ? $"Açık uygulamaya bağlanıldı (PID {Session.Application.ProcessId})"
            : $"Uygulama başlatıldı (PID {Session.Application.ProcessId})");
    }

    // ---------------------------------------------------------------- Senaryo adımları

    /// <summary>Elementi bulur (bulana kadar bekler).</summary>
    protected AutomationElement Find(Locator locator, TimeSpan? timeout = null) => Session.Find(locator, timeout);

    /// <summary>Element verilen süre içinde (varsayılan: hemen) görünüyor mu?</summary>
    protected bool Exists(Locator locator, TimeSpan? timeout = null) =>
        Session.Exists(locator, timeout ?? TimeSpan.Zero);

    /// <summary>Elementi bulur, aktif olmasını bekler ve fareyle tıklar.</summary>
    protected AutomationElement Click(Locator locator)
    {
        var element = FindEnabled(locator);
        element.Click();
        Log($"Tıklandı: {locator.Name}");
        return element;
    }

    /// <summary>Elementi bulur ve fareyle çift tıklar.</summary>
    protected AutomationElement DoubleClick(Locator locator)
    {
        var element = FindEnabled(locator);
        element.DoubleClick();
        Log($"Çift tıklandı: {locator.Name}");
        return element;
    }

    /// <summary>
    /// Butonu fare kullanmadan (InvokePattern ile) tetikler. Ekranın görünür olmasını gerektirmediği için
    /// butonlarda Click'ten daha kararlıdır.
    /// </summary>
    protected void Invoke(Locator locator)
    {
        FindEnabled(locator).AsButton().Invoke();
        Log($"Tetiklendi: {locator.Name}");
    }

    /// <summary>TextBox değerini doğrudan atar (hızlı, klavye düzeninden bağımsız).</summary>
    protected void SetText(Locator locator, string text)
    {
        FindEnabled(locator).AsTextBox().Text = text;
        Log($"Yazıldı: {locator.Name} = '{text}'");
    }

    /// <summary>Elemente odaklanıp klavyeyle yazar (PasswordBox gibi değer atanamayan alanlar için).</summary>
    protected void TypeText(Locator locator, string text)
    {
        FindEnabled(locator).Focus();
        Keyboard.Type(text);
        FlaUI.Core.Input.Wait.UntilInputIsProcessed();
        Log($"Klavyeyle yazıldı: {locator.Name}");
    }

    /// <summary>ComboBox'ta görünen metne göre seçim yapar.</summary>
    protected void Select(Locator locator, string itemText)
    {
        FindEnabled(locator).AsComboBox().Select(itemText);
        Log($"Seçildi: {locator.Name} = '{itemText}'");
    }

    /// <summary>CheckBox'ı işaretler veya işaretini kaldırır.</summary>
    protected void SetChecked(Locator locator, bool isChecked)
    {
        FindEnabled(locator).AsCheckBox().IsChecked = isChecked;
        Log($"{(isChecked ? "İşaretlendi" : "İşaret kaldırıldı")}: {locator.Name}");
    }

    /// <summary>Elementin kullanıcıya görünen metnini döndürür.</summary>
    protected string GetText(Locator locator) => Find(locator).GetText();

    /// <summary>Element aktif (tıklanabilir) mi? Element bulunamazsa hata verir.</summary>
    protected bool IsEnabled(Locator locator) => Find(locator).IsEnabled;

    /// <summary>CheckBox işaretli mi? Element bulunamazsa hata verir.</summary>
    protected bool IsChecked(Locator locator) => Find(locator).AsCheckBox().IsChecked == true;

    /// <summary>Elementin metni beklenen değere gelene kadar bekler.</summary>
    protected void WaitForText(Locator locator, string expected, TimeSpan? timeout = null)
    {
        Wait.Until(() => Session.TryFind(locator)?.GetText() == expected,
            timeout ?? Settings.DefaultTimeout,
            $"'{locator.Name}' metni '{expected}' olmadı");
    }

    /// <summary>Element ekrandan kaybolana kadar bekler (yükleniyor göstergesi, kapanan pencere vb.).</summary>
    protected void WaitUntilGone(Locator locator, TimeSpan? timeout = null)
    {
        Wait.Until(() => Session.TryFind(locator) is null,
            timeout ?? Settings.DefaultTimeout,
            $"'{locator.Name}' kaybolmadı");
    }

    protected static void Log(string message) => TestContext.Out.WriteLine($"→ {message}");

    private AutomationElement FindEnabled(Locator locator)
    {
        var element = Find(locator);
        Wait.Until(() => element.IsEnabled, Settings.DefaultTimeout, $"'{locator.Name}' aktif hale gelmedi");
        return element;
    }

    // ---------------------------------------------------------------- Ekran görüntüsü

    private static void TakeScreenshot()
    {
        try
        {
            var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, Settings.ScreenshotDirectory);
            Directory.CreateDirectory(directory);

            var safeName = string.Concat(TestContext.CurrentContext.Test.Name.Select(
                c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var path = Path.Combine(directory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            Capture.Screen().ToFile(path);
            TestContext.AddTestAttachment(path, "Hata anındaki ekran görüntüsü");
            TestContext.Out.WriteLine($"Ekran görüntüsü: {path}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Ekran görüntüsü alınamadı: {ex.Message}");
        }
    }
}
