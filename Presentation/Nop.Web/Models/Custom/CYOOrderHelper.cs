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
        /// <summary>
        /// The sender is Booginhead, and this is their ID.
        /// </summary>
        public static readonly string SENDER_ID = "BOO007324";

        /// <summary>
        /// The receiver is PRIDE, and this is their ID.
        /// </summary>
        public static readonly string RECEIVER_ID = "100001803";

        public static readonly string DEFAULT_SHIPPING_METHOD_CODE = "904";
        public static readonly string BUY_FROM_BUSINESS_PARTNER = "WRV5";
        public static readonly string ADDRESS_QUALIFIER = "STBP";
        public static readonly string SALES_UNIT = "EA";

        private static readonly string DATE_FORMAT = "yyyyMMdd";


        private CYOOrderHelper() 
        {
            this.OrderDate = DateTime.MinValue;
            this.IsTestMessage = false;
            this.Country = "USA";
            this.Currency = "USD";
            this.Items = new List<LineItem>();
        }

        public string OrderNumber { get; set; }
        public DateTime OrderDate { get;  set; }
        public bool IsTestMessage { get; set; }
        public string ProductType { get; set; }
        public string PrideOrderId { get { return string.Format("BOO{0}", OrderNumber); } }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public List<LineItem> Items { get; set; }

        private string ReplacePipes(string input)
        {
            return input.Replace('|', '/');
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
            fields.Add(DEFAULT_SHIPPING_METHOD_CODE);

            AddBlanks(fields, 8);

            // Does this ever change?
            fields.Add(Currency);

            AddBlanks(fields, 5);

            // Sender Id - Again
            fields.Add(SENDER_ID);

            AddBlanks(fields, 2);

            // Business parter code
            fields.Add(BUY_FROM_BUSINESS_PARTNER);

            AddBlanks(fields, 8);

            // Sender Id - Yet again
            fields.Add(SENDER_ID);

            AddBlanks(fields, 10);

            // End of message marker
            fields.Add("SA2_END");

            return string.Join<string>("|", fields);
        }

        public string LineSA3()
        {
            List<string> fields = new List<string>(26);

            fields.Add("SA3");
            // Message Reference: Booginhead order number XXXXXX prefixed with "BOO"
            fields.Add(PrideOrderId);
            // Sender Id
            fields.Add(SENDER_ID);
            // Receiver Id
            fields.Add(RECEIVER_ID);
            fields.Add(ADDRESS_QUALIFIER);
            fields.Add(ReplacePipes(Country));
            fields.Add(ReplacePipes(Address1));
            fields.Add(ReplacePipes(Address2));
            fields.Add(ReplacePipes(Address3));
            fields.Add(ReplacePipes(Address4));

            AddBlanks(fields, 3);

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

            fields.Add(BUY_FROM_BUSINESS_PARTNER);

            AddBlanks(fields, 27);

            fields.Add(ReplacePipes(Country));
            fields.Add("");
            fields.Add(ReplacePipes(Country));

            // End marker
            fields.Add("SA5_END");
            
            return string.Join<string>("|", fields);
        }

        private void AddBlanks(List<string> fields, int howMany)
        {
            for (int i = 0; i < howMany; i++)
                fields.Add("");
        }

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
}