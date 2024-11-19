using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Pages
{
    public class BasePage
    {
        protected readonly IPage Page;

        public BasePage(IPage page)
        {
            Page = page;
        }

        protected async Task WaitAndClick(string selector)
        {
            await Page.WaitForSelectorAsync(selector);
            await Page.ClickAsync(selector);
        }

        protected async Task WaitAndFill(string selector, string text)
        {
            await Page.WaitForSelectorAsync(selector);
            await Page.FillAsync(selector, text);
        }
    }
}
