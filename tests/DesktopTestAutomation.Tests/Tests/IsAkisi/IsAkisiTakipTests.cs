using DesktopTestAutomation.Tests.Framework;
using DesktopTestAutomation.Tests.Locators.IsAkisi;

namespace DesktopTestAutomation.Tests.Tests.IsAkisi;

/// <summary>
/// Kendi uygulamanız için senaryo örneği. XPath'ler Locators/IsAkisi altındaki dosyalardan gelir.
/// Çalıştırmak için appsettings.json içinde AppPath'i uygulamanızın exe yoluna ayarlayın;
/// ayarlanmamışsa bu testler "Ignored" olarak atlanır.
/// </summary>
[TestFixture]
[Category("IsAkisi")]
public class IsAkisiTakipTests : BaseUiTest
{
    // Uygulama fixture başında bir kez açılır; bu sınıftaki testler aynı oturumda sırayla ilerler.
    protected override AppLifetime Lifetime => AppLifetime.PerFixture;

    [Test]
    [Order(1)]
    public void IsAkisiTakip_Acilir_VeFiltreleButonunaTiklanir()
    {
        // 1. adım: İş Akışı menüsünü aç
        Click(IsAkisiMenuLocators.IsAkisi);

        // 2. adım: İş Akışı Takip ekranını aç
        Click(IsAkisiMenuLocators.IsAkisiTakip);

        // 3. adım: Yeni ekrandaki Filtrele butonuna tıkla.
        // Thread.Sleep gerekmez: Click, element ekrana gelene kadar (varsayılan 10 sn) bekler.
        Click(IsAkisiTakipLocators.FiltreleButonu);

        // Doğrulama örneği (ekrandaki gerçek bir elementle değiştirin):
        // Assert.That(Exists(IsAkisiTakipLocators.SonucListesi, TimeSpan.FromSeconds(5)), Is.True,
        //     "Filtreleme sonrası sonuç listesi görünmedi.");
    }
}
