using AventStack.ExtentReports;
using Demo_Playwright.Configuration;
using Demo_Playwright.Utilities;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Demo_Playwright.Tests
{
    public class BaseTest
    {
        protected IBrowser Browser;
        protected IPage Page;
        protected TestConfig Config;
        protected ExtentTest Test;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Config = TestConfig.Load();
            ExtentReportManager.InitializeReport();
        }

        [SetUp]
        public async Task Setup()
        {
            var browserFactory = new BrowserFactory(Config);
            Browser = await browserFactory.CreateBrowser();

            // Ensure HAR and video directories exist
            Directory.CreateDirectory(Config.VideoDir);
            Directory.CreateDirectory(Config.HarDir);

            var context = await Browser.NewContextAsync(new()
            {
                RecordVideoDir = Config.VideoDir,
                RecordHarPath = Path.Combine(Config.HarDir, $"{TestContext.CurrentContext.Test.Name}.har")
            });

            // Start tracing
            await context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true
            });

            Page = await context.NewPageAsync();
            Test = ExtentReportManager.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public async Task Teardown()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var status = TestContext.CurrentContext.Result.Outcome.Status;

            try
            {
                // Capture screenshot and log result
                await CaptureScreenshot(status);

                // Stop tracing
                if (Page?.Context != null)
                {
                    await Page.Context.Tracing.StopAsync(new()
                    {
                        Path = Path.Combine(Config.TraceDir, $"{testName}.zip")
                    });
                }
            }
            catch (Exception ex)
            {
                Test?.Log(Status.Warning, $"Error during teardown: {ex.Message}");
            }
            finally
            {
                // Clean up browser resources
                if (Browser != null)
                {
                    await Browser.DisposeAsync();
                }
            }
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            ExtentReportManager.FlushReport();
        }

        private async Task CaptureScreenshot(TestStatus status)
        {
            if (Page == null)
            {
                Test?.Log(Status.Warning, "Page instance is null. Skipping screenshot capture.");
                return;
            }

            try
            {
                // Create screenshot directory
                string screenshotDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
                Directory.CreateDirectory(screenshotDir);

                // Generate screenshot filename
                string statusSuffix = status.ToString().ToLower();
                string testName = TestContext.CurrentContext.Test.Name;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string screenshotPath = Path.Combine(screenshotDir, $"{testName}_{statusSuffix}_{timestamp}.png");

                // Capture screenshot
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });

                // Log screenshot to report
                Test?.Log(
                    status == TestStatus.Passed ? Status.Pass :
                    status == TestStatus.Failed ? Status.Fail :
                    status == TestStatus.Skipped ? Status.Skip : Status.Warning,
                    MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build()
                );
            }
            catch (Exception ex)
            {
                Test?.Log(Status.Error, $"Failed to capture screenshot: {ex.Message}");
            }
        }
    }
}
