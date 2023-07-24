namespace SearchTheWebServer.Validators
{
    public class FilterParametersValidator
    {
        public int startYear { get; set; } 
        public int endYear { get; set; }
        public string sortType { get; set; } = string.Empty;
        public FilterParametersValidator() { }
        public bool IsValidSort(string sortType)
        {
            if (sortType != null) {
                sortType.ToLower();
                if (sortType.Equals("decr") || sortType.Equals("incr"))
                    {
                    return true;
                }
                else return false;
            }
            else return true;
        }
        public bool IsValidYear(int? year) {
            if (startYear.GetType() == typeof(Int32) ||
                startYear.GetType() == typeof(Int16) ||
                startYear.GetType() == typeof(Int64)) return true;
            else return false;
        }
        public bool IsValidTimeInterval(int? startYear, int? endYear) {
       if(startYear!=null && endYear!=null)
            {
                if (startYear > endYear) return false;
                if (endYear > DateTime.Now.Year + 2) return false;
                if (startYear < 0 || endYear < 0) return false;
            }
        return true;
        }
    }
}
