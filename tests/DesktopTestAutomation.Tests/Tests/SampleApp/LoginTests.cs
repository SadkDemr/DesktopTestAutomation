using DesktopTestAutomation.Tests.Locators.SampleApp;

namespace DesktopTestAutomation.Tests.Tests.SampleApp;

[TestFixture]
[Category("SampleApp")]
[Category("Login")]
public class LoginTests : SampleAppUiTest
{
    [Test]
    public void ValidCredentials_OpensMainWindow()
    {
        LoginAs(TestData.ValidUsername, TestData.ValidPassword);

        Assert.That(GetText(MainWindowLocators.WelcomeText), Is.EqualTo($"Hoş geldin, {TestData.ValidUsername}"));
    }

    [TestCase("admin", "yanlis", "Kullanıcı adı veya şifre hatalı.", TestName = "WrongPassword_ShowsError")]
    [TestCase("baskasi", "admin123", "Kullanıcı adı veya şifre hatalı.", TestName = "UnknownUser_ShowsError")]
    [TestCase("", "admin123", "Kullanıcı adı ve şifre zorunludur.", TestName = "EmptyUsername_ShowsError")]
    [TestCase("admin", "", "Kullanıcı adı ve şifre zorunludur.", TestName = "EmptyPassword_ShowsError")]
    public void InvalidCredentials_ShowsError(string username, string password, string expectedError)
    {
        EnterCredentials(username, password);
        Invoke(LoginLocators.LoginButton);

        WaitForText(LoginLocators.ErrorText, expectedError);
        Assert.That(Exists(MainWindowLocators.Window), Is.False, "Hatalı girişte ana pencere açılmamalı.");
    }
}
