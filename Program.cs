using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using CloudQAAutomation.Pages;

var options = new ChromeOptions();
options.AddArgument("--start-maximized");

IWebDriver driver = new ChromeDriver(options);
int passed = 0, failed = 0;

// compares the value we typed with the value the field really has
void Check(string name, string expected, string actual)
{
    if (expected == actual) { passed++; Console.WriteLine($"PASS: {name}"); }
    else { failed++; Console.WriteLine($"FAIL: {name} (expected '{expected}', got '{actual}')"); }
}

// checks a true/false condition (used for validation)
void CheckTrue(string name, bool condition)
{
    if (condition) { passed++; Console.WriteLine($"PASS: {name}"); }
    else { failed++; Console.WriteLine($"FAIL: {name}"); }
}

try
{
    var page = new PracticeFormPage(driver);

    // optional: dotnet run -- "your form link"
    page.Open(args.Length > 0 ? args[0] : null);

    // Field 1: text input - enter data and check it
    Check("First Name", "Priya", page.EnterFirstName("Priya"));

    // Field 2: email input - enter data, check it, and validate the email format
    string email = page.EnterEmail("priya@example.com");
    Check("Email", "priya@example.com", email);
    CheckTrue("Email format is valid", Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"));

    // Field 3: text area - enter data and check it
    Check("About Yourself", "Learning test automation.", page.EnterAboutYourself("Learning test automation."));
}
catch (Exception ex)
{
    failed++;
    Console.WriteLine("ERROR: " + ex.Message);
}
finally { driver.Quit(); }

Console.WriteLine($"\nResult: {passed} passed, {failed} failed");