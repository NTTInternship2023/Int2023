using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchTheWebServer.Models
{
    public class IdNume
    {
        public int IdUser { get; set; }
        public string Username { get; set; }

        public IdNume(int idUser, string username){
            IdUser = idUser;
            Username=username;
        }
    }
}