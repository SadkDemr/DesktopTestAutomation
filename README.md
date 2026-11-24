# DesktopTestAutomation

WPF masaüstü uygulamaları için **FlaUI (UIA3) + NUnit** tabanlı UI test otomasyonu iskeleti.
İçinde pratik yapmak için örnek bir WPF uygulaması (`SampleWpfApp`) bulunur.

> **Test yazmaya başlıyorsanız:** [docs/YAZIM-KILAVUZU.md](docs/YAZIM-KILAVUZU.md). Tıklama, kontrol etme,
> if/else, döngü, klavye ve bekleme örneklerinin hepsi orada.

## Gereksinimler

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (".NET masaüstü geliştirme" iş yükü) veya `dotnet` CLI
- İsteğe bağlı: [FlaUInspect](https://github.com/FlaUI/FlaUInspect): element ağacını ve AutomationId'leri incelemek için

## Proje yapısı

```
DesktopTestAutomation.sln
├── src/SampleWpfApp/                  Test edilecek örnek WPF uygulaması (login + müşteri yönetimi)
├── tools/LocatorPicker/               Fareyle gösterilen element için XPath üreten araç
└── tests/DesktopTestAutomation.Tests/
    ├── appsettings.json               Uygulama yolu, attach modu, zaman aşımı ayarları
    ├── Framework/
    │   ├── Locator.cs                 Element adı + denenecek XPath listesi
    │   ├── AppSession.cs              Uygulamayı başlatma/bağlanma/kapatma, Locator çözümleme
    │   ├── BaseUiTest.cs              Yaşam döngüsü, ekran görüntüsü, senaryo adımları (Click, SetText...)
    │   ├── AppLifetime.cs             PerTest / PerFixture
    │   ├── TestSettings.cs            appsettings.json + ortam değişkeni okuma
    │   ├── Wait.cs                    Thread.Sleep yerine koşul bazlı bekleme
    │   └── ElementExtensions.cs       Ortak element yardımcıları (GetText)
    ├── Locators/                      XPath dosyaları (ekran başına bir dosya)
    │   ├── IsAkisi/                   Kendi uygulamanız: IsAkisiMenuLocators, IsAkisiTakipLocators
    │   └── SampleApp/                 Örnek uygulama: LoginLocators, MainWindowLocators, ConfirmDialogLocators
    └── Tests/                         Senaryolar (Locators'tan XPath çağırır)
        ├── IsAkisi/                   IsAkisiTakipTests
        └── SampleApp/                 LoginTests, CustomerTests
```

## XPath almanın kolay yolu: LocatorPicker

FlaUInspect'teki XPath kopyalanamaz ve yalnızca sıra numaralarından oluşur (`/Window[2]/Custom[1]/...`).
`tools/LocatorPicker` bunun yerine sağlam XPath'ler üretir, her birini çalışan uygulamada doğrular ve
Locator dosyalarına doğrudan yapıştırılacak C# kodunu panoya kopyalar.

1. Test edilecek uygulamayı açın.
2. LocatorPicker'ı başlatın: Visual Studio'da projeye sağ tık → **Debug → Start New Instance**, ya da
   `dotnet run --project tools/LocatorPicker`.
3. Fareyi elementin üzerine getirip **Ctrl+Shift+X**'e basın. Element turuncu çerçeveyle vurgulanır.
4. İlgili `Locators/...` dosyasına **Ctrl+V** ile yapıştırın:

```csharp
    public static readonly Locator GirisYapButton = new(
        "Giriş yap (Button)",
        "//Button[@AutomationId='LoginButton']",              // en sağlam
        "//Button[@Name='Giriş yap']",                        // yedek
        "/Window[@AutomationId='LoginWindow']/Button");       // yapısal yedek
```

- Buton, menü veya ağaç öğesinin içindeki yazıya gelseniz de kontrolün kendisi alınır.
  Yazının kendisi gerekiyorsa **Ctrl+Shift+Z** kullanın.
- Konsolda tüm adaylar ✓ (tekil) / ✗ (birden fazla elemente uyuyor) olarak listelenir. Koda yalnızca
  tekil olanlar girer.
- Her sonuç ayrıca çalışma klasöründeki `picked-locators.txt` dosyasına eklenir.
- Bir XPath'i denemek için: `LocatorPicker.exe --test "//Button[@Name='Filtrele']"`. Eşleşen elementleri
  listeler ve ekranda vurgular.

## XPath dosyaları ve senaryo yazımı

**1. XPath'i Locators dosyasına ekleyin** (ekran başına bir dosya). Birden fazla XPath verirseniz sırayla
denenir; ilk bulunan kullanılır:

```csharp
public static class IsAkisiMenuLocators
{
    public static readonly Locator IsAkisi = new(
        "İş Akışı menüsü",                                    // log ve hata mesajlarında görünen ad
        "//Text[@Name='İş Akışı']",                           // birincil
        "/Window[2]/Custom[1]/Custom[1]/Tree/TreeItem[1]");   // yedek (Inspector mutlak yolu)
}
```

**2. Test dosyasında adımları çağırın.** Adımlar elementi bulana kadar bekler (varsayılan 10 sn), bu yüzden
`Thread.Sleep` gerekmez:

```csharp
Click(IsAkisiMenuLocators.IsAkisi);
Click(IsAkisiMenuLocators.IsAkisiTakip);
Click(IsAkisiTakipLocators.FiltreleButonu);
```

Kullanılabilir adımlar: `Click`, `DoubleClick`, `Invoke` (fare kullanmadan buton), `SetText`, `TypeText` (klavye),
`Select` (ComboBox), `SetChecked`, `GetText`, `WaitForText`, `Exists`, `WaitUntilGone`, `Find`.

**XPath kökü önemli:**
- `//...` ile başlayan XPath'ler uygulamanın pencerelerinin **içinde** aranır.
- Tek `/` ile başlayanlar (FlaUI Inspector'ın kopyaladığı mutlak yollar) **masaüstünden** aranır.
  `Window[2]` gibi index'ler masaüstündeki pencere sırasına bağlıdır ve kolay kırılır. Mümkünse `Name` veya
  `AutomationId` içeren `//` XPath'leri birincil, mutlak yolu yedek olarak kullanın.

Bulunamayan elementlerde hata mesajı denenen tüm XPath'leri listeler ve ekran görüntüsü test sonucuna eklenir.

**Uygulama yaşam döngüsü:** Varsayılan olarak her test uygulamayı yeniden açar. Adım adım ilerleyen senaryolarda
fixture'a `protected override AppLifetime Lifetime => AppLifetime.PerFixture;` ekleyin ve testlere `[Order(n)]` verin.
appsettings.json'daki `"AttachToRunningApp": true` (varsayılan) ayarıyla uygulama açıksa yeniden başlatılmaz,
açık olana bağlanılır. Kapalıysa bir kez açılır ve test bitince **açık bırakılır**, böylece sonraki testler ve
çalıştırmalar aynı pencereyi kullanır. Her testin temiz bir uygulamayla başlaması için `false` yapın.
Örnek uygulama testleri bu ayardan bağımsız olarak her zaman temiz başlar.

## Visual Studio ile çalıştırma

1. `DesktopTestAutomation.sln` dosyasını açın.
2. **Build → Build Solution** (Ctrl+Shift+B). Örnek uygulama da otomatik derlenir.
3. **Test → Test Explorer** (Ctrl+E, T) penceresinden testleri çalıştırın.
4. Örnek uygulamayı elle denemek için `SampleWpfApp` projesine sağ tıklayıp **Set as Startup Project** seçin ve F5'e basın
   (kullanıcı: `admin`, şifre: `admin123`).

> Testler çalışırken fareye ve klavyeye dokunmayın. UI testleri gerçek masaüstünü kullanır.

## Komut satırından çalıştırma

```powershell
dotnet build
dotnet test                                   # tüm testler
dotnet test --filter "Category=IsAkisi"       # yalnızca İş Akışı senaryoları
dotnet test --filter "Category=Login"         # yalnızca login testleri
dotnet test --filter "Name~DeleteCustomer"    # adı eşleşen testler
```

Başarısız testlerin ekran görüntüleri `tests/DesktopTestAutomation.Tests/bin/<Config>/net8.0-windows/Screenshots/`
klasörüne kaydedilir ve test sonucuna eklenir.

## Kendi uygulamanızı test etmek

1. `appsettings.json` içinde `AppPath` değerine uygulamanızın exe yolunu yazın
   (ya da `DESKTOP_APP_PATH` ortam değişkenini ayarlayın; CI için uygundur).
   Ayarlanmadığı sürece kendi uygulamanızın testleri "Atlandı" (Ignored) olarak görünür.
2. Her ekran için `Locators/<Modül>/` altına bir locator dosyası, `Tests/<Modül>/` altına test dosyası ekleyin.
   Test sınıfları `BaseUiTest`'ten türer.
3. Örnek uygulama testleri (`Category=SampleApp`) `AppPath`'ten bağımsızdır. İsterseniz silebilirsiniz.
4. Mümkünse geliştiricilerden XAML'da elementlere `AutomationProperties.AutomationId` eklemelerini isteyin.
   `//Button[@AutomationId='FiltreleButton']` gibi XPath'ler, isim ve index tabanlı olanlardan çok daha kararlıdır.

## İpuçları

- **Element bulma:** Her zaman AutomationId'yi tercih edin. Görünen metin dile ve tasarıma göre değişir.
- **MessageBox butonları:** Metinler OS diline göre değişir ("Yes"/"Evet"). Dilden bağımsız Id'leri kullanın:
  OK=`1`, Cancel=`2`, Yes=`6`, No=`7`.
- **PasswordBox:** ValuePattern ile değer atanamaz. Odaklanıp klavyeyle yazılır (`LoginPage.EnterCredentials`).
- **Sanallaştırılmış listeler (DataGrid/ListView):** Ekranda görünmeyen satırlar UIA ağacında yer almaz.
  Büyük listelerde önce kaydırma yapın.
- **Paralel çalıştırma:** Aynı masaüstünde UI testleri paralel koşamaz. Paralellik için birden fazla VM/agent kullanın.
- **CI:** Test agent'ı servis olarak değil, oturum açmış etkileşimli bir kullanıcı olarak çalışmalı
  (ekran kilitli veya RDP penceresi küçültülmüş olmamalı).
