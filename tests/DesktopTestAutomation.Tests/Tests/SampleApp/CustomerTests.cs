using DesktopTestAutomation.Tests.Locators.SampleApp;

namespace DesktopTestAutomation.Tests.Tests.SampleApp;

[TestFixture]
[Category("SampleApp")]
[Category("Customers")]
public class CustomerTests : LoggedInSampleAppUiTest
{
    [Test]
    public void InitialList_ContainsSeedCustomers()
    {
        Assert.That(GetCustomers().Select(c => c.Name), Is.EqualTo(TestData.SeedCustomerNames));
    }

    [Test]
    public void AddCustomer_WithValidData_AppearsInGrid()
    {
        SetText(MainWindowLocators.NameTextBox, "Zeynep Demir");
        SetText(MainWindowLocators.EmailTextBox, "zeynep@example.com");
        Select(MainWindowLocators.CityComboBox, "İzmir");
        SetChecked(MainWindowLocators.IsActiveCheckBox, false);
        Invoke(MainWindowLocators.AddButton);

        WaitForText(MainWindowLocators.StatusText, "Müşteri eklendi: Zeynep Demir");

        var customers = GetCustomers();

        // Tüm doğrulamalar çalışır, hatalar sonunda birlikte raporlanır.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(customers, Has.Count.EqualTo(TestData.SeedCustomerNames.Count + 1));
            Assert.That(customers.Last(), Is.EqualTo(new CustomerRow("Zeynep Demir", "zeynep@example.com", "İzmir", "Hayır")));
            Assert.That(GetText(MainWindowLocators.CustomerCountText), Is.EqualTo("Toplam müşteri: 3"));
        }
    }

    [TestCase("", "ali@example.com", "Ad Soyad alanı zorunludur.", TestName = "AddCustomer_WithoutName_ShowsError")]
    [TestCase("Ali Veli", "gecersiz-eposta", "Geçerli bir e-posta adresi girin.", TestName = "AddCustomer_WithInvalidEmail_ShowsError")]
    public void AddCustomer_WithInvalidData_ShowsValidationError(string name, string email, string expectedError)
    {
        SetText(MainWindowLocators.NameTextBox, name);
        SetText(MainWindowLocators.EmailTextBox, email);
        Invoke(MainWindowLocators.AddButton);

        WaitForText(MainWindowLocators.StatusText, expectedError);
        Assert.That(GetCustomers(), Has.Count.EqualTo(TestData.SeedCustomerNames.Count));
    }

    [Test]
    public void DeleteCustomer_WhenConfirmed_RemovesRow()
    {
        SelectCustomer("Mehmet Kaya");
        Invoke(MainWindowLocators.DeleteButton);

        Assert.That(Find(ConfirmDialogLocators.Window).Name, Is.EqualTo("Onay"));
        Invoke(ConfirmDialogLocators.YesButton);

        WaitForText(MainWindowLocators.StatusText, "Müşteri silindi: Mehmet Kaya");
        Assert.That(GetCustomers().Select(c => c.Name), Is.EqualTo(new[] { "Ayşe Yılmaz" }));
    }

    [Test]
    public void DeleteCustomer_WhenCancelled_KeepsRow()
    {
        SelectCustomer("Mehmet Kaya");
        Invoke(MainWindowLocators.DeleteButton);
        Invoke(ConfirmDialogLocators.NoButton);

        WaitForText(MainWindowLocators.StatusText, "Silme işlemi iptal edildi.");
        Assert.That(GetCustomers().Select(c => c.Name), Is.EqualTo(TestData.SeedCustomerNames));
    }
}
