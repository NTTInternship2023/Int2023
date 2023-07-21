using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchTheWeb.Controller;
using SearchTheWeb.Models;

using SearchTheWeb.Data;

using Microsoft.AspNetCore.Mvc;

namespace SearchTheWeb.Controller
{
    public class LoginRegisterCheck
    {
        static readonly AppDbContext? tempContext;
    
        readonly UserController controller = new(tempContext);
        User user = new();
        private readonly HttpClient httpClient;

        /*
        public LoginRegisterCheck(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        } */

        public async Task<(bool, string?)> LoginCheck(LoginUserDto loginUserDto){
            var result = await controller.Login(loginUserDto);
            return (result.Item1, result.Item2.Value);
        } 

        public async Task<(bool, string?)> RegisterCheck(RegisterUserDto registerUserDto)
        {
            var result = await controller.Register(registerUserDto);
            if (result.Result is ConflictObjectResult conflictResult)
            {
                string? message = conflictResult.Value as string;
                return (false, message);
            } else if (result.Result is OkObjectResult okResult)
            {
                string? message = okResult.Value as string;
                return (true, message);
            }
            return (false, "Unknown error occured");
        }
        
    }
}