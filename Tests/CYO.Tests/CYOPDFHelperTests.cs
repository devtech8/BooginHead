using System;
using System.Collections.Generic;
using System.IO;
using Nop.Web.Models.Custom;
using NUnit.Framework;

namespace CYO.Tests
{
    [TestFixture]
    class CYOPDFHelperTests
    {
        private string SinglePageTemplate = null;
        private string MultiPageTemplate = null;
        private string TestImage = null;

        public CYOPDFHelperTests()
        {
            // These two files are copied from the App_Data directory in a post-build event for CYO.Tests
            SinglePageTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BH_Packing Slip_editable.pdf");
            MultiPageTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BH_MultiPGPackingSlip_editable.pdf");
            // This one is copied from local dir to build output dir in post-build event.
            TestImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnitTest.png");
        }

        private List<CYOOrderItem> CreateItems(int howMany)
        {
            List<CYOOrderItem> items = new List<CYOOrderItem>(howMany);
            for (int i = 0; i < howMany; i++)
            {
                items.Add(new CYOOrderItem(
                    "Item " + i.ToString(),
                    TestImage,
                    "Description of item " + i.ToString(),
                    (double)(i + 0.99),
                    i + 1));
            }
            return items;
        }

        [Test]
        public void SinglePageTest()
        {
            CYOPDFHelper pdfHelper = new CYOPDFHelper(SinglePageTemplate, MultiPageTemplate);
            pdfHelper.OrderDate = DateTime.Now.ToString("MM/dd/yyyy");
            pdfHelper.OrderedBy = "Jimi Hendrix" + Environment.NewLine + "1001 East 5th St." + Environment.NewLine + "Seattle, WA 98101";
            pdfHelper.OrderNumber = "99000011";
            pdfHelper.Shipping = 4.75;
            pdfHelper.ShippingMethod = ShippingMethod.USPS.ToString();
            pdfHelper.ShipTo = "Bruce Lee" + Environment.NewLine + "8301 53rd Ave NW" + Environment.NewLine + "Seattle, WA 98177";
            pdfHelper.SubTotal = 31.78;
            pdfHelper.Tax = 2.38;
            pdfHelper.Total = 34.16;
            pdfHelper.Items = CreateItems(4);
            string outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SinglePage.pdf");
            pdfHelper.CreatePackingSlip(outputFile);
            Console.WriteLine("Single page PDF written to " + outputFile);
        }

        [Test]
        public void MultiPageTest()
        {
            CYOPDFHelper pdfHelper = new CYOPDFHelper(SinglePageTemplate, MultiPageTemplate);
            pdfHelper.OrderDate = DateTime.Now.ToString("MM/dd/yyyy");
            pdfHelper.OrderedBy = "Edna Krabapple" + Environment.NewLine + "1001 East 5th St." + Environment.NewLine + "Seattle, WA 98101";
            pdfHelper.OrderNumber = "99000033";
            pdfHelper.Shipping = 4.75;
            pdfHelper.ShippingMethod = ShippingMethod.FEDEX_2DAY.ToString();
            pdfHelper.ShipTo = "Seymour Skinner" + Environment.NewLine + "8301 53rd Ave NW" + Environment.NewLine + "Seattle, WA 98177";
            pdfHelper.SubTotal = 31.78;
            pdfHelper.Tax = 2.38;
            pdfHelper.Total = 34.16;
            pdfHelper.Items = CreateItems(40);
            string outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MultiPage.pdf");
            pdfHelper.CreatePackingSlip(outputFile);
            Console.WriteLine("Multi page PDF written to " + outputFile);
        }
    }
}
