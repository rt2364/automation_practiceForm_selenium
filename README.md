# Automation Practice Form Test

## Task

Test any three fields of a web form using C# and Selenium. The automation must keep working even when the page changes, so it must not depend only on a hard-coded XPath or CSS selector.

## Aim

This project tests three fields: **First Name**, **Email** and **About Yourself**.

- First Name: type a value and check it.
- Email: type a value, check it, and validate the email format.
- About Yourself (text area): type a value and check it.

Form URL: PASTE_YOUR_FORM_LINK_HERE

## Project Structure

```
CloudAutomation/
├── Program.cs               # Starts Chrome, runs the tests, prints PASS/FAIL
├── Pages/
│   └── PracticeFormPage.cs  # Methods to fill the 3 fields
└── Locators/
    └── SmartLocator.cs      # Finds a field using its label text
```

## Setup and Run

1. Install the **.NET SDK** and **Google Chrome**.
2. Open a terminal in the project folder (where the `.csproj` file is).
3. Install the required package:
   dotnet add package Selenium.WebDriver
   If `WebDriverWait` is not found, also run: `dotnet add package Selenium.Support`
4. Run the automation: dotnet run

   dotnet run -- "http://app.cloudqa.io/home/AutomationPracticeForm"

Chrome opens, fills the fields, and closes. Expected result: `Result: 4 passed, 0 failed`

(You do not need to download ChromeDriver. Selenium does it automatically.)

## Locator Strategy (How Fields Are Found)

The code does not use a fixed XPath or CSS like `#fname` or `//input[3]`. It finds a field by its **visible label text** (for example, "First Name"), the same way a person reads the form. It tries three ways, one after another:

1. **label[for]**: the label points to the field's `id`.
2. **nearby label**: the field inside the label, or the first field after it.
3. **attribute score**: matches the label text with `placeholder`, `name`, `id`, `aria-label`, `title` and `type`. The best score wins.

The console shows which way worked, for example: `[locator] 'Email' found using: label[for]`

## How It Handles Changes in the Page

- **Position or order changes:** the field is found by label text, not by its place on the page.
- **`id`, `name` or `class` changes:** ways 1 and 2 do not depend on these values. Way 3 is only the last backup.
- **Small DOM changes:** the label can be a `label`, `span`, `div`, `p`, `td` or `th`. The field can be inside the label or after it, so extra wrapper tags do not matter.
- **XPath or CSS path changes:** no path is saved in the code.
- **Many similar elements:** when many labels match (like "Email" and "Confirm Email"), the exact match and the shortest text come first. The field must also be the right type (`input` or `textarea`), visible and enabled.
- **Slow or changing page:** the locator retries for up to 10 seconds instead of failing at once.

## Assumptions and Limitations

- The label text (for example, "First Name") stays almost the same. If the words change a lot, only the attribute backup can still find the field.
- The fields are simple text inputs or a text area. Radio buttons, checkboxes and dropdowns need extra methods.
- Fields inside an iframe or shadow DOM are not supported.
- Only Chrome is used. An internet connection is needed.
- The email check is basic: it looks for text, `@`, and a domain with a dot.
- If two fields have the same label, the first visible one is used.

## Required Fields and Safety Precautions

Only these three fields are treated as required. No other field is tested or required.

- **First Name is required.** The form should not submit without it.
- **Email is required and must be proper.** It must have `@` and a domain, like `priya@example.com`.
- **About Yourself is required.** Some text areas have a character limit (`maxlength`). If your text gets cut, use a shorter text.
- **Do not use `Thread.Sleep`.** Use `WebDriverWait` so the test waits for the page.
- **Always close the browser.** `driver.Quit()` is inside `finally`, so Chrome closes even if a test fails.
- **Use the correct label spelling.** The label text in the code must match the label on the page.

## Possible Errors and Fixes

**1. Namespace error (CS0234)**
- Cause: the `using` in `Program.cs` does not match the namespace in `PracticeFormPage.cs`.
- Fix: use the same namespace in both files: `using CloudQAAutomation.Pages;`

**2. `OpenQA` not found**
- Cause: the Selenium package is not installed.
- Fix: run `dotnet add package Selenium.WebDriver`

**3. Test fails because text is cut**
- Example: expected `Learning test automation.` but got `Learning test`.
- Cause: the field has a `maxlength`.
- Fix: use a shorter text, or check the field's `maxlength` in the browser.

**4. `NoSuchElementException: Could not find field`**
- Cause: the label text in the code does not match the page, or the page did not load.
- Fix: check the spelling of the label and the form link.

**5. `Cannot read properties of null (reading 'addEventListener')`**
- Cause: an element `id` was changed in the HTML (for example, `fname` to `xyz`), but the JavaScript still used the old `id`.
- Fix: update the JavaScript to use the new `id`.
  ```js
  // Before
  document.getElementById("fname").addEventListener(...)
  // After
  document.getElementById("xyz").addEventListener(...)
  ```