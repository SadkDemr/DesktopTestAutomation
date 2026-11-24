# Test Yazım Kılavuzu

Bu kılavuz, uygulamamız için nasıl test senaryosu yazılacağını adım adım anlatır.
Buradaki örneklerin çalışan halleri `tests/DesktopTestAutomation.Tests/Tests/Ornekler/OrnekSenaryolar.cs`
dosyasındadır; örnek uygulama üzerinde koşarlar ve kopyalanıp kullanılabilirler.

---

## İçindekiler

1. [Hızlı başvuru](#1-hızlı-başvuru)
2. [Bir test nasıl kurulur](#2-bir-test-nasıl-kurulur)
3. [Eylemler: tıklama, yazma, seçme](#3-eylemler-tıklama-yazma-seçme)
4. [Kontrol etmek: "görünüyor mu?"](#4-kontrol-etmek-görünüyor-mu)
5. [Doğrulamalar (Assert)](#5-doğrulamalar-assert)
6. [if / else örnekleri](#6-if--else-örnekleri)
7. [Döngüler ve tekrar eden veriler](#7-döngüler-ve-tekrar-eden-veriler)
8. [Klavye](#8-klavye)
9. [Bekleme](#9-bekleme)
10. [Kurallara uymayan durumlar](#10-kurallara-uymayan-durumlar)
11. [Sık yapılan hatalar](#11-sık-yapılan-hatalar)

---

## 1. Hızlı başvuru

| Ne yapmak istiyorum? | Yazılacak kod |
|---|---|
| Tıkla | `Click(Ekran.Buton);` |
| Çift tıkla | `DoubleClick(Ekran.Satir);` |
| Butona bas (fare kullanmadan) | `Invoke(Ekran.Buton);` |
| Metin kutusuna yaz | `SetText(Ekran.AdKutusu, "Ali");` |
| Klavyeyle yaz (şifre kutusu gibi) | `TypeText(Ekran.SifreKutusu, "1234");` |
| Açılır listeden seç | `Select(Ekran.SehirListesi, "Ankara");` |
| CheckBox işaretle / kaldır | `SetChecked(Ekran.AktifKutusu, true);` |
| Metni oku | `var metin = GetText(Ekran.Mesaj);` |
| **Görünüyor mu?** (test düşmez) | `if (Exists(Ekran.Popup, TimeSpan.FromSeconds(3))) { ... }` |
| Aktif mi? | `if (IsEnabled(Ekran.KaydetButonu)) { ... }` |
| İşaretli mi? | `if (IsChecked(Ekran.AktifKutusu)) { ... }` |
| Metin şu olana kadar bekle | `WaitForText(Ekran.Mesaj, "Kaydedildi");` |
| Kaybolana kadar bekle | `WaitUntilGone(Ekran.Yukleniyor);` |
| Elementi al (FlaUI ile özel işlem için) | `var e = Find(Ekran.Tablo);` |
| Teste not düş | `Log("Kayıt oluşturuldu");` |
| Doğrula | `Assert.That(GetText(Ekran.Mesaj), Is.EqualTo("Kaydedildi"));` |

> Burada `Ekran.Buton` gibi adlar, `Locators` klasöründeki dosyalarda tanımladığınız locator'lardır.

---

## 2. Bir test nasıl kurulur

Her ekran için iki dosya vardır:

| Dosya | Yer | İçerik |
|---|---|---|
| Locator dosyası | `Locators/<Modül>/<Ekran>Locators.cs` | Elementlerin XPath'leri |
| Test dosyası | `Tests/<Modül>/<Ekran>Tests.cs` | Senaryolar |

### Locator dosyası

XPath'leri **LocatorPicker** ile alıp buraya yapıştırın (bkz. README → "XPath almanın kolay yolu"):

```csharp
using DesktopTestAutomation.Tests.Framework;

namespace DesktopTestAutomation.Tests.Locators.IsAkisi;

public static class IsAkisiTakipLocators
{
    public static readonly Locator FiltreleButonu = new(
        "Filtrele butonu",                       // loglarda ve hata mesajlarında görünen ad
        "//Button[@AutomationId='btnFiltrele']", // birincil XPath
        "//Button[@Name='Filtrele']");           // yedek: birincil bulunamazsa denenir
}
```

### Test dosyası

```csharp
using DesktopTestAutomation.Tests.Framework;
using DesktopTestAutomation.Tests.Locators.IsAkisi;

namespace DesktopTestAutomation.Tests.Tests.IsAkisi;

[TestFixture]
[Category("IsAkisi")]                   // Test Explorer'da gruplamak ve filtrelemek için
public class IsAkisiTakipTests : BaseUiTest
{
    [Test]
    public void Filtreleme_SonuclariGosterir()
    {
        // 1. Hazırlık: ekrana git
        Click(IsAkisiMenuLocators.IsAkisi);
        Click(IsAkisiMenuLocators.IsAkisiTakip);

        // 2. Eylem
        Click(IsAkisiTakipLocators.FiltreleButonu);

        // 3. Doğrulama
        Assert.That(Exists(IsAkisiTakipLocators.SonucListesi, TimeSpan.FromSeconds(5)), Is.True,
            "Filtreleme sonrası sonuç listesi görünmedi.");
    }
}
```

**Kurallar:**
- Test sınıfı `BaseUiTest`'ten türer. Uygulamayı açma, kapatma ve hata anında ekran görüntüsü alma otomatik yapılır.
- Her testin üstüne `[Test]` yazılır.
- Test adı ne test ettiğini anlatsın: `Durum_BeklenenSonuc` (ör. `GecersizEposta_HataGosterir`).
- Bir test **tek bir şeyi** doğrulasın. Uzun akışları birkaç teste bölün.

---

## 3. Eylemler: tıklama, yazma, seçme

Bütün eylemler elementi **bulana kadar bekler** (varsayılan 10 sn) ve aktif olmasını bekler.
`Thread.Sleep` kullanmaya gerek yoktur.

```csharp
Click(Ekran.KaydetButonu);                    // fareyle tıklar
DoubleClick(Ekran.MusteriSatiri);             // çift tıklar
Invoke(Ekran.KaydetButonu);                   // fare kullanmadan butona basar

SetText(Ekran.AdKutusu, "Zeynep Demir");      // kutudaki metni silip yenisini yazar
TypeText(Ekran.SifreKutusu, "admin123");      // klavyeyle tuş tuş yazar
Select(Ekran.SehirListesi, "İzmir");          // açılır listede görünen metne göre seçer
SetChecked(Ekran.AktifKutusu, false);         // işareti kaldırır
```

### Click mi, Invoke mi?

| | `Click` | `Invoke` |
|---|---|---|
| Nasıl çalışır | Fareyi elementin üzerine götürüp tıklar | Butona doğrudan "basıldı" komutu gönderir |
| Nerede kullanılır | Her şeyde: menü, ağaç öğesi, metin, satır | Yalnızca butonlarda |
| Pencere görünür olmalı mı | Evet, önünde başka pencere olmamalı | Hayır |
| Öneri | Menü ve ağaç öğeleri için | Butonlar için (daha kararlı) |

### SetText mi, TypeText mi?

- `SetText`: Hızlıdır ve klavye dilinden etkilenmez. Normal metin kutuları için bunu kullanın.
- `TypeText`: Gerçek tuş basışı yapar. `SetText`'in çalışmadığı yerlerde kullanın: şifre kutuları
  (PasswordBox) ve her tuşa tepki veren arama kutuları gibi.

---

## 4. Kontrol etmek: "görünüyor mu?"

Bir elementin ekranda olup olmadığını iki farklı şekilde sorabilirsiniz. Aradaki fark önemli:

| | `Find(x)` | `Exists(x, süre)` |
|---|---|---|
| Element varsa | Elementi döndürür | `true` döner |
| Element yoksa | **Test hatayla durur** | `false` döner, test devam eder |
| Ne zaman | Elementin kesinlikle olması gerekiyorsa | "Varsa şunu yap" durumları, if/else |

```csharp
// Hemen bak (beklemeden):
bool varMi = Exists(Ekran.Popup);

// En fazla 3 saniye bekleyerek bak:
bool varMi = Exists(Ekran.Popup, TimeSpan.FromSeconds(3));
```

Diğer durum soruları:

```csharp
bool aktifMi    = IsEnabled(Ekran.KaydetButonu);   // buton tıklanabilir mi?
bool isaretliMi = IsChecked(Ekran.AktifKutusu);    // checkbox işaretli mi?
string metin    = GetText(Ekran.DurumMesaji);      // ekranda yazan metin
```

> `IsEnabled`, `IsChecked` ve `GetText` elementin ekranda **olmasını** bekler; yoksa test hata verir.
> Elementin olmayabileceği durumlarda önce `Exists` ile kontrol edin.

---

## 5. Doğrulamalar (Assert)

Test, doğrulamalar sayesinde "geçti" ya da "kaldı" olur. Hepsi `Assert.That(değer, koşul, "mesaj")` biçimindedir.
Mesaj isteğe bağlıdır ama test kaldığında ne olduğunu anlamayı çok kolaylaştırır.

```csharp
// Eşitlik
Assert.That(GetText(Ekran.Mesaj), Is.EqualTo("Kayıt başarılı"));

// İçeriyor / ile başlıyor / ile bitiyor
Assert.That(GetText(Ekran.Mesaj), Does.Contain("başarılı"));
Assert.That(GetText(Ekran.Baslik), Does.StartWith("Müşteri"));

// Doğru / yanlış
Assert.That(Exists(Ekran.HataMesaji), Is.False, "Beklenmeyen hata mesajı göründü.");
Assert.That(IsEnabled(Ekran.KaydetButonu), Is.True);

// Sayılar
Assert.That(satirSayisi, Is.EqualTo(5));
Assert.That(satirSayisi, Is.GreaterThan(0));

// Boş / dolu
Assert.That(GetText(Ekran.AdKutusu), Is.Empty);
Assert.That(GetText(Ekran.AdKutusu), Is.Not.Empty);
```

### Birden fazla doğrulama

Normalde ilk başarısız doğrulamada test durur. Hepsinin çalışmasını ve hataların birlikte raporlanmasını
istiyorsanız:

```csharp
using (Assert.EnterMultipleScope())
{
    Assert.That(GetText(Ekran.Ad), Is.EqualTo("Zeynep"));
    Assert.That(GetText(Ekran.Sehir), Is.EqualTo("İzmir"));
    Assert.That(IsChecked(Ekran.Aktif), Is.False);
}
```

---

## 6. if / else örnekleri

Koşullarda her zaman `Exists`, `IsEnabled`, `IsChecked` veya `GetText` kullanılır.

### Bir pencere/popup çıkarsa kapat

```csharp
Invoke(Ekran.SilButonu);

// Onay penceresi bazen çıkar, bazen çıkmaz:
if (Exists(OnayLocators.Pencere, TimeSpan.FromSeconds(2)))
{
    Invoke(OnayLocators.EvetButonu);
}
```

### Giriş yapılmamışsa giriş yap

Uygulama açık bırakıldığında (`AttachToRunningApp: true`) giriş zaten yapılmış olabilir:

```csharp
if (Exists(GirisLocators.Pencere, TimeSpan.FromSeconds(3)))
{
    Log("Giriş ekranı açık, giriş yapılıyor.");
    SetText(GirisLocators.KullaniciAdi, "admin");
    TypeText(GirisLocators.Sifre, "admin123");
    Invoke(GirisLocators.GirisButonu);
}
else
{
    Log("Zaten giriş yapılmış.");
}
```

### Duruma göre farklı yol

```csharp
if (IsChecked(Ekran.AktifKutusu))
{
    SetChecked(Ekran.AktifKutusu, false);
}

if (!IsEnabled(Ekran.SilButonu))
{
    Click(Ekran.IlkSatir);   // önce bir satır seç ki buton aktif olsun
}

var durum = GetText(Ekran.DurumEtiketi);
if (durum == "Onay Bekliyor")
{
    Invoke(Ekran.OnaylaButonu);
}
else if (durum == "Reddedildi")
{
    Invoke(Ekran.TekrarGonderButonu);
}
else
{
    Assert.Fail($"Beklenmeyen durum: {durum}");
}
```

> **Dikkat:** if/else'i uygulamanın *gerçekten* değişken davrandığı yerlerde kullanın (popup bazen çıkar,
> giriş bazen yapılmıştır). Testin sonucunu if ile gizlemeyin; beklenen davranışı `Assert` ile doğrulayın.

---

## 7. Döngüler ve tekrar eden veriler

### foreach: aynı işlemi birden fazla kez yap

```csharp
var musteriler = new[] { "Ali Veli", "Fatma Şahin", "Can Öztürk" };

foreach (var ad in musteriler)
{
    SetText(Ekran.AdKutusu, ad);
    SetText(Ekran.EpostaKutusu, "test@example.com");
    Invoke(Ekran.EkleButonu);
    WaitForText(Ekran.DurumMesaji, $"Müşteri eklendi: {ad}");
}
```

### TestCase: aynı testi farklı verilerle çalıştır

Her `TestCase` satırı Test Explorer'da ayrı bir test olarak görünür ve ayrı raporlanır:

```csharp
[TestCase("", "ali@example.com", "Ad Soyad alanı zorunludur.")]
[TestCase("Ali Veli", "gecersiz-eposta", "Geçerli bir e-posta adresi girin.")]
public void GecersizVeri_HataGosterir(string ad, string eposta, string beklenenHata)
{
    SetText(Ekran.AdKutusu, ad);
    SetText(Ekran.EpostaKutusu, eposta);
    Invoke(Ekran.EkleButonu);

    WaitForText(Ekran.DurumMesaji, beklenenHata);
}
```

> Farklı verilerle aynı kontrol yapılacaksa `foreach` yerine `TestCase` tercih edin. Biri kalınca
> hangisinin kaldığı raporda net görünür.

---

## 8. Klavye

Dosyanın başına şunları ekleyin:

```csharp
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
```

```csharp
Keyboard.Type(VirtualKeyShort.ENTER);     // tek tuş: ENTER, TAB, ESCAPE, F5, DELETE, DOWN, UP...
Keyboard.Type("serbest metin");           // o an odakta olan yere yazar

// Kısayol (tuşlar birlikte basılır):
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);   // Ctrl+S
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);   // Ctrl+A
Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.F4);          // Alt+F4
```

Klavye komutları **odaktaki** elemente gider. Gerekirse önce odaklayın:

```csharp
Find(Ekran.AdKutusu).Focus();
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
Keyboard.Type("Yeni Ad");
```

---

## 9. Bekleme

**`Thread.Sleep` kullanmayın.** Yavaş makinede yetmez, hızlı makinede boşa zaman harcar.
Bunun yerine:

| Durum | Kod |
|---|---|
| Element gelene kadar bekle | Gerek yok, `Click`/`SetText`/`Find` zaten bekler |
| Daha uzun bekle (yavaş ekran) | `Find(Ekran.Rapor, TimeSpan.FromSeconds(30));` |
| Metin şu olana kadar bekle | `WaitForText(Ekran.Durum, "Tamamlandı");` |
| Metin için daha uzun bekle | `WaitForText(Ekran.Durum, "Tamamlandı", TimeSpan.FromSeconds(60));` |
| "Yükleniyor" kaybolana kadar bekle | `WaitUntilGone(Ekran.YukleniyorGostergesi);` |
| Varsa bekle, yoksa geç | `if (Exists(Ekran.Popup, TimeSpan.FromSeconds(3))) { ... }` |

Varsayılan bekleme süresi `appsettings.json` içindeki `DefaultTimeoutSeconds` değeridir (10 sn).

---

## 10. Kurallara uymayan durumlar

Hazır metotların yetmediği yerlerde `Find(...)` ile elementi alıp FlaUI'nin kendi metotlarını kullanabilirsiniz:

```csharp
using FlaUI.Core.AutomationElements;

Find(Ekran.MusteriSatiri).RightClick();             // sağ tık
Find(Ekran.Liste).AsListBox().Select(2);            // listede 3. öğeyi seç (0'dan başlar)
var tablo = Find(Ekran.Tablo).AsGrid();             // tablo
int satirSayisi = tablo.RowCount;
string hucre = tablo.Rows[0].Cells[1].Name;         // 1. satır, 2. sütun
Find(Ekran.AgacOgesi).AsTreeItem().Expand();        // ağaç düğümünü aç
```

Sık kullandığınız bir işlem olursa, onu `BaseUiTest`'e ya da ilgili modülün temel sınıfına yardımcı metot
olarak ekleyin (örnek: `Tests/SampleApp/SampleAppUiTest.cs` içindeki `LoginAs`, `SelectCustomer`).

### Tekrar eden akışları metoda çevirin

Birçok testte aynı adımlar tekrarlanıyorsa (ör. menüden ekrana gitmek) bunları bir metoda toplayın:

```csharp
private void IsAkisiTakipEkraniniAc()
{
    Click(IsAkisiMenuLocators.IsAkisi);
    Click(IsAkisiMenuLocators.IsAkisiTakip);
}

[Test]
public void Filtreleme_SonuclariGosterir()
{
    IsAkisiTakipEkraniniAc();
    Click(IsAkisiTakipLocators.FiltreleButonu);
    // ...
}
```

---

## 11. Sık yapılan hatalar

| Sorun | Sebep | Çözüm |
|---|---|---|
| "... bulunamadı. Denenen XPath'ler: ..." | XPath yanlış veya ekran henüz açılmadı | LocatorPicker ile XPath'i yeniden alın; `--test` ile deneyin |
| Butona tıklandı ama bir şey olmadı | Butonun içindeki yazı alınmış | LocatorPicker'da Ctrl+Shift+X kullanın (kontrolün kendisini alır) |
| Test bazen geçiyor bazen kalıyor | `Thread.Sleep` veya çok kısa bekleme | `WaitForText`/`Exists(..., süre)` kullanın |
| Test, önceki testin ekranında başlıyor | `AttachToRunningApp: true` uygulamayı açık bırakır | Testin başında bilinen ekrana dönün veya ayarı `false` yapın |
| `Click` çalışmıyor, `Invoke` çalışıyor | Önünde başka pencere var / pencere küçültülmüş | Butonlarda `Invoke` kullanın |
| `SetText` şifre kutusuna yazmıyor | PasswordBox değer atamaya izin vermez | `TypeText` kullanın |
| Türkçe karakterler bozuk yazılıyor | Klavye ile yazma, klavye diline bağlıdır | Mümkünse `SetText` kullanın |
| `/Window[2]/...` XPath'i bazen çalışmıyor | Sıra numarası masaüstündeki pencere sırasına bağlıdır | Adı veya Id'si olan `//` XPath'lerini birincil yapın |

### Test çalıştırma

- **Visual Studio:** Test → Test Explorer. Tek testi sağ tık → Run, kategoriye göre gruplamak için "Group By → Traits".
- **Komut satırı:**
  ```
  dotnet test --filter "Category=IsAkisi"
  dotnet test --filter "Name~Filtreleme"
  ```
- Test kaldığında ekran görüntüsü otomatik alınır; Test Explorer'da testin detayında "Attachments" altındadır.
