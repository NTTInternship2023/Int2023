using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SearchTheWebServer.Data;
using SearchTheWebServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace SearchTheWebServer.Controller
{
    [Route("Logger")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        protected readonly AppDbContext _db;
        private readonly ILogger<LoggerController> _logger; // Add the logger
        public class LogItem
        {
            public string? Keyword { get; set; }
            public int TotalSearches { get; set; }
            public int WeekNumber { get; set; }
        }

        public LoggerController(AppDbContext db, ILogger<LoggerController> logger) // Modify the constructor
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAll")]
        [DisableCors]
        public async Task<ActionResult<List<SearchLog>>> GetAll(){
            return (await _db.SearchLogs.ToListAsync()).ToList();
        }

        [HttpGet]
        [Route("GetLast")]
        public async Task<SearchLog> GetLast()
        {
            var LastLog = await _db.SearchLogs.OrderByDescending(x => x.Id).FirstAsync();
            return LastLog;
        }

        [HttpGet]
        [Route("Suggestions")]
        public async Task<ActionResult<HashSet<string>>> GetSearchSuggestions( string hint)
        {
            var searchSuggestions = (await _db.SearchLogs.Where(w => w.ActionDetail.Contains(hint)).ToListAsync()).ToList();
            HashSet<string> suggestions = new HashSet<string>();
            foreach (var search in searchSuggestions){
                suggestions.Add(search.ActionDetail);
            }
            return suggestions;
        }

        [HttpPost]
        [Route("LogAction")]
        public async Task<ActionResult<SearchLog>> LogAction([FromBody] SearchLogDTO searchLogDTO)
        {
            try
            {
                SearchLog newSearchLog = new()
                {
                    IdUser = searchLogDTO.IdUser,
                    Date = searchLogDTO.Date,
                    Action = searchLogDTO.Action,
                    ActionDetail = searchLogDTO.ActionDetail
                };
                _db.SearchLogs.Add(newSearchLog);
                await _db.SaveChangesAsync();
                return Ok($"Search for {searchLogDTO.ActionDetail}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error save log: {ex.Message}");
            }
        }

        // Add the new endpoint
  [HttpGet]
[Route("SearchAction")]
public ActionResult<IEnumerable<LogItem>> GetLogItems(int? limit = null)
{
    try
    {
        var logItems = _db.SearchLogs.Where(log => log.Action == "Search")
            .AsEnumerable() // Perform client-side evaluation
            .GroupBy(log => new { log.ActionDetail, WeekNumber = GetWeekNumber(log.Date) })
            .Select(group => new LogItem
            {
                Keyword = group.Key.ActionDetail,
                TotalSearches = group.Count(),
                WeekNumber = group.Key.WeekNumber
            })
            .OrderByDescending(item => item.TotalSearches)
            .ThenBy(item => item.Keyword)
            .ToList();

        if (limit.HasValue && limit.Value > 0)
        {
            if (limit.Value >= logItems.Count)
            {
                return Ok(logItems);
            }
            else
            {
                logItems = logItems.Take(limit.Value).ToList();
            }
        }

        return Ok(logItems);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving log items: {Message}", ex.Message);
        return StatusCode(500, $"An error occurred while retrieving log items: {ex.Message}");
    }
}

        private static int GetWeekNumber(DateTime date)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            return ci.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
