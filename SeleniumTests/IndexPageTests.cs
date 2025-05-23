using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace SeleniumTests
{
    public class IndexPageTests : IDisposable
    {
        private readonly Process _process;
        private readonly IWebDriver _driver;

        public IndexPageTests()
        {
            var startInfo = new ProcessStartInfo("dotnet", "run --no-build --urls http://localhost:5005")
            {
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Full-DAR-Redaction"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            _process = Process.Start(startInfo)!;

            var client = new HttpClient();
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    var resp = client.GetAsync("http://localhost:5005").Result;
                    if (resp.IsSuccessStatusCode) break;
                }
                catch
                {
                    // ignore until server is ready
                }
                Thread.Sleep(1000);
            }

            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            _driver = new ChromeDriver(options);
        }

        [Fact]
        public void IndexPage_ContainsHeading()
        {
            _driver.Navigate().GoToUrl("http://localhost:5005");
            Assert.Contains("Convert Draft Assessment Report Part I", _driver.PageSource);
        }

        public void Dispose()
        {
            _driver?.Quit();
            if (!_process.HasExited)
            {
                _process.Kill();
                _process.WaitForExit();
            }
            _process.Dispose();
        }
    }
}
