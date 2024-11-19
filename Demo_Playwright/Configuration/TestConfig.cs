using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Configuration
{
    public class TestConfig
    {
        public string BaseUrl { get; set; }
        public string Browser { get; set; }
        public string Environment { get; set; }
        public int Timeout { get; set; }
        public bool Headless { get; set; }
        public int SlowMo { get; set; }
        public string TraceDir { get; set; }
        public string ScreenshotDir { get; set; }
        public string VideoDir { get; set; }
        public string HarDir { get; set; }

        public static TestConfig Load()
        {
            var configJson = File.ReadAllText("Configuration/config.json");
            return JsonConvert.DeserializeObject<TestConfig>(configJson);
        }
    }
}
