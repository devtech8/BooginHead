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
            Assert.AreEqual(ShippingMethod.DEFAULT, orderHelper.ShippingMethod);
            Assert.IsNotNull(orderHelper.Items);

            orderHelper.Address1 = "Address Line 1";
            orderHelper.Address2 = "Address Line 2";
            orderHelper.Address3 = "Address Line 3";
            orderHelper.Address4 = "Address Line 4";
            orderHelper.City = "Vancouver";
            orderHelper.Country = "Canada";
            orderHelper.Currency = "CAD";
            orderHelper.IsTestMessage = false;
            orderHelper.OrderNumber = "100000999";
            orderHelper.State = "BC";
            orderHelper.Zip = "ABC123";

            orderHelper.Items.Add(new LineItem("Part1", 10));
            orderHelper.Items.Add(new LineItem("Part2", 20));
            orderHelper.Items.Add(new LineItem("Part3", 30));
            orderHelper.Items.Add(new LineItem("Part4", 40));
            orderHelper.Items.Add(new LineItem("Part5", 50));

            Assert.AreEqual(ExpectedOutput, orderHelper.GetFormattedOrder());
        }

        public string ExpectedOutput = @"SA1|BOO100000999|BOO007324|100001803|ORD001|BEMIS|ORD001||20140122|||test message|SA1_END
SA2|BOO100000999|BOO007324|100001803|20140122|20140122|20140122|100001803|||||904|||||||||CAD||||||BOO007324|||WRV5|||||||||BOO007324|||||||||||SA2_END
SA3|BOO100000999|BOO007324|100001803|STBP|Canada|Address Line 1|Address Line 2|Address Line 3|Address Line 4||||ABC123|BC||||||||Vancouver|Vancouver||SA3_END
SA5|BOO100000999|BOO007324|100001803|1|1|||Part1|||10|20140122|20140122|20140122|EA||EA||||||||||||||||||||||||||||||||||WRV5||||||||||||||||||||||||||||Canada||Canada|SA5_END
SA5|BOO100000999|BOO007324|100001803|2|2|||Part2|||20|20140122|20140122|20140122|EA||EA||||||||||||||||||||||||||||||||||WRV5||||||||||||||||||||||||||||Canada||Canada|SA5_END
SA5|BOO100000999|BOO007324|100001803|3|3|||Part3|||30|20140122|20140122|20140122|EA||EA||||||||||||||||||||||||||||||||||WRV5||||||||||||||||||||||||||||Canada||Canada|SA5_END
SA5|BOO100000999|BOO007324|100001803|4|4|||Part4|||40|20140122|20140122|20140122|EA||EA||||||||||||||||||||||||||||||||||WRV5||||||||||||||||||||||||||||Canada||Canada|SA5_END
SA5|BOO100000999|BOO007324|100001803|5|5|||Part5|||50|20140122|20140122|20140122|EA||EA||||||||||||||||||||||||||||||||||WRV5||||||||||||||||||||||||||||Canada||Canada|SA5_END";
    }
}
