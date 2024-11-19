using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Utilities
{
    public class TestDataReader
    {
        public static T ReadTestData<T>(string filename)
        {
            var jsonContent = File.ReadAllText($"TestData/{filename}.json");
            return JsonConvert.DeserializeObject<T>(jsonContent);
        }
    }
}
