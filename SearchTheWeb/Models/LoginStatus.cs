using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchTheWeb.Models
{
    public class LoginStatus
    {
        public int id { get; set;}
        public string username {get;set;} = "";
        public string message { get; set; } = "";
        public bool status { get; set; }=false;
        public bool isAdmin { get; set; }=false;
    }
}