using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteShop_Utility
{
    //Для хранения констант
    public static class WC
    {
        public const string ImagePath = @"\images\product\";
        public const string SessionCart = "ShoppingCartSession";
        public const string SessionInquiryId = "InquirySession";

        public const string AdminRole = "Admin";
        public const string CastomerRole = "Castomer";

        public const string EmailAdmin = "lafer.art@mailfence.com";

        public const string CstegoryName = "Category";
        public const string ApplicationTypeName = "ApplicationType";

        public const string Success = "Success"; 
        public const string Error = "Error";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusProcessing = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static readonly IEnumerable<string> listStatus = new ReadOnlyCollection<string>(
            new List<string>
            {
                StatusPending,StatusApproved,StatusProcessing,StatusShipped,StatusCancelled,StatusRefunded
            });
    }
    //тестирование Git
    //тестирование TestBranch
    //тестирование TestBranch2
    //тестирование TestBranch3

    //тестирование TestBranch5
}
