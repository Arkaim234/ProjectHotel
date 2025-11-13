using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.DTOs
{
    public class LoginDto
    {
        public bool IsEmployee { get; set; } 
        public string Login { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty; 
        public string ClaimValue { get; set; } = string.Empty; 
    }
}