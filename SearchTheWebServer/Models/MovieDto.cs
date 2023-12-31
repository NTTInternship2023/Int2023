﻿namespace SearchTheWebServer.Models;

public class MovieDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public Dictionary<string,string?>? RegionalTitles { get; set; }
    public string? ImageUrl { get; set; }
    public int ReleaseYear { get; set; }
    
    public double Rating { get; set; }
    public int? NumVotes { get; set; } = null;
}