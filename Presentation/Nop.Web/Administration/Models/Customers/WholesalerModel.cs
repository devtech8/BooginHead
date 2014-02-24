using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Nop.Admin.Models.Customers
{
    public class WholesalerModel : BaseNopEntityModel
    {
        [Required]
        public string TaxId { get; set; }
        [Required]
        public string WebsiteURL { get; set; }
        [Required]
        public bool International { get; set; }
        [Required]
        public string HowDidYouHear { get; set; }
        [Required]
        public string YearsInBusiness { get; set; }
        [Required]
        public string StoreFront { get; set; }
        [Required]
        public string TypeOfStore { get; set; }
        [Required]
        public string NameOfWebStore { get; set; }
        [Required]
        public string AmazonSellerName { get; set; }
        [Required]
        public bool AcceptedTerms { get; set; }
    }
}