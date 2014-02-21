using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Logging;
using System.IO;
using System.Text;
using Nop.Core.Domain.Shipping;

namespace Nop.Web.Models.Custom
{
    /// <summary>
    /// This class creates the files necessary for orders fulfilled by PRIDE.
    /// </summary>
    public class CYOPrideOrderCreator
    {
        private ILogger _logger = null;
        private IWebHelper _webHelper = null;
        private string _singlePageTemplate = null;
        private string _multiPageTemplate = null;
        private List<string> _proofsToCleanUp = null;

        public CYOPrideOrderCreator()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
            this._singlePageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/pdf_templates/"), "BH_Packing_Slip_editable.pdf");
            this._multiPageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/pdf_templates/"), "BH_MultiPGPackingSlip_editable.pdf");
            this._proofsToCleanUp = new List<string>();
        }

        /// <summary>
        /// Creates the required PRIDE order files and moves them
        /// into the diretory ~/App_Data/cyo/orders_unsent.
        /// These files will go into the folder App_Data/cyo/orders_unsent. They'll 
        /// be picked up by the scheduled job CYOFileTransferTask, and sent by SFTP
        /// to PRIDE for fulfillment.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void CreatePRIDEOrderFiles(Nop.Core.Domain.Orders.Order order)
        {
            // This is f***ed-up. If an order is sent all in one shipment,
            // it has zero shipments!
            if (order.Shipments.Count == 0)
            {
                Core.Domain.Shipping.Shipment shipment = new Core.Domain.Shipping.Shipment();
                shipment.Order = order;
                shipment.OrderId = order.Id;
                foreach (var item in order.OrderItems)
                {
                    ShipmentItem shipmentItem = new ShipmentItem();
                    shipmentItem.Id = item.Id;
                    shipmentItem.OrderItemId = item.Id;
                    shipmentItem.Quantity = item.Quantity;
                    shipmentItem.Shipment = shipment;
                    shipmentItem.ShipmentId = shipment.Id;
                    shipment.ShipmentItems.Add(shipmentItem);
                }
                order.Shipments.Add(shipment);
            }
            for(int i=0; i < order.Shipments.Count; i++)
            {
                int shipmentNumber = i + 1;
                Nop.Core.Domain.Shipping.Shipment shipment = order.Shipments.ElementAt(i);
                CreateFilesForShipment(shipment, shipmentNumber);
            }
            CleanUpProofs();
        }

        private void CreateFilesForShipment(Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber)
        {
            Nop.Core.Domain.Orders.Order order = shipment.Order;
            List<OrderItem> cyoItems = GetCyoItems(shipment);

            // If there are no CYO items in the order, do not create files
            // for PRIDE. Booginhead will fulfill the order. PRIDE fulfills
            // orders that contain CYO-only or a mix of CYO and non-CYO items.
            if (cyoItems.Count == 0)
                return;

            foreach (var item in cyoItems)
            {
                string imageGuid = CYOModel.ExtractGuid(item.AttributesXml);
                CopyProofToOrdersFolder(imageGuid);
            }
            CreateOrderFiles(shipment, shipmentNumber);
            RenameImageFiles(shipment, shipmentNumber, cyoItems);
        }

        /// <summary>
        /// Returns a list of CYO items in the shipment.
        /// </summary>
        /// <param name="shipment"></param>
        /// <returns></returns>
        private List<OrderItem> GetCyoItems(Nop.Core.Domain.Shipping.Shipment shipment)
        {
            List<OrderItem> cyoItems = new List<OrderItem>();
            foreach (var item in shipment.ShipmentItems)
            {
                OrderItem orderItem = shipment.Order.OrderItems.First(i => i.Id == item.OrderItemId);
                if (orderItem.Product.ProductTags.FirstOrDefault(tag => tag.Name == "CYO") != null)
                {
                    cyoItems.Add(orderItem);
                }
            }
            return cyoItems;
        }


        /// <summary>
        /// Delete proof images that were moved into the orders folder.
        /// Don't do this until the end, because multiple shipments in one
        /// order may refer to the same proof image.
        /// </summary>
        private void CleanUpProofs()
        {
            foreach (string imageFile in this._proofsToCleanUp)
            {
                if (File.Exists(imageFile))
                {
                    File.Delete(imageFile);
                }
            }
        }

        /// <summary>
        /// While constructing the PDF order file, the proof images need to keep
        /// their Guids. After the PDF order file is generated, the name of each
        /// proof image should match the name of the other files, so PRIDE can 
        /// keep them together. PRIDE requested this naming scheme. 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="cyoItems"></param>
        private void RenameImageFiles(Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber, List<OrderItem> cyoItems)
        {
            string directory = this._webHelper.MapPath("~/App_Data/cyo/orders_unsent/");
            int itemNumber = 1;
            foreach (var item in cyoItems)
            {
                string imageName = string.Format("BH_{0}_{1}_{2}", shipment.Order.Id.ToString("D8"), shipmentNumber, itemNumber.ToString("D3"));
                string imageGuid = CYOModel.ExtractGuid(item.AttributesXml);
                string sourceFile = Path.Combine(directory, string.Format("{0}.png", imageGuid));
                string destFile = Path.Combine(directory, string.Format("{0}.png", imageName));
                if (!File.Exists(destFile))
                {
                    File.Move(sourceFile, destFile);
                }
                itemNumber++;
            }

        }

        /// <summary>
        /// Copies the proof image into the orders folder, so the ImageCleanupTask won't delete it.
        /// We don't want to remove it from the proofs folder until we're all done creating the
        /// PRIDE files, because it's possible for two shipments to refer to the same proof image.
        /// </summary>
        /// <param name="imageGuid"></param>
        private void CopyProofToOrdersFolder(string imageGuid)
        {
            string sourcePath = this._webHelper.MapPath("~/App_Data/cyo/in_cart/");
            string destPath = this._webHelper.MapPath("~/App_Data/cyo/orders_unsent/");
            string sourceFile = Path.Combine(sourcePath, string.Format("{0}.png", imageGuid));
            string destFile = Path.Combine(destPath, string.Format("{0}.png", imageGuid));
            if (!File.Exists(destFile))
            {
                File.Copy(sourceFile, destFile);
                this._proofsToCleanUp.Add(sourceFile);
            }
        }

        /// <summary>
        /// Creates the pipe-delimited order file and the PDF packing slip
        /// and puts these in the orders folder.
        /// </summary>
        /// <param name="order"></param>
        private void CreateOrderFiles(Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber)
        {
            CreatePRIDEOrderFile(shipment, shipmentNumber);
            CreatePDFPackingSlip(shipment, shipmentNumber);
        }

        private void CreatePDFPackingSlip(Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber)
        {
            Nop.Core.Domain.Orders.Order order = shipment.Order;
            CYOPDFHelper pdfHelper = new CYOPDFHelper(_singlePageTemplate, _multiPageTemplate);
            pdfHelper.OrderDate = DateTime.Now.ToString("MM/dd/yyyy");
            pdfHelper.OrderedBy = FormatAddress(order.Customer.BillingAddress);
            pdfHelper.OrderNumber = order.Id.ToString("D8");
            pdfHelper.Shipping = Double.Parse(order.OrderShippingExclTax.ToString());
            pdfHelper.ShippingMethod = order.ShippingMethod;
            pdfHelper.ShipTo = FormatAddress(order.ShippingAddress);
            pdfHelper.SubTotal = Double.Parse(order.OrderSubtotalExclTax.ToString());
            pdfHelper.Tax = Double.Parse(order.OrderTax.ToString());
            pdfHelper.Total = Double.Parse(order.OrderTotal.ToString());

            AddLineItemsToPDF(pdfHelper, shipment, shipmentNumber);

            if (shipment.Order.Shipments.Count > 1)
            {
                string message = string.Format("This order has {0} shipments. " + 
                        "Totals below are for the ENTIRE order, " + 
                        "but only items listed above are included in this shipment.", shipment.Order.Shipments.Count);
                CYOOrderItem cyoOrderItem = new CYOOrderItem("", "", message, 0, 0);
                pdfHelper.Items.Add(cyoOrderItem);
            }

            string outputFile = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/orders_unsent/"), string.Format("BH_{0}_{1}.pdf", order.Id.ToString("D8"), shipmentNumber));
            pdfHelper.CreatePackingSlip(outputFile);
        }

        private void AddLineItemsToPDF(CYOPDFHelper pdfHelper, Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber)
        {
            Nop.Core.Domain.Orders.Order order = shipment.Order;
            int itemNumber = 1;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                OrderItem item = order.OrderItems.First(i => i.Id == shipmentItem.OrderItemId);
                string description = string.Format("{0} ({1})", item.Product.Name, item.Product.ManufacturerPartNumber);
                string pathToImageFile = _webHelper.MapPath("~/Content/Images/Thumbs/default-image_80.gif");
                byte[] imageBinary = null;
                if (item.Product.ProductTags.FirstOrDefault(tag => tag.Name == "CYO") != null)
                {
                    Dictionary<string, string> attributes = ParseAttributes(item.AttributesXml);
                    string imageGuid = attributes["CYO Unique Id"];
                    string designName = string.Format("BH_{0}_{1}_{2}.png", order.Id.ToString("D8"), shipmentNumber, itemNumber.ToString("D3"));
                    string color = attributes["CYO Color"];
                    string size = attributes["CYO Size"];
                    string brand = attributes["CYO Brand"];
                    description = string.Format("CYO Pacifier Color={0}{4}Size={1},Brand={2}{4}Design={3}", color, size, brand, designName, Environment.NewLine);
                    pathToImageFile = Path.Combine(this._webHelper.MapPath("~/App_Data/cyo/orders_unsent/"), string.Format("{0}.png", imageGuid));
                }
                else
                {
                    // Not a CYO item
                    ProductPicture productPicture = item.Product.ProductPictures.FirstOrDefault();
                    if (productPicture != null)
                    {
                        imageBinary = productPicture.Picture.PictureBinary;
                    }
                }
                double priceAsDouble = Double.Parse(item.UnitPriceExclTax.ToString());


                CYOOrderItem cyoOrderItem = null;
                if (imageBinary != null)
                    cyoOrderItem = new CYOOrderItem(item.Product.Name, imageBinary, description, priceAsDouble, shipmentItem.Quantity);
                else
                    cyoOrderItem = new CYOOrderItem("CYO Pacifier", pathToImageFile, description, priceAsDouble, shipmentItem.Quantity);

                pdfHelper.Items.Add(cyoOrderItem);
                itemNumber++;
            }
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

        private void CreatePRIDEOrderFile(Nop.Core.Domain.Shipping.Shipment shipment, int shipmentNumber)
        {
            Nop.Core.Domain.Orders.Order order = shipment.Order;
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

            int cyoItemNumber = 1;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                OrderItem item = order.OrderItems.First(i => i.Id == shipmentItem.OrderItemId);
                string description = string.Format("{0} ({1})", item.Product.Name, item.Product.ManufacturerPartNumber);
                if (item.Product.ProductTags.FirstOrDefault(tag => tag.Name == "CYO") != null)
                {
                    description = string.Format("CYO Pacifier {0}_{1}", shipmentNumber, cyoItemNumber.ToString("D3"));
                    cyoItemNumber++;
                }
                orderHelper.Items.Add(new LineItem(description, shipmentItem.Quantity));
            }
            string filePath = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/orders_unsent"), string.Format("BH_{0}_{1}.txt", order.Id.ToString("D8"), shipmentNumber));
            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                byte[] orderData = System.Text.Encoding.UTF8.GetBytes(orderHelper.GetFormattedOrder());
                fs.Write(orderData, 0, orderData.Length);
            }
        }


        /// <summary>
        /// Returns a dictionary of attrbutes, including size, color, brand and unique id.
        /// All of these attributes should be marked as "Required" in the Admin control
        /// panel, so we know they will be here.
        /// </summary>
        /// <param name="attributesXml"></param>
        /// <returns></returns>
        private Dictionary<string, string> ParseAttributes(string attributesXml)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            IProductAttributeParser p = EngineContext.Current.Resolve<IProductAttributeParser>();
            IList<ProductVariantAttribute> attrs = p.ParseProductVariantAttributes(attributesXml);
            foreach (var attr in attrs)
            {
                string name = attr.ProductAttribute.Name;
                string value = p.ParseValues(attributesXml, attr.Id).FirstOrDefault();
                attributes[name] = value;

            }

            return attributes;
        }
    }
}
