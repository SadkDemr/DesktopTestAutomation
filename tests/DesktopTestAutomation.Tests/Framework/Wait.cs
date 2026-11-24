using System.Diagnostics;

namespace DesktopTestAutomation.Tests.Framework;

/// <summary>
/// Thread.Sleep yerine kullanılacak bekleme yardımcıları. Koşul sağlanana kadar
/// belirli aralıklarla tekrar dener; geçici UIA hatalarını (element henüz hazır değil,
/// pencere kapanıyor vb.) yutar ve zaman aşımında anlamlı bir hata fırlatır.
/// </summary>
public static class Wait
{
    public static TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(200);

    public static T Until<T>(Func<T?> probe, TimeSpan timeout, string description) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? lastError = null;

        while (true)
        {
            try
            {
                var result = probe();
                if (result is not null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            if (stopwatch.Elapsed >= timeout)
            {
                throw new TimeoutException($"{timeout.TotalSeconds:0.#} sn içinde gerçekleşmedi: {description}", lastError);
            }

            Thread.Sleep(PollInterval);
        }
    }

    public static void Until(Func<bool> condition, TimeSpan timeout, string description)
    {
        Until(() => condition() ? Signal.Instance : null, timeout, description);
    }

    private sealed class Signal
    {
        public static readonly Signal Instance = new();
    }
}
