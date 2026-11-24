using System.Globalization;
using System.Text;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace LocatorPicker;

public sealed record XPathCandidate(string XPath, string Kind, int MatchCount, bool PointsToElement, string? Error = null)
{
    /// <summary>XPath tam olarak bir elemente ve o da seçilen elemente denk geliyor mu?</summary>
    public bool IsUnique => MatchCount == 1 && PointsToElement;
}

public sealed record PickResult(
    string ControlType,
    string Name,
    string AutomationId,
    string WindowTitle,
    IReadOnlyList<XPathCandidate> Candidates,
    string Snippet);

/// <summary>
/// Bir UI elementi için sağlamlık sırasına göre XPath adayları üretir, her adayı uygulamada
/// çalıştırıp tekil olup olmadığını doğrular ve test projesindeki Locator formatında C# kodu oluşturur.
/// </summary>
public sealed class LocatorGenerator(AutomationBase automation)
{
    private const int MaxXPathsInSnippet = 3;

    private readonly AutomationElement _desktop = automation.GetDesktop();

    public PickResult Generate(AutomationElement element)
    {
        var window = FindTopLevelWindow(element);
        var type = ControlTypeOf(element);
        var name = element.Properties.Name.ValueOrDefault ?? string.Empty;
        var id = element.Properties.AutomationId.ValueOrDefault ?? string.Empty;

        var relative = new List<(string XPath, string Kind)>();

        // 1) En sağlam: AutomationId, 2) görünen ad, 3) ikisi birlikte
        if (id.Length > 0) relative.Add(($"//{type}[@AutomationId={Literal(id)}]", "AutomationId"));
        if (name.Length > 0) relative.Add(($"//{type}[@Name={Literal(name)}]", "Name"));
        if (id.Length > 0 && name.Length > 0)
            relative.Add(($"//{type}[@AutomationId={Literal(id)} and @Name={Literal(name)}]", "AutomationId+Name"));

        // 4) AutomationId'si olan en yakın üst elemana göre (ör. bir panel / grid içindeki buton)
        var anchor = NearestAncestorWithAutomationId(element, window);
        if (anchor is not null)
        {
            var anchorPath = $"//{ControlTypeOf(anchor)}[@AutomationId={Literal(anchor.Properties.AutomationId.Value)}]";
            if (name.Length > 0) relative.Add(($"{anchorPath}//{type}[@Name={Literal(name)}]", "Üst eleman + Name"));

            var index = IndexAmong(anchor, $".//{type}", element);
            if (index > 0) relative.Add(($"({anchorPath}//{type})[{index}]", "Üst eleman + sıra"));
        }

        // 5) Üst elemanın adına göre (ör. ağaçta "İş Akışı" altındaki "İş Akışı Takip")
        var parent = Parent(element);
        var parentName = parent?.Properties.Name.ValueOrDefault ?? string.Empty;
        if (parent is not null && !parent.Equals(window) && parentName.Length > 0 && name.Length > 0)
            relative.Add(($"//{ControlTypeOf(parent)}[@Name={Literal(parentName)}]/{type}[@Name={Literal(name)}]", "Üst ad + Name"));

        var candidates = relative
            .DistinctBy(c => c.XPath)
            .Select(c => Evaluate(window, c.XPath, c.Kind, element))
            .ToList();

        // 6) Yapısal yol: pencereyi Id/ad ile sabitleyip içini sıra numarasıyla tarif eder (masaüstünden)
        var structural = StructuralPath(element, window, anchorWindow: true);
        if (structural is not null) candidates.Add(Evaluate(_desktop, structural, "Pencere + yapısal yol", element));

        // 7) FlaUInspect'in ürettiği tamamen sıra numaralı yol (en kırılgan, son çare)
        var fullIndex = StructuralPath(element, window, anchorWindow: false);
        if (fullIndex is not null && fullIndex != structural)
            candidates.Add(Evaluate(_desktop, fullIndex, "Tam sıra numaralı yol", element));

        var snippet = BuildSnippet(type, name, id, candidates);
        return new PickResult(type, name, id, window.Properties.Name.ValueOrDefault ?? string.Empty, candidates, snippet);
    }

    // ---------------------------------------------------------------- Doğrulama

    private static XPathCandidate Evaluate(AutomationElement root, string xPath, string kind, AutomationElement target)
    {
        try
        {
            var matches = root.FindAllByXPath(xPath);
            return new XPathCandidate(xPath, kind, matches.Length, matches.Any(m => m.Equals(target)));
        }
        catch (Exception ex)
        {
            return new XPathCandidate(xPath, kind, 0, false, ex.Message);
        }
    }

    private static int IndexAmong(AutomationElement root, string xPath, AutomationElement target)
    {
        try
        {
            var matches = root.FindAllByXPath(xPath);
            for (var i = 0; i < matches.Length; i++)
            {
                if (matches[i].Equals(target)) return i + 1;
            }
        }
        catch
        {
            // aday üretilemez
        }

        return 0;
    }

    // ---------------------------------------------------------------- Ağaçta gezinme

    private AutomationElement? Parent(AutomationElement element) =>
        automation.TreeWalkerFactory.GetControlViewWalker().GetParent(element);

    private AutomationElement FindTopLevelWindow(AutomationElement element)
    {
        var current = element;
        while (true)
        {
            var parent = Parent(current);
            if (parent is null || parent.Equals(_desktop)) return current;
            current = parent;
        }
    }

    private AutomationElement? NearestAncestorWithAutomationId(AutomationElement element, AutomationElement window)
    {
        var current = Parent(element);
        while (current is not null && !current.Equals(window))
        {
            if (!string.IsNullOrEmpty(current.Properties.AutomationId.ValueOrDefault)) return current;
            current = Parent(current);
        }

        return null;
    }

    /// <summary>
    /// Masaüstünden başlayan yol. anchorWindow true ise pencere sıra numarası yerine AutomationId/Name ile tanımlanır.
    /// Her adımda, aynı tipteki kardeşleri arasında birden fazla varsa sıra numarası eklenir (FlaUI ile aynı kural).
    /// </summary>
    private string? StructuralPath(AutomationElement element, AutomationElement window, bool anchorWindow)
    {
        var walker = automation.TreeWalkerFactory.GetControlViewWalker();
        var segments = new List<string>();
        var current = element;

        while (current is not null && !current.Equals(_desktop))
        {
            var type = ControlTypeOf(current);

            if (anchorWindow && current.Equals(window))
            {
                var windowId = current.Properties.AutomationId.ValueOrDefault;
                var windowName = current.Properties.Name.ValueOrDefault;
                var predicate = !string.IsNullOrEmpty(windowId) ? $"[@AutomationId={Literal(windowId)}]"
                              : !string.IsNullOrEmpty(windowName) ? $"[@Name={Literal(windowName)}]"
                              : null;
                if (predicate is null) return null;
                segments.Add(type + predicate);
                break;
            }

            var parent = walker.GetParent(current);
            if (parent is null) break;

            var sameType = new List<AutomationElement>();
            for (var child = walker.GetFirstChild(parent); child is not null; child = walker.GetNextSibling(child))
            {
                if (ControlTypeOf(child) == type) sameType.Add(child);
            }

            var position = sameType.FindIndex(c => c.Equals(current)) + 1;
            segments.Add(sameType.Count > 1 && position > 0 ? $"{type}[{position}]" : type);
            current = parent;
        }

        segments.Reverse();
        return segments.Count == 0 ? null : "/" + string.Join("/", segments);
    }

    private static string ControlTypeOf(AutomationElement element) =>
        element.Properties.ControlType.ValueOrDefault.ToString();

    // ---------------------------------------------------------------- C# kod üretimi

    private static string BuildSnippet(string type, string name, string id, IReadOnlyList<XPathCandidate> candidates)
    {
        // Yedekler farklı türden olsun ki biri kırılınca hepsi birden kırılmasın:
        // en fazla 2 öznitelik tabanlı (Id/Name/üst eleman) + 1 yapısal yol.
        var unique = candidates.Where(c => c.IsUnique).ToList();
        var attributeBased = unique.Where(c => !c.Kind.Contains("yol")).Take(MaxXPathsInSnippet - 1);
        var structural = unique.Where(c => c.Kind.Contains("yol")).Take(1);
        var chosen = attributeBased.Concat(structural).ToList();
        var warning = string.Empty;

        if (chosen.Count == 0)
        {
            chosen = candidates.Where(c => c.PointsToElement).Take(1).ToList();
            warning = "    // UYARI: Tekil XPath bulunamadı; aşağıdaki XPath birden fazla elemente uyuyor.\n";
        }

        var description = name.Length > 0 ? $"{name} ({type})" : id.Length > 0 ? $"{id} ({type})" : type;
        var fieldName = ToIdentifier(name.Length > 0 ? name : id) + type;

        var sb = new StringBuilder();
        sb.Append(warning);
        sb.Append($"    public static readonly Locator {fieldName} = new(\n");
        sb.Append($"        \"{EscapeCSharp(description)}\"");
        foreach (var c in chosen)
        {
            sb.Append($",\n        \"{EscapeCSharp(c.XPath)}\"");
        }

        sb.Append(");\n");
        return sb.ToString();
    }

    private static string ToIdentifier(string text)
    {
        var map = new Dictionary<char, string>
        {
            ['ç'] = "c", ['Ç'] = "C", ['ğ'] = "g", ['Ğ'] = "G", ['ı'] = "i", ['İ'] = "I",
            ['ö'] = "o", ['Ö'] = "O", ['ş'] = "s", ['Ş'] = "S", ['ü'] = "u", ['Ü'] = "U",
        };

        var ascii = string.Concat(text.Select(ch => map.TryGetValue(ch, out var r) ? r : ch.ToString()));
        var words = ascii.Split(ascii.Where(ch => !char.IsLetterOrDigit(ch)).Distinct().ToArray(),
            StringSplitOptions.RemoveEmptyEntries);

        var result = string.Concat(words.Select(w =>
            char.ToUpper(w[0], CultureInfo.InvariantCulture) + w[1..]));

        if (result.Length == 0) return "Element";
        return char.IsDigit(result[0]) ? "_" + result : result;
    }

    private static string EscapeCSharp(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>Test projesindeki Locator.Literal ile aynı: tırnak içeren metinleri XPath'e güvenle gömer.</summary>
    private static string Literal(string value)
    {
        if (!value.Contains('\'')) return $"'{value}'";
        if (!value.Contains('"')) return $"\"{value}\"";
        return "concat('" + value.Replace("'", "',\"'\",'") + "')";
    }
}
