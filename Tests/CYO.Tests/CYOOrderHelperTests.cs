using System;
using Nop.Web.Models.Custom;
using NUnit.Framework;

namespace CYO.Tests
{
    [TestFixture]
    public class CYOOrderHelperTests
    {
        [Test]
        public void GetFormattedOrder_Test()
        {
            CYOOrderHelper orderHelper = new CYOOrderHelper();

            // Make sure defaults are what we expect
            Assert.AreEqual(DateTime.Now.Date, orderHelper.OrderDate.Date);
            Assert.AreEqual("USA", orderHelper.Country);
            Assert.AreEqual("USD", orderHelper.Currency);
            Assert.IsFalse(orderHelper.IsTestMessage);
            Assert.AreEqual(ShippingMethod.USPS, orderHelper.ShippingMethod);
            Assert.IsNotNull(orderHelper.Items);

            orderHelper.RecipientNameLine1 = "Springfield Toys";
            orderHelper.RecipientNameLine2 = "Edna Krabapple";
            orderHelper.Address1 = "501 Main St.";
            orderHelper.Address2 = "Suite E";
            orderHelper.City = "Springfield";
            orderHelper.Country = "USA";
            orderHelper.Currency = "USD";
            orderHelper.IsTestMessage = false;
            orderHelper.OrderNumber = "100000999";
            orderHelper.State = "OR";
            orderHelper.Zip = "95114";

            orderHelper.Items.Add(new LineItem("Part1", 10));
            orderHelper.Items.Add(new LineItem("Part2", 20));
            orderHelper.Items.Add(new LineItem("Part3", 30));
            orderHelper.Items.Add(new LineItem("Part4", 40));
            orderHelper.Items.Add(new LineItem("Part5", 50));

            //Console.WriteLine(orderHelper.GetFormattedOrder());
            Assert.AreEqual(ExpectedOutput, orderHelper.GetFormattedOrder());
        }

        public string ExpectedOutput = string.Format(@"SA1|BOO100000999|BOO007324|100001803|ORD001|BEMIS|ORD001||{0}|||test message|SA1_END
SA2|BOO100000999|BOO007324|100001803|{0}|{0}|{0}|100001803|||||904|||||||||USD||||||BOO007324|||S78001|||WRV5|||||||||BOO007324|SA2_END
SA3|BOO100000999|BOO007324|100001803|STBP|USA|Springfield Toys|Edna Krabapple|501 Main St.|Suite E|||95114|OR||||||||Springfield|Springfield||SA3_END
SA5|BOO100000999|BOO007324|100001803|1|1|||Part1|||10|{0}|{0}|{0}|EA||EA||||||||||||||||||||||||||||||||||S78001||||||||||||||||||||||||||||USA||USA|SA5_END
SA5|BOO100000999|BOO007324|100001803|2|2|||Part2|||20|{0}|{0}|{0}|EA||EA||||||||||||||||||||||||||||||||||S78001||||||||||||||||||||||||||||USA||USA|SA5_END
SA5|BOO100000999|BOO007324|100001803|3|3|||Part3|||30|{0}|{0}|{0}|EA||EA||||||||||||||||||||||||||||||||||S78001||||||||||||||||||||||||||||USA||USA|SA5_END
SA5|BOO100000999|BOO007324|100001803|4|4|||Part4|||40|{0}|{0}|{0}|EA||EA||||||||||||||||||||||||||||||||||S78001||||||||||||||||||||||||||||USA||USA|SA5_END
SA5|BOO100000999|BOO007324|100001803|5|5|||Part5|||50|{0}|{0}|{0}|EA||EA||||||||||||||||||||||||||||||||||S78001||||||||||||||||||||||||||||USA||USA|SA5_END", 
                                       DateTime.Now.ToString(CYOOrderHelper.DATE_FORMAT));
    }
}
