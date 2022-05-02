using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteShop_Models.ViewModels
{
    public class DetailsVM

    {
        public DetailsVM()
        {
            Product = new Product();
        }
        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }
    }
}
