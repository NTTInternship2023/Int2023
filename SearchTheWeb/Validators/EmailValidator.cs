using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SearchTheWeb.Validators
{
    public static class EmailValidator
    {
        public static bool EmailPatternValidation(string? emailInput)
        {
            emailInput ??= string.Empty;

            var emailformat = new Regex("^[A-Za-z0-9._%+-]+@[a-z.-]+.(com|ro)$");
            var isValidated = emailformat.IsMatch(emailInput);

            if (isValidated) return true;
            else return false;      
        }
    }
}
