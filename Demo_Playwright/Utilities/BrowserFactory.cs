using Demo_Playwright.Configuration;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Utilities
{
    public class BrowserFactory
    {
        private readonly TestConfig _config;

        public BrowserFactory(TestConfig config)
        {
            _config = config;
        }

        public async Task<IBrowser> CreateBrowser()
        {
            var playwright = await Playwright.CreateAsync();

            var browserType = _config.Browser.ToLower() switch
            {
                "chromium" => playwright.Chromium,
                "firefox" => playwright.Firefox,
                "webkit" => playwright.Webkit,
                _ => playwright.Chromium
            };

            return await browserType.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _config.Headless,
                SlowMo = _config.SlowMo
            });
        }
    }
}
