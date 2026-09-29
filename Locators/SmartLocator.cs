using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CloudQAAutomation.Locators{
    // Finds form fields using the visible label text instead of a fixed id/xpath/css.
    public class SmartLocator {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public SmartLocator(IWebDriver driver){
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            _wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        }

        public IWebElement FindField(string labelText, params string[] tags){
            try{
                return _wait.Until(d => TryFind(labelText, tags))!;
            }
            catch (WebDriverTimeoutException){
                throw new NoSuchElementException("Could not find field: " + labelText);
            }
        }

        private IWebElement? TryFind(string labelText, string[] tags){
            // Strategy 1: <label for="..."> points to the field's id
            var el = ByLabelFor(labelText, tags);
            if (el != null) { Log("label[for]", labelText); return el; }

            // Strategy 2: field inside the label, or the first field after the label text
            el = ByNearbyLabel(labelText, tags);
            if (el != null) { Log("nearby label", labelText); return el; }

            // Strategy 3 (last option): score attributes (placeholder, name, id, aria-label...)
            el = ByAttributeScore(labelText, tags);
            if (el != null) { Log("attribute score", labelText); return el; }

            return null;
        }

        private IWebElement? ByLabelFor(string text, string[] tags){
            foreach (var label in FindLabels(text)){
                var forId = label.GetAttribute("for");
                if (string.IsNullOrEmpty(forId)) continue;

                var match = _driver.FindElements(By.Id(forId))
                    .FirstOrDefault(e => IsUsable(e) && TagOk(e, tags));
                if (match != null) return match;
            }
            return null;
        }

        private IWebElement? ByNearbyLabel(string text, string[] tags){
            foreach (var label in FindLabels(text)){
                foreach (var tag in tags){
                    // field inside the label, or the first one that follows it
                    var candidates = label.FindElements(By.XPath(
                        $".//{tag} | following::{tag}[1]"));
                    var match = candidates.FirstOrDefault(IsUsable);
                    if (match != null) return match;
                }
            }
            return null;
        }

        private IWebElement? ByAttributeScore(string text, string[] tags) {
            string key = Normalize(text);
            string[] attrs = { "placeholder", "name", "id", "aria-label", "title", "type" };

            return _driver.FindElements(By.CssSelector("input, textarea"))
                .Where(e => IsUsable(e) && TagOk(e, tags)).Select(e => new
                {
                    Element = e,
                    // exact match = 2 points, partial match = 1 point
                    Score = attrs.Sum(a =>{
                        string v = Normalize(e.GetAttribute(a));
                        return v == key ? 2 : (v.Contains(key) ? 1 : 0);
                    })
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Element)
                .FirstOrDefault();
        }

        // any element (label, span, div...) whose text matches, not only <label>.
        // When many elements match (e.g. "Email" and "Confirm Email"),
        // the exact match comes first, then the shortest text.
        private List<IWebElement> FindLabels(string text){
            string lower = text.ToLower();
            string key = Normalize(text);
            string xpath =
                "//*[self::label or self::span or self::div or self::p or self::td or self::th]" +
                "[contains(translate(normalize-space(text()),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'" + lower + "')]";

            return _driver.FindElements(By.XPath(xpath))
                .Where(e => e.Displayed)
                .OrderBy(e => Normalize(e.Text) == key ? 0 : 1)
                .ThenBy(e => e.Text.Length)
                .ToList();
        }
        private static bool IsUsable(IWebElement e) => e.Displayed && e.Enabled;
        private static bool TagOk(IWebElement e, string[] tags) =>
            tags.Length == 0 || tags.Contains(e.TagName.ToLower());

        // ignores spaces, _, -, * and : so "First Name *" equals "first_name"
        private static string Normalize(string? s) =>
            (s ?? "").Replace(" ", "").Replace("_", "").Replace("-", "")
                     .Replace("*", "").Replace(":", "").ToLower();

        private static void Log(string strategy, string field) =>
            Console.WriteLine($"   [locator] '{field}' found using: {strategy}");
    }
}