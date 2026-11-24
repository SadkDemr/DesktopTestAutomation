using System.Reflection;
using System.Text.Json;

namespace DesktopTestAutomation.Tests.Framework;

/// <summary>
/// appsettings.json'dan okunan test ayarları.
/// Test edilecek uygulamanın yolu: DESKTOP_APP_PATH ortam değişkeni → appsettings.json AppPath.
/// </summary>
public sealed class TestSettings
{
    public const string AppPathEnvironmentVariable = "DESKTOP_APP_PATH";

    private static readonly Lazy<TestSettings> LazyCurrent = new(Load);

    public static TestSettings Current => LazyCurrent.Value;

    public string? AppPath { get; init; }
    public bool AttachToRunningApp { get; init; }
    public int StartupTimeoutSeconds { get; init; } = 20;
    public int DefaultTimeoutSeconds { get; init; } = 10;
    public string ScreenshotDirectory { get; init; } = "Screenshots";

    public TimeSpan StartupTimeout => TimeSpan.FromSeconds(StartupTimeoutSeconds);
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(DefaultTimeoutSeconds);

    /// <summary>Test edilecek uygulamanın exe yolu; ayarlanmamışsa null.</summary>
    public string? ResolveAppPath()
    {
        var candidate = Environment.GetEnvironmentVariable(AppPathEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = AppPath;
        }

        return string.IsNullOrWhiteSpace(candidate) ? null : ToExistingFullPath(candidate);
    }

    /// <summary>Solution içindeki örnek WPF uygulamasının exe yolu (csproj'da build sırasında belirlenir).</summary>
    public static string ResolveSampleAppPath()
    {
        var path = typeof(TestSettings).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "SampleAppPath")?.Value
            ?? throw new InvalidOperationException("SampleAppPath bilgisi test assembly'sinde bulunamadı.");

        return ToExistingFullPath(path);
    }

    private static string ToExistingFullPath(string path)
    {
        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path), AppContext.BaseDirectory);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Uygulama bulunamadı: {fullPath}. Uygulamanın derlendiğinden ve yolun doğru olduğundan emin olun.",
                fullPath);
        }

        return fullPath;
    }

    private static TestSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            return new TestSettings();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        return JsonSerializer.Deserialize<TestSettings>(File.ReadAllText(path), options) ?? new TestSettings();
    }
}
