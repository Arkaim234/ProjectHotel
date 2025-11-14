using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Frimework.Core.Atributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPut : Attribute
    {
        public string? Route { get; set; }
        public HttpPut(string route)
        {
            Route = route;
        }
        public HttpPut()
        {
        }
    }
}
