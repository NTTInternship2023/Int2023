using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchTheWeb.Models
{
    public class SearchLogDTO
    {
        public int IdUser { get; set; }

        public DateTime Date { get; set; }

        //Action is "remove" or "search"
        public string Action { get; set; } = "";


        public string ActionDetail { get; set; } = "";
    }
}