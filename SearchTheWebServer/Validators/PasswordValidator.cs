using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SearchTheWebServer.Validators
{
    public static class PasswordValidator
    {
       public static bool PasswordPatternValidation(string? passwordInput)
{
    if (passwordInput == null)
    {
        return false; // or handle the null case as needed
    }

    var hasNumber = new Regex(@"[0-9]+");
    var hasUpperChar = new Regex(@"[A-Z]+");
    var hasMinimum8Chars = new Regex(@".{8,}");

    var isValidated = hasNumber.IsMatch(passwordInput) && hasUpperChar.IsMatch(passwordInput) && hasMinimum8Chars.IsMatch(passwordInput);

    return isValidated;
}

    }
}
