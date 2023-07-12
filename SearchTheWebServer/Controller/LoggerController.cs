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

namespace SearchTheWebServer.Controller
{
    [Route("Logger")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        protected readonly AppDbContext _db;
        
        public LoggerController(AppDbContext db){
            _db = db;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<List<SearchLog>>> GetAll(){
            return (await _db.SearchLogs.ToListAsync()).ToList();
        }

        [HttpGet]
        [Route("GetLast")]
        public async Task<SearchLog> GetLast(){
           var LastLog = await _db.SearchLogs.OrderByDescending(x => x.Id).FirstAsync();
           return LastLog;
        }

        [HttpPost]
        [Route("Suggestions")]
        public async Task<ActionResult<List<SearchLog>>> GetSearchSuggestions([FromBody]string hint){
            Console.WriteLine(hint);
            var searchSuggestions = (await _db.SearchLogs.Where(w=>w.ActionDetail.StartsWith(hint)).ToListAsync()).ToList();
            return searchSuggestions;
        }

        [HttpPost]
        [Route("LogAction")]
        public async Task<ActionResult<SearchLog>> LogAction([FromBody]SearchLogDTO searchLogDTO){
            try{
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
            }catch(Exception ex){
                return StatusCode(500,$"Error save log: {ex.Message}");
            }
        }
    }
}