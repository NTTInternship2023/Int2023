namespace SearchTheWebServer.Models;

public class SearchLog
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public DateTime Date { get; set; }

    //Action is "remove" or "search"
    public string Action { get; set; } = "";


    public string ActionDetail { get; set; } = "";
}