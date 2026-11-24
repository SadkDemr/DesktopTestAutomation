using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace DesktopTestAutomation.Tests.Framework;

public static class ElementExtensions
{
    /// <summary>
    /// Bir elementin kullanıcıya görünen metnini döndürür.
    /// Sırasıyla ValuePattern, içindeki Text elementi ve Name özelliğine bakar
    /// (WPF DataGrid hücreleri, TextBlock'lar vb. için ortak çözüm).
    /// </summary>
    public static string GetText(this AutomationElement element)
    {
        if (element.Patterns.Value.TryGetPattern(out var valuePattern))
        {
            var value = valuePattern.Value.ValueOrDefault;
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        var textChild = element.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));
        if (textChild is not null)
        {
            return textChild.Name;
        }

        return element.Name;
    }
}
