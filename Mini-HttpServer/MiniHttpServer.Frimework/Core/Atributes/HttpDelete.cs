using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Frimework.Core.Atributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpDelete : Attribute
    {
        public string? Route { get; set; }
        public HttpDelete(string route)
        {
            Route = route;
        }
        public HttpDelete()
        {
        }
    }
}
