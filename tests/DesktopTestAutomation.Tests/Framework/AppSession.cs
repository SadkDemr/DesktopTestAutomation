using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace DesktopTestAutomation.Tests.Framework;

/// <summary>
/// Test altındaki uygulamanın yaşam döngüsünü yönetir (başlatma / bağlanma / kapatma)
/// ve Locator'ları uygulamanın pencerelerinde çözer.
/// </summary>
public sealed class AppSession : IDisposable
{
    private readonly bool _closeOnDispose;

    private AppSession(Application application, UIA3Automation automation, bool wasAttached, bool closeOnDispose)
    {
        Application = application;
        Automation = automation;
        WasAttached = wasAttached;
        _closeOnDispose = closeOnDispose;
    }

    public Application Application { get; }
    public UIA3Automation Automation { get; }

    /// <summary>true: zaten açık olan uygulamaya bağlanıldı; false: uygulama bu oturumda başlatıldı.</summary>
    public bool WasAttached { get; }

    /// <summary>
    /// Uygulamayı başlatır.
    /// <paramref name="reuseRunningApp"/> true ise: uygulama zaten açıksa yeni örnek açmadan ona bağlanır,
    /// kapalıysa bir kez başlatır. Her iki durumda da test sonunda uygulama kapatılmaz; böylece sonraki
    /// testler ve sonraki çalıştırmalar aynı uygulamayı kullanır.
    /// false ise: her seferinde yeni örnek başlatır ve test sonunda kapatır (en temiz, en izole yöntem).
    /// </summary>
    public static AppSession Start(string exePath, TimeSpan startupTimeout, bool reuseRunningApp = false)
    {
        var automation = new UIA3Automation();

        try
        {
            var running = reuseRunningApp ? FindRunningProcess(exePath) : null;
            Application application;

            if (running is not null)
            {
                application = Application.Attach(running);
            }
            else
            {
                var startInfo = new ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    // true: uygulama test sürecinin çıktı bağlantılarını (handle) miras almaz. false olursa,
                    // test sonunda açık bırakılan uygulama bu bağlantıları tuttuğu için 'dotnet test' hiç bitmez.
                    UseShellExecute = true,
                };
                application = Application.Launch(startInfo);
            }

            application.WaitWhileMainHandleIsMissing(startupTimeout);
            return new AppSession(application, automation, wasAttached: running is not null, closeOnDispose: !reuseRunningApp);
        }
        catch
        {
            automation.Dispose();
            throw;
        }
    }

    // ---------------------------------------------------------------- Locator çözümleme

    /// <summary>Locator'ı bir kez dener; bulamazsa null döner (beklemez).</summary>
    /// <param name="root">Verilirse "//" XPath'leri yalnızca bu elementin içinde aranır.</param>
    public AutomationElement? TryFind(Locator locator, AutomationElement? root = null)
    {
        foreach (var xPath in locator.XPaths)
        {
            var found = Locator.IsDesktopPath(xPath)
                ? Automation.GetDesktop().FindAllByXPath(xPath).FirstOrDefault(BelongsToApplication)
                : SearchRoots(root).Select(r => r.FindFirstByXPath(xPath)).FirstOrDefault(e => e is not null);

            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>Locator'ın XPath'lerinden biri eşleşene kadar bekler; zaman aşımında hata fırlatır.</summary>
    public AutomationElement Find(Locator locator, TimeSpan? timeout = null, AutomationElement? root = null)
    {
        return Wait.Until(
            () => TryFind(locator, root),
            timeout ?? TestSettings.Current.DefaultTimeout,
            $"'{locator.Name}' bulunamadı. Denenen XPath'ler:{Environment.NewLine}  " +
            string.Join(Environment.NewLine + "  ", locator.XPaths));
    }

    /// <summary>Element verilen süre içinde görünürse true döner.</summary>
    public bool Exists(Locator locator, TimeSpan timeout, AutomationElement? root = null)
    {
        try
        {
            Find(locator, timeout, root);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    private IEnumerable<AutomationElement> SearchRoots(AutomationElement? root) =>
        root is not null ? [root] : Application.GetAllTopLevelWindows(Automation);

    private bool BelongsToApplication(AutomationElement element) =>
        element.Properties.ProcessId.ValueOrDefault == Application.ProcessId;

    // ---------------------------------------------------------------- Kapatma

    public void Dispose()
    {
        try
        {
            if (_closeOnDispose)
            {
                CloseGracefully(TimeSpan.FromSeconds(5));
            }
        }
        finally
        {
            Application.Dispose();
            Automation.Dispose();
        }
    }

    /// <summary>
    /// Açık pencereleri kapatarak uygulamanın normal şekilde çıkmasını bekler; olmazsa process'i sonlandırır.
    /// Not: FlaUI'nin Application.Close() metodu yalnızca açılıştaki ana pencereyi kapatır. Login → ana pencere
    /// gibi pencere değişen uygulamalarda o pencere zaten kapanmış olduğundan burada o an açık olan pencereler kapatılır.
    /// </summary>
    private void CloseGracefully(TimeSpan timeout)
    {
        if (Application.HasExited)
        {
            return;
        }

        try
        {
            foreach (var window in Application.GetAllTopLevelWindows(Automation))
            {
                window.Close();
            }

            using var process = Process.GetProcessById(Application.ProcessId);
            if (process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                return;
            }
        }
        catch
        {
            // Pencere kapanırken UIA hataları olabilir; aşağıda process sonlandırılır.
        }

        if (!Application.HasExited)
        {
            Application.Kill();
        }
    }

    /// <summary>
    /// Açık uygulamayı bulur: önce exe yolu birebir aynı olan process, yoksa aynı isimli ilk process
    /// (uygulama farklı bir kısayoldan / kopyadan açılmış olabilir).
    /// </summary>
    private static Process? FindRunningProcess(string exePath)
    {
        var candidates = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exePath))
            .Where(p => !p.HasExited)
            .ToList();

        var match = candidates.FirstOrDefault(p => PathEquals(p, exePath)) ?? candidates.FirstOrDefault();

        foreach (var other in candidates.Where(p => p != match))
        {
            other.Dispose();
        }

        return match;
    }

    private static bool PathEquals(Process process, string exePath)
    {
        try
        {
            return string.Equals(process.MainModule?.FileName, exePath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false; // Erişim izni olmayan process'ler
        }
    }
}
