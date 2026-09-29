// Fields used: First Name, Email, About Yourself
using OpenQA.Selenium;
using CloudQAAutomation.Locators;

namespace CloudQAAutomation.Pages{
    public class PracticeFormPage{
        private readonly IWebDriver _driver;
        private readonly SmartLocator _locator;
        private const string Url = "http://app.cloudqa.io/home/AutomationPracticeForm";

        public PracticeFormPage(IWebDriver driver){
            _driver = driver;
            _locator = new SmartLocator(driver);
        }

        public void Open(string? url = null) => _driver.Navigate().GoToUrl(url ?? Url);
        public string EnterFirstName(string value) => Fill("First Name", value, "input");
        public string EnterEmail(string value) => Fill("Email", value, "input");

        public string EnterAboutYourself(string value) => Fill("About Yourself", value, "textarea");

        // types the value and returns what the field actually contains
        private string Fill(string label, string value, string tag){
            var field = _locator.FindField(label, tag);
            field.Clear();
            field.SendKeys(value);
            return field.GetAttribute("value") ?? string.Empty;
        }
    }
}