using DesktopTestAutomation.Tests.Locators.SampleApp;
using DesktopTestAutomation.Tests.Tests.SampleApp;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;

namespace DesktopTestAutomation.Tests.Tests.Ornekler;

/// <summary>
/// docs/YAZIM-KILAVUZU.md içindeki örneklerin çalışan halleri.
/// Örnek uygulama (SampleWpfApp) üzerinde koşar; yeni senaryo yazarken buradan kopyalayabilirsiniz.
/// </summary>
[TestFixture]
[Category("Ornekler")]
public class OrnekSenaryolar : SampleAppUiTest
{
    // ------------------------------------------------------------------ if / else

    [Test]
    public void IfElse_GirisEkraniVarsaGirisYap()
    {
        // Uygulama açık bırakılıyorsa (AttachToRunningApp) giriş zaten yapılmış olabilir.
        // Exists beklemeden (veya verilen süre kadar) bakar ve true/false döner; test düşmez.
        if (Exists(LoginLocators.Window, TimeSpan.FromSeconds(3)))
        {
            Log("Giriş ekranı açık, giriş yapılıyor.");
            LoginAs(TestData.ValidUsername, TestData.ValidPassword);
        }
        else
        {
            Log("Zaten giriş yapılmış, bu adım atlandı.");
        }

        Assert.That(Exists(MainWindowLocators.Window, TimeSpan.FromSeconds(5)), Is.True, "Ana pencere görünmedi.");
    }

    [Test]
    public void IfElse_OnayPenceresiCikarsaEvetDe()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);
        SelectCustomer("Mehmet Kaya");
        Invoke(MainWindowLocators.DeleteButton);

        // Bazı işlemler onay penceresi açar, bazıları açmaz: varsa kapat, yoksa devam et.
        if (Exists(ConfirmDialogLocators.Window, TimeSpan.FromSeconds(2)))
        {
            Invoke(ConfirmDialogLocators.YesButton);
        }

        WaitForText(MainWindowLocators.StatusText, "Müşteri silindi: Mehmet Kaya");
    }

    [Test]
    public void IfElse_CheckBoxDurumunaGore()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);

        if (IsChecked(MainWindowLocators.IsActiveCheckBox))
        {
            SetChecked(MainWindowLocators.IsActiveCheckBox, false);
        }

        Assert.That(IsChecked(MainWindowLocators.IsActiveCheckBox), Is.False);
    }

    [Test]
    public void IfElse_ButonAktifDegilseOnceSecimYap()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);

        if (!IsEnabled(MainWindowLocators.DeleteButton))
        {
            SelectCustomer("Ayşe Yılmaz");
        }

        Assert.That(IsEnabled(MainWindowLocators.DeleteButton), Is.True);
    }

    // ------------------------------------------------------------------ Döngü

    [Test]
    public void Dongu_BirdenFazlaKayitEkle()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);

        var yeniMusteriler = new[] { "Ali Veli", "Fatma Şahin", "Can Öztürk" };

        foreach (var ad in yeniMusteriler)
        {
            SetText(MainWindowLocators.NameTextBox, ad);
            SetText(MainWindowLocators.EmailTextBox, "test@example.com");
            Invoke(MainWindowLocators.AddButton);
            WaitForText(MainWindowLocators.StatusText, $"Müşteri eklendi: {ad}");
        }

        Assert.That(GetText(MainWindowLocators.CustomerCountText), Is.EqualTo("Toplam müşteri: 5"));
    }

    // ------------------------------------------------------------------ Klavye

    [Test]
    public void Klavye_KisayolVeTuslar()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);
        SetText(MainWindowLocators.NameTextBox, "Eski Ad");

        // Alana odaklan, Ctrl+A ile tümünü seç, üzerine yaz.
        Find(MainWindowLocators.NameTextBox).Focus();
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
        Keyboard.Type("Yeni Ad");
        Keyboard.Type(VirtualKeyShort.TAB); // tek tuş: Enter, Tab, Escape, F5...

        Assert.That(GetText(MainWindowLocators.NameTextBox), Is.EqualTo("Yeni Ad"));
    }

    // ------------------------------------------------------------------ Doğrulamalar

    [Test]
    public void Dogrulama_CokluKontrol()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);

        // Bir doğrulama başarısız olsa bile hepsi çalışır, hatalar en sonda birlikte raporlanır.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetText(MainWindowLocators.WelcomeText), Does.Contain("admin"));
            Assert.That(GetText(MainWindowLocators.CustomerCountText), Does.StartWith("Toplam müşteri"));
            Assert.That(IsEnabled(MainWindowLocators.AddButton), Is.True);
            Assert.That(Exists(ConfirmDialogLocators.Window), Is.False, "Beklenmeyen onay penceresi açık.");
        }
    }
}
