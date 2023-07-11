using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SearchTheWebServer.Data;
using SearchTheWebServer.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}