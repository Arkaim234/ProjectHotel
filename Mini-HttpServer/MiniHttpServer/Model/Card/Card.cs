using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model.Card
{
    internal class Card
    {
        public string Title {  get; set; }
        public decimal Price { get; set; }
        public int Quantity {  get; set; }
        public string Img { get; set; }

    }
}
