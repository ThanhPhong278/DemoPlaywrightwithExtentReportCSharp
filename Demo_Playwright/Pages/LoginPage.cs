using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Pages
{
    public class LoginPage : BasePage
    {
        private const string UsernameInput = "#salon_id";
        private const string PasswordInput = "#salon_pass";
        private const string LoginButton = "#frm_salon > p.mt20.mb10 > button";
        public const string UserAccountName = "#__BVID__227__BV_toggle_";

        public LoginPage(IPage page) : base(page) { }

        public async Task Login(string username, string password)
        {
            await WaitAndFill(UsernameInput, username);
            await WaitAndFill(PasswordInput, password);
            await WaitAndClick(LoginButton);
        }
    }
}
