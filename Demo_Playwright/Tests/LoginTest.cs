using AventStack.ExtentReports;
using Demo_Playwright.Models;
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

        [Test]
        public async Task ValidLogin()
        {
            var testData = TestDataReader.ReadTestData<LoginTestData>("loginData");

            // Navigate to the login page
            await Page.GotoAsync(Config.BaseUrl);
            Test.Log(Status.Info, "Navigated to login page");

            // Perform login action
            await _loginPage.Login(testData.Username, testData.Password);
            Test.Log(Status.Info, "Entered login credentials");

            // Verify the user account element is visible
            //await Page.WaitForSelectorAsync("#app-header-block > div.header__top-bar > div > div.top-bar__tool-box > div.user-account");
            //Test.Log(Status.Pass, "Successfully logged in");
            var isUserAccountVisible = await Page.Locator(LoginPage.UserAccountName).IsVisibleAsync();

            if (isUserAccountVisible)
            {
                Test.Log(Status.Pass, "Successfully logged in. User account element is visible.");
            }
            else
            {
                Test.Log(Status.Fail, "Login failed. User account element is not visible.");
            }
        }

    }
}

