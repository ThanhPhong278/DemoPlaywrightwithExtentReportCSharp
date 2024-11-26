using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Demo_Playwright.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_Playwright.Utilities
{
    public class ExtentReportManager
    {
        private static ExtentReports _extent;
        private static ExtentTest _test;
        private static string _reportPath;

        public static void InitializeReport()
        {
            try
            {
                // Get the current execution directory
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Create TestResults directory in the execution path
                _reportPath = Path.Combine(baseDirectory, "TestResults");

                // Ensure directory exists
                if (!Directory.Exists(_reportPath))
                {
                    Directory.CreateDirectory(_reportPath);
                }

                // Create ExtentReport HTML file path
                string reportFilePath = Path.Combine(_reportPath, "ExtentReport.html");

                // Initialize ExtentHtmlReporter
                var htmlReporter = new ExtentSparkReporter(reportFilePath);
                htmlReporter.Config.DocumentTitle = "AhaPlus Test Execution Report";
                htmlReporter.Config.ReportName = "AhaPlus Test Results";
                htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Standard;

                // Initialize ExtentReports
                _extent = new ExtentReports();
                _extent.AttachReporter(htmlReporter);
                
                _extent.AddSystemInfo("AhaPlus Site Testing", "Salon Admin");
                _extent.AddSystemInfo("Release", "R85");
                _extent.AddSystemInfo("Environment", TestConfig.Load().Environment);
                _extent.AddSystemInfo("Browser", TestConfig.Load().Browser);
                _extent.AddSystemInfo("User Name", Environment.UserName);
                _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize extent report: {ex.Message}");
                throw;
            }
        }

        public static ExtentTest CreateTest(string testName)
        {
            try
            {
                _test = _extent.CreateTest(testName);
                return _test;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create test in extent report: {ex.Message}");
                throw;
            }
        }

        public static void LogTestStep(Status status, string stepDescription)
        {
            try
            {
                if (_test != null)
                {
                    _test.Log(status, stepDescription);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log test step: {ex.Message}");
            }
        }

        public static void FlushReport()
        {
            try
            {
                if (_extent != null)
                {
                    _extent.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to flush extent report: {ex.Message}");
                throw;
            }
        }

        public static string GetReportPath()
        {
            return _reportPath;
        }
    }
}
