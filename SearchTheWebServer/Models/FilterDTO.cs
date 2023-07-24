using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchTheWebServer.Models
{
    public class FilterDTO
    {
        public string? Title { get; set; }
        public int? StartYear { get; set; }
        public string? sort { get; set; }
        public string? TitleType { get; set; }
        public int? EndYear { get; set; }
        public int IdUser { get; set; }
    }
}