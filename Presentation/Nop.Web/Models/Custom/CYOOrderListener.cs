using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Web.Models.Custom
{
    public class CYOOrderListener : IConsumer<OrderPlacedEvent>
    {
        private ILogger _logger = null;
        private IWebHelper _webHelper = null;
        private string _singlePageTemplate = null;
        private string _multiPageTemplate = null;

        public CYOOrderListener()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
            this._singlePageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/pdf_templates"), "BH_Packing Slip_editable.pdf");
            this._multiPageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/pdf_templates"), "BH_MultiPGPackingSlip_editable.pdf");
        }

        void IConsumer<OrderPlacedEvent>.HandleEvent(OrderPlacedEvent eventMessage)
        {
            bool hasCYOItem = false;
            foreach (var item in eventMessage.Order.OrderItems)
            {
                if (item.Product.ProductTags.First(tag => tag.Name == "CYO") != null)
                {
                    // This is a very short string of XML. 
                    // Skipping document createion & will just extract the regex.                    
                    string imageGuid = CYOModel.ExtractGuid(item.AttributesXml);
                    MoveProofToOrdersFolder(imageGuid);
                    hasCYOItem = true;
                }
            }
            if (hasCYOItem)
            {
                CreateOrderFiles(eventMessage.Order);
            }
        }

        /// <summary>
        /// Moves the proof image into the orders folder, so the ImageCleanupTask won't delete it.
        /// </summary>
        /// <param name="imageGuid"></param>
        private void MoveProofToOrdersFolder(string imageGuid)
        {
            string sourcePath = this._webHelper.MapPath("~/App_Data/cyo/in_cart/");
            string destPath = this._webHelper.MapPath("~/App_Data/cyo/orders_unsent/");
            string sourceFile = Path.Combine(sourcePath, string.Format("{0}.png", imageGuid));
            string destFile = Path.Combine(destPath, string.Format("{0}.png", imageGuid));
            File.Move(sourceFile, destFile);
        }

        /// <summary>
        /// Creates the pipe-delimited order file and the PDF packing slip
        /// and puts these in the orders folder.
        /// </summary>
        /// <param name="order"></param>
        private void CreateOrderFiles(Nop.Core.Domain.Orders.Order order)
        {
            CreatePRIDEOrderFile(order);
            CreatePDFPackingSlip(order);
        }

        private void CreatePDFPackingSlip(Core.Domain.Orders.Order order)
        {
            CYOPDFHelper pdfHelper = new CYOPDFHelper(_singlePageTemplate, _multiPageTemplate);
            pdfHelper.OrderDate = DateTime.Now.ToString("MM/dd/yyyy");
            pdfHelper.OrderedBy = FormatAddress(order.Customer.BillingAddress);
            pdfHelper.OrderNumber = order.Id.ToString("D8");

            // Do we have a way of separating CYO shipping cost from other item shipping cost?
            pdfHelper.Shipping = Double.Parse(order.OrderShippingExclTax.ToString());

            // Need to convert shipping method from NOP to Enum.
            pdfHelper.ShippingMethod = ShippingMethod.USPS; // order.ShippingMethod;

            pdfHelper.ShipTo = FormatAddress(order.ShippingAddress);
            pdfHelper.SubTotal = Double.Parse(order.OrderSubtotalExclTax.ToString());
            pdfHelper.Tax = Double.Parse(order.OrderTax.ToString());
            pdfHelper.Total = Double.Parse(order.OrderTotal.ToString());

            AddLineItemsToPDF(pdfHelper, order);

            string outputFile = Path.Combine(_webHelper.MapPath("~/App_Data/orders_unsent/"), string.Format("BH_{0}", order.Id.ToString("D8")));
            pdfHelper.CreatePackingSlip(outputFile);
        }

        private void AddLineItemsToPDF(CYOPDFHelper pdfHelper, Core.Domain.Orders.Order order)
        {
            
        }

        private string FormatAddress(Address address)
        {
            StringBuilder sb = new StringBuilder(address.FirstName + " " + address.LastName + Environment.NewLine);
            if (!string.IsNullOrEmpty(address.Company))
                sb.Append(address.Company + Environment.NewLine);
            if (!string.IsNullOrEmpty(address.Address1))
                sb.Append(address.Address1 + Environment.NewLine);
            if (!string.IsNullOrEmpty(address.Address2))
                sb.Append(address.Address2 + Environment.NewLine);
            sb.Append(string.Format("{0}, {1} {2}", address.City, address.StateProvince.Abbreviation, address.ZipPostalCode));
            return sb.ToString();
        }

        private void CreatePRIDEOrderFile(Nop.Core.Domain.Orders.Order order)
        {
            CYOOrderHelper orderHelper = new CYOOrderHelper();
            orderHelper.RecipientNameLine1 = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;
            orderHelper.RecipientNameLine2 = order.ShippingAddress.Company;
            orderHelper.Address1 = order.ShippingAddress.Address1;
            orderHelper.Address2 = order.ShippingAddress.Address2;
            orderHelper.City = order.ShippingAddress.City;
            orderHelper.Country = order.ShippingAddress.Country.ThreeLetterIsoCode;
            orderHelper.Currency = "USD";
            orderHelper.IsTestMessage = false;
            orderHelper.OrderNumber = order.Id.ToString("D8");
            orderHelper.State = order.ShippingAddress.StateProvince.Abbreviation;
            orderHelper.Zip = order.ShippingAddress.ZipPostalCode;

            foreach (var item in order.OrderItems)
            {
                if (item.Product.ProductTags.First(tag => tag.Name == "CYO") != null)
                {
                    // TODO: Include color and size in the description.
                    string imageGuid = CYOModel.ExtractGuid(item.AttributesXml);
                    string color = "White";
                    string description = string.Format("Create Your Own Pacifier{0}Color={1}{0}Design={2}.png", Environment.NewLine, color, imageGuid);
                    orderHelper.Items.Add(new LineItem(description, item.Quantity));
                }
            }
            string filePath = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/orders_unsent"), string.Format("BH_{0}.txt", order.Id.ToString("D8")));
            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                byte[] orderData = System.Text.Encoding.UTF8.GetBytes(orderHelper.GetFormattedOrder());
                fs.Write(orderData, 0, orderData.Length);
            }
        }
    }
}