using AventStack.ExtentReports;
using Demo_Playwright.Pages;
using Demo_Playwright.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Tests
{
    public class LoginTest : BaseTest
    {
        private LoginPage _loginPage;

        [SetUp]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Page);
        }

        public class LoginTestData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [Test]
        public async Task ValidLogin()
        {
            var testData = TestDataReader.ReadTestData<LoginTestData>("loginData");

            await Page.GotoAsync($"{Config.BaseUrl}");
            Test.Log(Status.Info, "Navigated to login page");

            await _loginPage.Login(testData.Username, testData.Password);
            Test.Log(Status.Info, "Entered login credentials");

            await Page.WaitForSelectorAsync("#app-header-block > div.header__top-bar > div > div.top-bar__tool-box > div.user-account");
            Test.Log(Status.Pass, "Successfully logged in");
        }
    }
}
