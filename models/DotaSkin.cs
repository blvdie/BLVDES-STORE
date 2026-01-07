using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVAYA_RABOTA.models
{
        public class DotaSkin
        {
            public int SkinId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string ImageSource { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
