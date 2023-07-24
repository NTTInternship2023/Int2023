using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SearchTheWebServer.Validators
{
    public static class EmailValidator
    {
        public static bool EmailPatternValidation(string? emailInput)
        {
            var emailformat = new Regex("^[A-Za-z0-9._%+-]+@[a-z.-]+.(com|ro)$");
           var isValidated = emailInput != null && emailformat.IsMatch(emailInput);


            if (isValidated) return true;
            else return false;      
        }
    }
}
