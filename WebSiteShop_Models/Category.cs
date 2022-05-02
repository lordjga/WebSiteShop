using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteShop_Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Name")]
        [Required]
        public string CategoryName { get; set; }
        [DisplayName("Display Order")]
        [Required]
        [Range(1, int.MaxValue,ErrorMessage ="Display order for category must be greater then 0")]
        public int DisplayOrder { get; set; }

    }
}
