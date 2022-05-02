using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteShop_Models.ViewModels
{
    public class ProductUserVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public List<Product> ProductList { get; set; }

        public ProductUserVM()
        {
            ProductList = new List<Product>();
        }
    }
}
