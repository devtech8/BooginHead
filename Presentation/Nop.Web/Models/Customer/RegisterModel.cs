using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Customer;
using Foolproof;

namespace Nop.Web.Models.Customer
{
    [Validator(typeof(RegisterValidator))]
    public partial class RegisterModel : BaseNopModel
    {
        public RegisterModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Account.Fields.Email")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage="This field is required for wholesale registration.")]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Fields.ConfirmPassword")]
        [AllowHtml]
        public string ConfirmPassword { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Account.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }


        public bool DateOfBirthEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Company")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.StreetAddress")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [NopResourceDisplayName("Account.Fields.StreetAddress2")]
        [AllowHtml]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.City")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Country")]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.StateProvince")]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Phone")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [NopResourceDisplayName("Account.Fields.Fax")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string Fax { get; set; }
        
        public bool NewsletterEnabled { get; set; }
        [NopResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }
        
        public bool AcceptPrivacyPolicyEnabled { get; set; }

        //time zone
        [NopResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [NopResourceDisplayName("Account.Fields.VatNumber")]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }

        public bool DisplayCaptcha { get; set; }

        // Custom for Booginhead: Wholesaler
        [NopResourceDisplayName("Account.RegisterAsWholesaler")]
        [AllowHtml]
        public bool RegisterAsWholesaler { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string TaxId { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string WebsiteURL { get; set; }
        [AllowHtml]
        public bool International { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string HowDidYouHear { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string YearsInBusiness { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string StoreFront { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string TypeOfStore { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string NameOfWebStore { get; set; }
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string AmazonSellerName { get; set; }
        [AllowHtml]
        [EqualTo("RegisterAsWholesaler", ErrorMessage = "You must accept the terms for wholesale registration.")]
        public bool AcceptedTerms { get; set; }

        [NopResourceDisplayName("Account.Fields.StreetAddress")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string ShippingStreetAddress { get; set; }
        [NopResourceDisplayName("Account.Fields.StreetAddress2")]
        [AllowHtml]
        public string ShippingStreetAddress2 { get; set; }
        [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string ShippingZipPostalCode { get; set; }
        [NopResourceDisplayName("Account.Fields.City")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public string ShippingCity { get; set; }
        [NopResourceDisplayName("Account.Fields.Country")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public int ShippingCountryId { get; set; }
        [NopResourceDisplayName("Account.Fields.StateProvince")]
        [AllowHtml]
        [RequiredIf("RegisterAsWholesaler", true, ErrorMessage = "This field is required for wholesale registration.")]
        public int ShippingStateProvinceId { get; set; }

        public List<SelectListItem> StorefrontOptions
        {
            get
            {
                List<SelectListItem> items = new List<SelectListItem>();
                SelectListItem item1 = new SelectListItem();
                SelectListItem item0 = new SelectListItem();
                item0.Value = null;
                item0.Text = "-- Choose One --";
                item1.Text = item1.Value = "Bricks and Mortar";
                SelectListItem item2 = new SelectListItem();
                item2.Text = item2.Value = "Web";
                SelectListItem item3 = new SelectListItem();
                item3.Text = item3.Value = "Both";
                items.Add(item0);
                items.Add(item1);
                items.Add(item2);
                items.Add(item3);
                return items;
            }
        }

        public List<SelectListItem> YearsInBusinessOptions
        {
            get
            {
                List<SelectListItem> items = new List<SelectListItem>();
                SelectListItem item0 = new SelectListItem();
                item0.Value = null;
                item0.Text = "-- Choose One --";
                SelectListItem item1 = new SelectListItem();
                item1.Text = item1.Value = "Less than one";
                SelectListItem item2 = new SelectListItem();
                item2.Text = item2.Value = "1";
                SelectListItem item3 = new SelectListItem();
                item3.Text = item3.Value = "2";
                SelectListItem item4 = new SelectListItem();
                item4.Text = item4.Value = "3";
                SelectListItem item5 = new SelectListItem();
                item5.Text = item5.Value = "4";
                SelectListItem item6 = new SelectListItem();
                item6.Text = item6.Value = "5 or more";
                items.Add(item0);
                items.Add(item1);
                items.Add(item2);
                items.Add(item3);
                items.Add(item4);
                items.Add(item5);
                items.Add(item6);
                return items;
            }
        }
        // End Custom for Booginhead: Wholesaler

    }
}