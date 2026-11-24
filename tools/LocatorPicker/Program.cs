using System.Drawing;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.UIA3;

namespace LocatorPicker;

/// <summary>
/// Fareyle gösterilen element için sağlam XPath'ler üretir ve test projesindeki Locator formatında
/// C# kodunu panoya kopyalar.
///
///   Ctrl+Shift+X : imlecin altındaki kontrolü al (buton/menü içindeki yazıya gelinse de kontrolün kendisi)
///   Ctrl+Shift+Z : tam olarak imlecin altındaki elementi al (yazının kendisi gibi)
///   Ctrl+C       : çıkış
///
/// Komut satırı:
///   LocatorPicker.exe --test "//Button[@Name='Filtrele']"   XPath'i çalışan uygulamalarda dener
///   LocatorPicker.exe --point X Y [--exact]                  verilen ekran koordinatındaki elementi alır
/// </summary>
internal static class Program
{
    private const int PickHotkeyId = 1;
    private const int PickExactHotkeyId = 2;
    private const uint VkX = 0x58;
    private const uint VkZ = 0x5A;

    private static readonly string OutputFile = Path.Combine(Environment.CurrentDirectory, "picked-locators.txt");

    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using var automation = new UIA3Automation();
        var generator = new LocatorGenerator(automation);

        if (args.Length >= 3 && args[0] == "--point")
        {
            var point = new Point(int.Parse(args[1]), int.Parse(args[2]));
            Pick(automation, generator, point, exact: args.Contains("--exact"));
            return 0;
        }

        if (args.Length >= 2 && args[0] == "--test")
        {
            TestXPath(automation, args[1]);
            return 0;
        }

        const uint modifiers = NativeMethods.ModControl | NativeMethods.ModShift | NativeMethods.ModNoRepeat;
        if (!NativeMethods.RegisterHotKey(IntPtr.Zero, PickHotkeyId, modifiers, VkX) ||
            !NativeMethods.RegisterHotKey(IntPtr.Zero, PickExactHotkeyId, modifiers, VkZ))
        {
            Console.Error.WriteLine("Kısayol tuşları kaydedilemedi (başka bir uygulama kullanıyor olabilir).");
            return 1;
        }

        Console.WriteLine("LocatorPicker hazır.");
        Console.WriteLine("  Fareyi elementin üzerine getirin ve Ctrl+Shift+X'e basın.");
        Console.WriteLine("  (Buton/menü içindeki yazıya gelseniz de kontrolün kendisi alınır.");
        Console.WriteLine("   Yazının kendisini almak için Ctrl+Shift+Z.)");
        Console.WriteLine("  Çıkmak için bu pencerede Ctrl+C.");
        Console.WriteLine($"  Sonuçlar panoya kopyalanır ve şu dosyaya eklenir: {OutputFile}");
        Console.WriteLine();

        while (NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
        {
            if (msg.Message == NativeMethods.WmHotkey)
            {
                var exact = msg.WParam.ToInt32() == PickExactHotkeyId;
                Pick(automation, generator, Mouse.Position, exact);
            }
        }

        return 0;
    }

    private static void Pick(UIA3Automation automation, LocatorGenerator generator, Point point, bool exact)
    {
        try
        {
            var element = automation.FromPoint(point);
            if (!exact)
            {
                element = PromoteToControl(automation, element);
            }

            if (element.Properties.ProcessId.ValueOrDefault == Environment.ProcessId)
            {
                Console.WriteLine("LocatorPicker'ın kendi penceresi seçildi; hedef uygulamanın üzerine gelin.");
                return;
            }

            // Önce XPath'ler değerlendirilir, sonra vurgulanır: vurgu, masaüstünde geçici bir pencere açar ve
            // değerlendirme sırasında açıksa masaüstünden başlayan XPath'lerin sonucunu bozar.
            var result = generator.Generate(element);
            Highlight(element);
            Print(result);

            System.Windows.Forms.Clipboard.SetText(result.Snippet);
            File.AppendAllText(OutputFile,
                $"// {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Pencere: {result.WindowTitle}\n{result.Snippet}\n",
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Element alınamadı: {ex.Message}");
        }
    }

    /// <summary>
    /// Buton, menü öğesi, ağaç öğesi gibi kontrollerin içindeki metin (Text) seçildiyse kontrolün kendisini döndürür.
    /// Kullanıcı neredeyse her zaman metni değil, tıklanacak kontrolü ister.
    /// </summary>
    private static AutomationElement PromoteToControl(UIA3Automation automation, AutomationElement element)
    {
        if (element.Properties.ControlType.ValueOrDefault != ControlType.Text)
        {
            return element;
        }

        var parent = automation.TreeWalkerFactory.GetControlViewWalker().GetParent(element);
        var parentType = parent?.Properties.ControlType.ValueOrDefault;

        return parentType is ControlType.Button or ControlType.MenuItem or ControlType.TreeItem
            or ControlType.ListItem or ControlType.TabItem or ControlType.Hyperlink
            or ControlType.CheckBox or ControlType.RadioButton or ControlType.SplitButton
            ? parent!
            : element;
    }

    /// <summary>Bir XPath'i çalışan uygulamalarda dener ve eşleşen elementleri listeler.</summary>
    private static void TestXPath(UIA3Automation automation, string xPath)
    {
        var desktop = automation.GetDesktop();
        var isDesktopPath = xPath.StartsWith('/') && !xPath.StartsWith("//");

        // "//" XPath'leri test framework'ündeki gibi pencerelerin içinde aranır.
        var roots = isDesktopPath ? [desktop] : desktop.FindAllChildren();
        var matches = new List<AutomationElement>();

        foreach (var root in roots)
        {
            try
            {
                matches.AddRange(root.FindAllByXPath(xPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geçersiz XPath: {ex.Message}");
                return;
            }
        }

        Console.WriteLine($"{matches.Count} eşleşme: {xPath}");
        foreach (var m in matches.Take(20))
        {
            Console.WriteLine($"  {m.Properties.ControlType.ValueOrDefault,-12} Name='{m.Properties.Name.ValueOrDefault}' " +
                              $"AutomationId='{m.Properties.AutomationId.ValueOrDefault}' PID={m.Properties.ProcessId.ValueOrDefault}");
            Highlight(m);
        }
    }

    private static void Highlight(AutomationElement element)
    {
        try
        {
            element.DrawHighlight(false, Color.OrangeRed, TimeSpan.FromSeconds(1));
        }
        catch
        {
            // Vurgulama isteğe bağlı
        }
    }

    private static void Print(PickResult result)
    {
        Console.WriteLine(new string('─', 80));
        Console.WriteLine($"Element : {result.ControlType}  Name='{result.Name}'  AutomationId='{result.AutomationId}'");
        Console.WriteLine($"Pencere : {result.WindowTitle}");
        Console.WriteLine("Adaylar :");

        foreach (var c in result.Candidates)
        {
            var status = c.IsUnique ? "✓ tekil"
                : c.Error is not null ? "✗ hata"
                : c.PointsToElement ? $"✗ {c.MatchCount} eşleşme"
                : c.MatchCount > 0 ? $"✗ başka {c.MatchCount} el."
                : "✗ eşleşmedi";
            Console.WriteLine($"  {status,-14} [{c.Kind}] {c.XPath}");
            if (c.Error is not null) Console.WriteLine($"                 {c.Error}");
        }

        Console.WriteLine();
        Console.WriteLine("Panoya kopyalandı:");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(result.Snippet);
        Console.ResetColor();
        Console.WriteLine();
    }
}
