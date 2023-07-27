using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchTheWebServer.Models
{
    public  class LoginStatus
    {
        public int Id { get; set;}
        public string Username { get; set;} = "";
        public string Message{ get; set;}="";
        public bool Status { get; set; }=false;
        public bool IsAdmin { get; set; }=false;
    }
}