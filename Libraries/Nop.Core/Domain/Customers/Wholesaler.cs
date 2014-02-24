using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Customers
{
    public partial class Wholesaler : BaseEntity
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
        public bool AcceptedTerms { get; set; }
    }
}
