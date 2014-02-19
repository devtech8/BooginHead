using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Admin.Models.Customers
{
    public class WholesalerModel : BaseNopEntityModel
    {
        public string TaxId { get; set; }
        public string WebsiteURL { get; set; }
        public bool International { get; set; }
        public string HowDidYouHear { get; set; }
        public string YearsInBusiness { get; set; }
        public string StoreFront { get; set; }
        public string TypeOfStore { get; set; }
        public string NameOfWebStore { get; set; }
        public string AmazonSellerName { get; set; }

    }
}