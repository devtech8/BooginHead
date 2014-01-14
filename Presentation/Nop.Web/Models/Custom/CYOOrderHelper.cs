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

        /// <summary>
        /// Date format string for PRIDE order file
        /// </summary>
        private static readonly string DATE_FORMAT = "yyyyMMdd";

        /// <summary>
        /// Time format string for PRIDE order file
        /// </summary>
        private static readonly string TIME_FORMAT = "Hmmss";

        private DateTime orderDate = DateTime.MinValue;
        private bool isTestMessage = false;

        private CYOOrderHelper() { }

        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZip { get; set; }
        public DateTime OrderDate { get { return orderDate; } set { orderDate = value; } }
        public bool IsTestMessage { get { return isTestMessage; } set { isTestMessage = value; } }
        public string ProductType { get; set; }



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
            DateTime now = DateTime.Now;
            List<string> fields = new List<string>();

            // Record type
            fields.Add("SA1");
            // Message Reference: Booginhead order number XXXXXX
            fields.Add(string.Format("BOO{0}", OrderNumber));
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
            fields.Add(now.ToString(DATE_FORMAT));
            // Old transmission reference: blank on purpose
            fields.Add("");
            // Transmission time: not required, so we're omitting it
            fields.Add(now.ToString(TIME_FORMAT));
            // Test Identifier
            fields.Add(isTestMessage ? "" : "T");
            // End of message marker
            fields.Add("SA1_END");
            
            return string.Join<string>("|", fields);
        }

        /// <summary>
        /// Returns teh SA2 line of the order, which is the
        /// "Order Header". This is required and must appear
        /// exactly once.
        /// </summary>
        /// <returns></returns>
        public string LineSA2()
        {
            List<string> fields = new List<string>();

            // Record type
            fields.Add("SA2");
            // Message Reference: Booginhead order number XXXXXX
            fields.Add(string.Format("BOO{0}", OrderNumber));
            // Sender Id
            fields.Add(SENDER_ID);


            return string.Join<string>("|", fields);
        }

        public string LineSA3()
        {
            return "";
        }


        public string LineSA4()
        {
            return "";
        }


        private string LineSA5()
        {
            return "";
        }

    
    }
}