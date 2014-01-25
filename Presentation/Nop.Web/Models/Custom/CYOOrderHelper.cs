using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Custom
{
    /// <summary>
    /// This class helps generate the pipe-delimited file for PRIDE.
    /// </summary>
    public class CYOOrderHelper
    {

        # region Static Properties

        /// <summary>
        /// The sender is Booginhead, and this is their ID.
        /// </summary>
        public static readonly string SENDER_ID = "BOO007324";

        /// <summary>
        /// The receiver is PRIDE, and this is their ID.
        /// </summary>
        public static readonly string RECEIVER_ID = "100001803";
        public static readonly string BUY_FROM_BUSINESS_PARTNER = "S78001";
        public static readonly string SHIP_FROM_WAREHOUSE = "WRV5";
        public static readonly string ADDRESS_QUALIFIER = "STBP";
        public static readonly string SALES_UNIT = "EA";
        
        /// <summary>
        /// This maps Booginhead shipping options to PRIDE shipping codes.
        /// </summary>
        public static readonly Dictionary<ShippingMethod, string> SHIPPING_METHOD_CODES = new Dictionary<ShippingMethod, string>()
        {
            {ShippingMethod.USPS, "L13" },
            {ShippingMethod.FEDEX_2DAY, "904" },
            {ShippingMethod.FEDEX_GROUND, "907" },
            {ShippingMethod.FEDEX_PRIORITY_OVERNIGHT, "902" },
            {ShippingMethod.FEDEX_STANDARD_OVERNIGHT, "903" }
        };

        public static readonly string DATE_FORMAT = "yyyyMMdd";

        # endregion

        #region Private Properties

        private string _country = "USA";

        #endregion

        # region Constructor

        public CYOOrderHelper() 
        {
            this.OrderDate = DateTime.Now;
            this.IsTestMessage = false;
            this.Country = "USA";
            this.Currency = "USD";
            this.ShippingMethod = ShippingMethod.USPS;
            this.Items = new List<LineItem>();
        }

        # endregion

        # region Public Properties

        public string OrderNumber { get; set; }
        public DateTime OrderDate { get;  set; }
        public bool IsTestMessage { get; set; }
        public string PrideOrderId { get { return string.Format("BOO{0}", OrderNumber); } }
        public string RecipientNameLine1 { get; set; }
        public string RecipientNameLine2 { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    
        public ShippingMethod ShippingMethod { get; set; }        
        public List<LineItem> Items { get; set; }

        /// <summary>
        /// As of early 2014, PRIDE supports USD only.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// US should use 5-digit zip only. Other countries
        /// should have zip format validated. E.g. Canada
        /// should be "Z9Z 9Z9", not "Z9Z9Z9". PRIDE will 
        /// correct the zip format, but that slows things
        /// down.
        /// </summary>
        public string Zip { get; set; }        

        public string Country 
        {
            get
            {
                return this._country;
            }
            set
            {
                string country = value;
                if (!string.IsNullOrEmpty(country) && country.Length != 3)
                {
                    // Try to set correct 3-letter country code.
                    // If we can't do this, PRIDE will correct the order.
                    if (CountryCodes.ByName.ContainsKey(country.ToUpper()))
                        country = CountryCodes.ByName[country.ToUpper()];
                }
                this._country = country;
            }
        }

        # endregion

        # region Private Methods

        private string ReplacePipes(string input)
        {
            return input.Replace('|', '/');
        }

        private void AddBlanks(List<string> fields, int howMany)
        {
            for (int i = 0; i < howMany; i++)
                fields.Add("");
        }

        #endregion

        #region Public Methods

        public string GetFormattedOrder()
        {
            List<string> lines = new List<string>();
            lines.Add(LineSA1());
            lines.Add(LineSA2());
            lines.Add(LineSA3());
            for (int i = 0; i < this.Items.Count; i++)
            {
                lines.Add(LineSA5(this.Items[i], i + 1));
            }
            return string.Join<string>(Environment.NewLine, lines);
        }

        /// <summary>
        /// Returns the SA1 line of the PRIDE order. This is the
        /// "Message Overhead". It's required and must appear exactly
        /// once.
        /// </summary>
        /// <returns></returns>
        public string LineSA1() 
        {
            List<string> fields = new List<string>();

            // Record type
            fields.Add("SA1");
            // Message Reference: Booginhead order number XXXXXX with BOO prefix
            fields.Add(PrideOrderId);
            // Sender Id
            fields.Add(SENDER_ID);
            // Receiver Id
            fields.Add(RECEIVER_ID);
            // Message
            fields.Add("ORD001");
            // Organization 
            fields.Add("BEMIS");
            // Order Type
            fields.Add("ORD001");
            // Transmission Reference: blank on purpose
            fields.Add("");
            // Transmission date
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            // Old transmission reference: blank on purpose
            fields.Add("");
            // Transmission time: this is marked as required in
            // the BEMIS data format PDF doc, but PRIDE told me
            // in 1/17/2014 to leave this blank.
            fields.Add("");
            // Test Identifier
            fields.Add(IsTestMessage ? "" : "test message");
            // End of message marker
            fields.Add("SA1_END");
            
            return string.Join<string>("|", fields);
        }

        /// <summary>
        /// Returns the SA2 line of the order, which is the
        /// "Order Header". This is required and must appear
        /// exactly once.
        /// </summary>
        /// <returns></returns>
        public string LineSA2()
        {
            List<string> fields = new List<string>();

            // Record type
            fields.Add("SA2");
            // Message Reference: Booginhead order number XXXXXX prefixed with "BOO"
            fields.Add(PrideOrderId);
            // Sender Id
            fields.Add(SENDER_ID);
            // Receiver Id
            fields.Add(RECEIVER_ID);
            // Invoice Date
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            // Expected Delivery Date - don't know why , but we just set it to the order date
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            // Expected Delivery Date - Again
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            // Receiver Id - Again
            fields.Add(RECEIVER_ID);

            AddBlanks(fields, 4);

            // Shipping method code - is this something the user can change?
            fields.Add(SHIPPING_METHOD_CODES[this.ShippingMethod]);

            AddBlanks(fields, 8);

            // Does this ever change?
            fields.Add(Currency);

            AddBlanks(fields, 5);

            // Sender Id - Again
            fields.Add(SENDER_ID);

            AddBlanks(fields, 2);

            // Business parter code
            fields.Add(BUY_FROM_BUSINESS_PARTNER);

            AddBlanks(fields, 2);

            // Warehouse
            fields.Add(SHIP_FROM_WAREHOUSE);

            AddBlanks(fields, 8);

            fields.Add(SENDER_ID);

            // End of message marker
            fields.Add("SA2_END");

            return string.Join<string>("|", fields);
        }

        public string LineSA3()
        {
            List<string> fields = new List<string>();

            fields.Add("SA3");
            // Message Reference: Booginhead order number XXXXXX prefixed with "BOO"
            fields.Add(PrideOrderId);
            // Sender Id
            fields.Add(SENDER_ID);
            // Receiver Id
            fields.Add(RECEIVER_ID);
            fields.Add(ADDRESS_QUALIFIER);
            fields.Add(ReplacePipes(Country));
            fields.Add(ReplacePipes(RecipientNameLine1));
            fields.Add(ReplacePipes(RecipientNameLine2));
            fields.Add(ReplacePipes(Address1));
            fields.Add(ReplacePipes(Address2));

            AddBlanks(fields, 2);

            fields.Add(ReplacePipes(Zip));
            fields.Add(ReplacePipes(State));

            AddBlanks(fields, 7);

            // City appears twice on purpose
            fields.Add(ReplacePipes(City));
            fields.Add(ReplacePipes(City));

            // Another blank field
            fields.Add("");

            // End marker
            fields.Add("SA3_END");

            return string.Join<string>("|", fields);
        }

        /// <summary>
        /// This line appears multiple times, once for 
        /// each item in the order. Note that items can
        /// have quantities, so we would have 3 SA5 lines
        /// if the customer bough 10 units of item 1, plus
        /// 10 units of item 2, plus 10 units of item 3.
        /// </summary>
        /// <returns></returns>
        private string LineSA5(LineItem item, int itemNumber)
        {
            List<string> fields = new List<string>();

            fields.Add("SA5");
            // Message Reference: Booginhead order number XXXXXX prefixed with "BOO"
            fields.Add(PrideOrderId);
            // Sender Id
            fields.Add(SENDER_ID);
            // Receiver Id
            fields.Add(RECEIVER_ID);
            // Item number is added twice on purpose
            fields.Add(itemNumber.ToString());
            fields.Add(itemNumber.ToString());

            AddBlanks(fields, 2);

            // Here's the actual order
            fields.Add(item.PartNumber);

            AddBlanks(fields, 2);

            // And the quantity
            fields.Add(item.Quantity.ToString());

            // And the date 3 times, so they don't forget
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            fields.Add(OrderDate.ToString(DATE_FORMAT));
            fields.Add(OrderDate.ToString(DATE_FORMAT));

            // Sales unit: first time is for package quantity
            // Second time describes how to calculate the price.
            // Unit and price are "Each"
            fields.Add(SALES_UNIT);
            fields.Add("");
            fields.Add(SALES_UNIT);

            AddBlanks(fields, 33);

            fields.Add(SHIP_FROM_WAREHOUSE);

            AddBlanks(fields, 27);

            fields.Add(ReplacePipes(Country));
            fields.Add("");
            fields.Add(ReplacePipes(Country));

            // End marker
            fields.Add("SA5_END");
            
            return string.Join<string>("|", fields);
        }

        # endregion

    }

    public class LineItem
    {
        public LineItem() 
        {
            this.Quantity = 0;
        }

        public LineItem(string partNumber, int quantity)
        {
            this.PartNumber = partNumber;
            this.Quantity = quantity;
        }

        public string PartNumber { get; set; }
        public int Quantity { get; set; }
    }

    public enum ShippingMethod
    {
        USPS,
        FEDEX_2DAY,
        FEDEX_GROUND,
        FEDEX_PRIORITY_OVERNIGHT,
        FEDEX_STANDARD_OVERNIGHT
    }

    public class CountryCodes
    {
        /// <summary>
        /// Booginhead requires these country codes for shipping.
        /// </summary>
        public static Dictionary<string, string> ByName = new Dictionary<string,string>() {
            {"ARUBA", "ABW"},
            {"AFGHANISTAN", "AFG"},
            {"ANGOLA", "AGO"},
            {"ANGUILLA", "AIA"},
            {"ALBANIA", "ALB"},
            {"ANDORRA", "AND"},
            {"NETHERLANDS ANTILLES", "ANT"},
            {"UNITED ARAB EMIRATES", "ARE"},
            {"ARGENTINA", "ARG"},
            {"ARMENIA", "ARM"},
            {"AMERICAN SAMOA", "ASM"},
            {"ANTARCTICA", "ATA"},
            {"FRENCH SOUTHERN TERRITORIES", "ATF"},
            {"ANTIGUA AND BARBUDA", "ATG"},
            {"AUSTRALIA", "AUS"},
            {"AUSTRIA", "AUT"},
            {"AZERBAIJAN", "AZE"},
            {"BURUNDI", "BDI"},
            {"BELGIUM", "BEL"},
            {"BENIN", "BEN"},
            {"BURKINA FASO", "BFA"},
            {"BANGLADESH", "BGD"},
            {"BULGARIA", "BGR"},
            {"BAHRAIN", "BHR"},
            {"BAHAMAS", "BHS"},
            {"BOSNIA AND HERZEGOWINA", "BIH"},
            {"BELARUS", "BLR"},
            {"BELIZE", "BLZ"},
            {"BERMUDA", "BMU"},
            {"BOLIVIA", "BOL"},
            {"BRAZIL", "BRA"},
            {"BARBADOS", "BRB"},
            {"BRUNEI DARUSSALAM", "BRN"},
            {"BHUTAN", "BTN"},
            {"BOUVET ISLAND", "BVT"},
            {"BOTSWANA", "BWA"},
            {"CENTRAL AFRICAN REPUBLIC", "CAF"},
            {"CANADA", "CAN"},
            {"COCOS (KEELING) ISLANDS", "CCK"},
            {"SWITZERLAND", "CHE"},
            {"CHILE", "CHL"},
            {"CHINA", "CHN"},
            {"IVOIRE", "CIV"},
            {"CAMEROON", "CMR"},
            {"DEMOCRATIC REPUBLIC OF CONGO", "COD"},
            {"PEOPLES REPUBLIC OF CONGO", "COG"},
            {"COOK ISLANDS", "COK"},
            {"COLOMBIA", "COL"},
            {"COMOROS", "COM"},
            {"CAPE VERDE", "CPV"},
            {"COSTA RICA", "CRI"},
            {"CUBA", "CUB"},
            {"CHRISTMAS ISLAND", "CXR"},
            {"CAYMAN ISLANDS", "CYM"},
            {"CYPRUS", "CYP"},
            {"CZECH REPUBLIC", "CZE"},
            {"GERMANY", "DEU"},
            {"DJIBOUTI", "DJI"},
            {"DOMINICA", "DMA"},
            {"DENMARK", "DNK"},
            {"DOMINICAN REPUBLIC", "DOM"},
            {"ALGERIA", "DZA"},
            {"ECUADOR", "ECU"},
            {"EGYPT", "EGY"},
            {"ERITREA", "ERI"},
            {"WESTERN SAHARA", "ESH"},
            {"SPAIN", "ESP"},
            {"ESTONIA", "EST"},
            {"ETHIOPIA", "ETH"},
            {"FINLAND", "FIN"},
            {"FIJI", "FJI"},
            {"FALKLAND ISLANDS", "FLK"},
            {"FRANCE", "FRA"},
            {"FAROE ISLANDS", "FRO"},
            {"FEDERATED STATES OF MICRONESIA", "FSM"},
            {"FRANCE, METROPOLITAN", "FXX"},
            {"GABON", "GAB"},
            {"UNITED KINGDOM", "GBR"},
            {"GEORGIA", "GEO"},
            {"GHANA", "GHA"},
            {"GIBRALTAR", "GIB"},
            {"GUINEA", "GIN"},
            {"GUADELOUPE", "GLP"},
            {"GAMBIA", "GMB"},
            {"BISSAU", "GNB"},
            {"EQUATORIAL GUINEA", "GNQ"},
            {"GREECE", "GRC"},
            {"GRENADA", "GRD"},
            {"GREENLAND", "GRL"},
            {"GUATEMALA", "GTM"},
            {"FRENCH GUIANA", "GUF"},
            {"GUAM", "GUM"},
            {"GUYANA", "GUY"},
            {"HONG KONG", "HKG"},
            {"HEARD AND MC DONALD ISLANDS", "HMD"},
            {"HONDURAS", "HND"},
            {"CROATIA", "HRV"},
            {"HAITI", "HTI"},
            {"HUNGARY", "HUN"},
            {"INDONESIA", "IDN"},
            {"INDIA", "IND"},
            {"BRITISH INDIAN OCEAN TERRITORY", "IOT"},
            {"IRELAND", "IRL"},
            {"ISLAMIC REPUBLIC OF IRAN", "IRN"},
            {"IRAQ", "IRQ"},
            {"ICELAND", "ISL"},
            {"ISRAEL", "ISR"},
            {"ITALY", "ITA"},
            {"JAMAICA", "JAM"},
            {"JORDAN", "JOR"},
            {"JAPAN", "JPN"},
            {"KAZAKHSTAN", "KAZ"},
            {"KENYA", "KEN"},
            {"KYRGYZSTAN", "KGZ"},
            {"CAMBODIA", "KHM"},
            {"KIRIBATI", "KIR"},
            {"SAINT KITTS AND NEVIS", "KNA"},
            {"REPUBLIC OF KOREA (SOUTH)", "KOR"},
            {"KUWAIT", "KWT"},
            {"LAO PEOPLES DEMOCRATIC REPUBLI", "LAO"},
            {"LEBANON", "LBN"},
            {"LIBERIA", "LBR"},
            {"LIBYAN ARAB JAMAHIRIYA", "LBY"},
            {"SAINT LUCIA", "LCA"},
            {"LIECHTENSTEIN", "LIE"},
            {"SRI LANKA", "LKA"},
            {"LESOTHO", "LSO"},
            {"LITHUANIA", "LTU"},
            {"LUXEMBOURG", "LUX"},
            {"LATVIA", "LVA"},
            {"MACAU", "MAC"},
            {"MOROCCO", "MAR"},
            {"MONACO", "MCO"},
            {"REPUBLIC OF MOLDOVA", "MDA"},
            {"MADAGASCAR", "MDG"},
            {"MALDIVES", "MDV"},
            {"MEXICO", "MEX"},
            {"MARSHALL ISLANDS", "MHL"},
            {"MACEDONIA", "MKD"},
            {"MALI", "MLI"},
            {"MALTA", "MLT"},
            {"MYANMAR", "MMR"},
            {"MONGOLIA", "MNG"},
            {"NORTHERN MARIANA ISLANDS", "MNP"},
            {"MOZAMBIQUE", "MOZ"},
            {"MAURITANIA", "MRT"},
            {"MONTSERRAT", "MSR"},
            {"MARTINIQUE", "MTQ"},
            {"MAURITIUS", "MUS"},
            {"MALAWI", "MWI"},
            {"MALAYSIA", "MYS"},
            {"MAYOTTE", "MYT"},
            {"NAMIBIA", "NAM"},
            {"NEW CALEDONIA", "NCL"},
            {"NIGER", "NER"},
            {"NORFOLK ISLAND", "NFK"},
            {"NIGERIA", "NGA"},
            {"NICARAGUA", "NIC"},
            {"NIUE", "NIU"},
            {"THE NETHERLANDS", "NLD"},
            {"NORWAY", "NOR"},
            {"NEPAL", "NPL"},
            {"NAURU", "NRU"},
            {"NEW ZEALAND", "NZL"},
            {"OMAN", "OMN"},
            {"PAKISTAN", "PAK"},
            {"PANAMA", "PAN"},
            {"PITCAIRN", "PCN"},
            {"PERU", "PER"},
            {"PHILIPPINES", "PHL"},
            {"PALAU", "PLW"},
            {"PAPUA NEW GUINEA", "PNG"},
            {"POLAND", "POL"},
            {"PUERTO RICO", "PRI"},
            {"DEMOCRATIC PEOPLES REPUBLIC OF", "PRK"},
            {"PORTUGAL", "PRT"},
            {"PARAGUAY", "PRY"},
            {"PALESTINIAN TERRITORY, OCCUPIED", "PSE"},
            {"FRENCH POLYNESIA", "PYF"},
            {"QATAR", "QAT"},
            {"REUNION ISLAND", "REU"},
            {"ROMANIA", "ROU"},
            {"RUSSIAN FEDERATION", "RUS"},
            {"RWANDA", "RWA"},
            {"SAUDI ARABIA", "SAU"},
            {"SUDAN", "SDN"},
            {"SENEGAL", "SEN"},
            {"SINGAPORE", "SGP"},
            {"SOUTH GEORGIA AND THE SOUTH SA", "SGS"},
            {"ST. HELENA", "SHN"},
            {"SVALBARD AND JAN MAYEN ISLANDS", "SJM"},
            {"SOLOMON ISLANDS", "SLB"},
            {"SIERRA LEONE", "SLE"},
            {"EL SALVADOR", "SLV"},
            {"SAN MARINO", "SMR"},
            {"SOMALIA", "SOM"},
            {"ST. PIERRE AND MIQUELON", "SPM"},
            {"SAO TOME AND PRINCIPE", "STP"},
            {"SURINAME", "SUR"},
            {"SLOVAKIA", "SVK"},
            {"SLOVENIA", "SVN"},
            {"SWEDEN", "SWE"},
            {"SWAZILAND", "SWZ"},
            {"SEYCHELLES", "SYC"},
            {"SYRIAN ARAB REPUBLIC", "SYR"},
            {"TURKS AND CAICOS ISLANDS", "TCA"},
            {"CHAD", "TCD"},
            {"TOGO", "TGO"},
            {"THAILAND", "THA"},
            {"TAJIKISTAN", "TJK"},
            {"TOKELAU", "TKL"},
            {"TURKMENISTAN", "TKM"},
            {"EAST TIMOR", "TLS"},
            {"TONGA", "TON"},
            {"TRINIDAD AND TOBAGO", "TTO"},
            {"TUNISIA", "TUN"},
            {"TURKEY", "TUR"},
            {"TUVALU", "TUV"},
            {"TAIWAN", "TWN"},
            {"UNITED REPUBLIC OF TANZANIA", "TZA"},
            {"UGANDA", "UGA"},
            {"UKRAINE", "UKR"},
            {"UNITED STATES MINOR OUTLYING I", "UMI"},
            {"URUGUAY", "URY"},
            {"UNITED STATES", "USA"},
            {"UZBEKISTAN", "UZB"},
            {"VATICAN CITY STATE", "VAT"},
            {"SAINT VINCENT AND THE GRENADIN", "VCT"},
            {"VENEZUELA", "VEN"},
            {"VIRGIN ISLANDS, BRITISH", "VGB"},
            {"VIRGIN ISLANDS, US", "VIR"},
            {"VIET NAM", "VNM"},
            {"VANUATU", "VUT"},
            {"WALLIS AND FUTUNA ISLANDS", "WLF"},
            {"SAMOA", "WSM"},
            {"YEMEN", "YEM"},
            {"YUGOSLAVIA", "YUG"},
            {"SOUTH AFRICA", "ZAF"},
            {"ZAMBIA", "ZMB"},
            {"ZIMBABWE", "ZWE"}
        };
    }
}