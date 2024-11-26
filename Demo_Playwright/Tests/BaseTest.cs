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

        // One-time setup for the entire test suite
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Load test configuration
            Config = TestConfig.Load();

            // Initialize the Extent Report manager
            ExtentReportManager.InitializeReport();
        }

        // Setup method executed before each test
        [SetUp]
        public async Task Setup()
        {
            // Create a browser instance using the configuration
            var browserFactory = new BrowserFactory(Config);
            Browser = await browserFactory.CreateBrowser();

            // Ensure directories for video and HAR files exist
            Directory.CreateDirectory(Config.VideoDir);
            Directory.CreateDirectory(Config.HarDir);

            // Create a new browser context with video recording and HAR options
            var context = await Browser.NewContextAsync(new()
            {
                RecordVideoDir = Config.VideoDir,
                RecordHarPath = Path.Combine(Config.HarDir, $"{TestContext.CurrentContext.Test.Name}.har")
            });

            // Start tracing for detailed logging
            await context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true
            });

            // Create a new page and associate it with the current test
            Page = await context.NewPageAsync();
            Test = ExtentReportManager.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        // Teardown method executed after each test
        [TearDown]
        public async Task Teardown()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            string renamedVideoPath = Path.Combine(Config.VideoDir, $"{testName}.webm");

            try
            {
                // Capture screenshot and stop tracing for the test
                await LogMediaToReport(status, testName);

                // Stop tracing if the browser context is still open
                if (Page?.Context != null && IsContextOpen(Page.Context))
                {
                    await Page.Context.Tracing.StopAsync(new()
                    {
                        Path = Path.Combine(Config.TraceDir, $"{testName}.zip")
                    });
                }
            }
            catch (Exception ex)
            {
                // Log any errors encountered during teardown
                Test?.Log(Status.Warning, $"Error during teardown: {ex.Message}");
            }
            finally
            {
                // Ensure context and browser are closed
                await CloseContextAndPage();

                if (Browser != null)
                {
                    await Browser.DisposeAsync();
                }

                // Process video files asynchronously after teardown
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleVideoAsync(testName, renamedVideoPath);
                    }
                    catch (Exception ex)
                    {
                        // Log any errors encountered while handling video files
                        Test?.Log(Status.Error, $"Error handling video files: {ex.Message}");
                    }
                });
            }
        }

        // One-time teardown for the entire test suite
        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            try
            {
                // Finalize the Extent Report
                ExtentReportManager.FlushReport();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during global teardown: {ex.Message}");
            }
        }

        // Logs screenshots and attaches them to the report
        public async Task LogMediaToReport(TestStatus status, string testName)
        {
            if (Page == null || !IsPageOpen(Page))
            {
                // Skip media capture if the page is null or already closed
                Test?.Log(Status.Warning, "Page is null or already closed. Skipping media capture.");
                return;
            }

            // Define the screenshot directory
            string screenshotDir = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "screenshots");
            Directory.CreateDirectory(screenshotDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string statusSuffix = status.ToString().ToLower();

            // Define the screenshot file path
            string screenshotPath = Path.Combine(screenshotDir, $"{testName}_{statusSuffix}_{timestamp}.png");

            try
            {
                // Capture a full-page screenshot
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });

                // Attach the screenshot to the report
                Test?.Log(
                    status == TestStatus.Passed ? Status.Pass :
                    status == TestStatus.Failed ? Status.Fail :
                    status == TestStatus.Skipped ? Status.Skip : Status.Warning,
                    MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build()
                );
            }
            catch (Exception ex)
            {
                // Log any errors encountered during screenshot capture
                Test?.Log(Status.Error, $"Error while capturing screenshot: {ex.Message}");
            }
        }

        // Processes and moves the video file to the appropriate directory
        private async Task HandleVideoAsync(string testName, string renamedVideoPath)
        {
            try
            {
                // Find all saved video files in the video directory
                var savedVideos = Directory.GetFiles(Config.VideoDir, "*.webm");
                if (savedVideos.Length > 0)
                {
                    string generatedVideoPath = savedVideos[0];

                    // Wait until the video file is ready
                    await WaitForFileToBeReady(generatedVideoPath);

                    // Rename and move the video file
                    if (File.Exists(renamedVideoPath))
                    {
                        File.Delete(renamedVideoPath);
                    }

                    File.Move(generatedVideoPath, renamedVideoPath);

                    // Attach the video to the report if the file exists
                    if (File.Exists(renamedVideoPath))
                    {
                        string absoluteVideoPath = Path.GetFullPath(renamedVideoPath).Replace("\\", "/");
                        string videoHtml = $"<video width='800' controls><source src='file:///{absoluteVideoPath}' type='video/webm'>Your browser does not support the video tag.</video>";
                        Test?.Log(Status.Info, "Test execution video:" + videoHtml);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing video for test {testName}: {ex.Message}");
            }
        }

        // Waits until the file is ready for use or timeout occurs
        private async Task WaitForFileToBeReady(string path, int timeoutInSeconds = 10)
        {
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            var start = DateTime.Now;

            while ((DateTime.Now - start) < timeout)
            {
                if (IsFileReady(path))
                    return;

                // Poll every 100ms to check file readiness
                await Task.Delay(100);
            }

            throw new IOException($"File {path} not ready within timeout.");
        }

        // Checks if a file is ready for access
        private bool IsFileReady(string path)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        // Verifies if a browser context is still open
        private bool IsContextOpen(IBrowserContext context)
        {
            try
            {
                _ = context.Pages;
                return true;
            }
            catch (PlaywrightException)
            {
                return false;
            }
        }

        // Verifies if a page is still open
        private bool IsPageOpen(IPage page)
        {
            try
            {
                _ = page.Url;
                return true;
            }
            catch (PlaywrightException)
            {
                return false;
            }
        }

        // Closes the browser context and page gracefully
        private async Task CloseContextAndPage()
        {
            try
            {
                if (Page?.Context != null && IsContextOpen(Page.Context))
                {
                    await Page.Context.CloseAsync();
                }

                if (Page != null && IsPageOpen(Page))
                {
                    await Page.CloseAsync();
                }
            }
            catch (PlaywrightException ex)
            {
                // Log any errors encountered during context or page closure
                Test?.Log(Status.Warning, $"Error closing context or page: {ex.Message}");
            }
        }
    }
}