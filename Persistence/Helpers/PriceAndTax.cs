using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Helpers
{
    public class PriceAndTax
    {
        decimal price;
        public decimal Price { get { return price; } }

        decimal tax;
        public decimal Tax { get { return tax; } }

        public PriceAndTax(decimal price, decimal tax)
        {
            this.price = price;
            this.tax = tax;
        }
    }
}
