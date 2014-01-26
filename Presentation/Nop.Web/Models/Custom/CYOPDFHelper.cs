using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Nop.Web.Models.Custom
{
    public class CYOPDFHelper
    {
        public static readonly int SINGLE_ORDER_TEMPLATE_MAX_ITEMS = 14;
        public static readonly int MULTI_ORDER_TEMPLATE_MAX_ITEMS = 16;

        /// <summary>
        /// Full system path the PDF template for single-page orders.
        /// This template works for orders with 14 or fewer line items.
        /// </summary>
        public string SinglePageOrderTemplate { get; set; }

        /// <summary>
        /// Full system path the PDF template for multi-page orders.
        /// This template works for orders with more than 14 line items.
        /// </summary>
        public string MultiPageOrderTemplate { get; set; }

        /// <summary>
        /// A list of line items included in this order.
        /// This should include only the items being shipped by PRIDE.
        /// </summary>
        public List<CYOOrderItem> Items { get; set; }

        /// <summary>
        /// Name and formatted address of the customer who placed the order.
        /// </summary>
        public string OrderedBy { get; set; }

        /// <summary>
        /// Name and formatted address of recipient.
        /// </summary>
        public string ShipTo { get; set; }

        /// <summary>
        /// Order date in mm/dd/yyyy format.
        /// </summary>
        public string OrderDate { get; set; }

        /// <summary>
        /// The order number
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Shipping method
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Order sub-total
        /// </summary>
        public double SubTotal { get; set; }

        /// <summary>
        /// The shipping cost
        /// </summary>
        public double Shipping { get; set; }

        /// <summary>
        /// Tax
        /// </summary>
        public double Tax { get; set; }

        /// <summary>
        /// Order total cost
        /// </summary>
        public double Total { get; set; }

        
        private int lastItemPrinted = 0;

        public CYOPDFHelper()
        {
            this.Items = new List<CYOOrderItem>();
        }

        public CYOPDFHelper(string singlePageOrderTemplate, string multiPageOrderTemplate)
        {
            this.SinglePageOrderTemplate = singlePageOrderTemplate;
            this.MultiPageOrderTemplate = multiPageOrderTemplate;
            this.Items = new List<CYOOrderItem>();
        }


        public void CreatePackingSlip(string outputFilePath)
        {
            lastItemPrinted = 0;
            List<byte[]> pages = new List<byte[]>();
            while(lastItemPrinted < Items.Count)
                pages.Add(CreatePage(lastItemPrinted));
            byte[] pdfData = null;
            if (Items.Count <= SINGLE_ORDER_TEMPLATE_MAX_ITEMS)
                pdfData = pages[0];
            else
                pdfData = MergePages(pages);
            FileStream output = null;
            try 
            {
                output = File.Open(outputFilePath, FileMode.Create);
                output.Write(pdfData, 0, pdfData.Length);               
            }
            finally
            {
                if (output != null)
                    output.Close();
            }
        }

        private byte[] MergePages(List<byte[]> pages)
        {
            byte[] pdfData = null;
            PdfReader pdfReader = new PdfReader(pages[0]);
            Document newDocument = new Document(pdfReader.GetPageSizeWithRotation(1));
            using (var memStream = new MemoryStream())
            {
                PdfCopy copier = new PdfCopy(newDocument, memStream);
                newDocument.Open();
                for (int i = 0; i < pages.Count; i++)
                {
                    PdfReader tempReader = new PdfReader(pages[i]);
                    copier.AddPage(copier.GetImportedPage(tempReader, 1));
                    tempReader.Close();
                }
                newDocument.Close();
                pdfData = memStream.ToArray();
            }
            pdfReader.Close();
            return pdfData;
        }

        private byte[] CreatePage(int itemIndex)
        {
            MemoryStream memStream = new MemoryStream();
            string template = SinglePageOrderTemplate;
            int maxItemsPerPage = SINGLE_ORDER_TEMPLATE_MAX_ITEMS;            
            if ((Items.Count - itemIndex) > SINGLE_ORDER_TEMPLATE_MAX_ITEMS)
            {
                template = MultiPageOrderTemplate;
                maxItemsPerPage = MULTI_ORDER_TEMPLATE_MAX_ITEMS;
            }
            lastItemPrinted = Math.Min(itemIndex + maxItemsPerPage, Items.Count);
            bool thisIsTheLastPage = (lastItemPrinted == Items.Count);

            using (PdfStamper pdfStamper = new PdfStamper(new PdfReader(template), memStream))
            {
                CreateOrderHeader(pdfStamper);
                for (int i = itemIndex; i < lastItemPrinted; i++)
                    CreateLineItem(pdfStamper, i);
                if (thisIsTheLastPage)
                    CreateOrderTotals(pdfStamper);
                pdfStamper.FormFlattening = true;
            }

            byte[] pdfData = memStream.ToArray();
            memStream.Close();
            return pdfData;
        }

        private void CreateOrderHeader(PdfStamper pdfStamper)
        {
            pdfStamper.AcroFields.SetField("Ordered By", this.OrderedBy);
            pdfStamper.AcroFields.SetField("Ship To", this.ShipTo);
            pdfStamper.AcroFields.SetField("Order Date", this.OrderDate);
            pdfStamper.AcroFields.SetField("Order Number", this.OrderNumber);
            pdfStamper.AcroFields.SetField("number of items", this.Items.Count.ToString());
        }

        private void CreateLineItem(PdfStamper pdfStamper, int itemIndex)
        {
            int lineNumber = GetLineNumber(itemIndex);
            CYOOrderItem item = Items[itemIndex];
            pdfStamper.AcroFields.SetField(string.Format("ITEMRow{0}", lineNumber), item.ItemName);

            if (item.Image != null)
            {
                //pdfStamper.AcroFields.SetField(string.Format("ImageRow{0}", lineNumber), "");
                string fieldName = string.Format("ImageRow{0}", lineNumber);
                AcroFields.FieldPosition fieldPosition = pdfStamper.AcroFields.GetFieldPositions(fieldName)[0];

                PushbuttonField imageField = new PushbuttonField(pdfStamper.Writer, fieldPosition.position, fieldName);
                imageField.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
                imageField.Image = item.Image;
                imageField.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
                imageField.ProportionalIcon = false;
                imageField.Options = BaseField.READ_ONLY;
                
                pdfStamper.AcroFields.RemoveField(fieldName);
                pdfStamper.AddAnnotation(imageField.Field, fieldPosition.page);
            }

            pdfStamper.AcroFields.SetField(string.Format("DESCRIPTIONRow{0}", lineNumber), item.Description);
            pdfStamper.AcroFields.SetField(string.Format("UNIT PRICERow{0}", lineNumber), item.UnitPrice.ToString("C2"));
            pdfStamper.AcroFields.SetField(string.Format("QTYRow{0}", lineNumber), item.Quantity.ToString());
            pdfStamper.AcroFields.SetField(string.Format("TOTALRow{0}", lineNumber), item.Total.ToString("C2"));
        }


        private int GetLineNumber(int itemIndex)
        {
            int lineNumber = itemIndex + 1;
            if (itemIndex >= MULTI_ORDER_TEMPLATE_MAX_ITEMS)
                lineNumber = (itemIndex % MULTI_ORDER_TEMPLATE_MAX_ITEMS) + 1;
            return lineNumber;
        }

        private void CreateOrderTotals(PdfStamper pdfStamper)
        {
            pdfStamper.AcroFields.SetField("TOTALSubTotal", this.SubTotal.ToString("C2"));
            pdfStamper.AcroFields.SetField("TOTALShipping", this.Shipping.ToString("C2"));
            pdfStamper.AcroFields.SetField("TOTALTax", this.Tax.ToString("C2"));
            pdfStamper.AcroFields.SetField("TOTALTotal", this.Total.ToString("C2"));
        }

    }

    public class CYOOrderItem
    {
        public string ItemName { get; set; }
        public Image Image { get; set; }
        public string Description { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double Total { get { return UnitPrice * Quantity; } }

        public CYOOrderItem(string itemName, string pathToImageFile, string description, double unitPrice, int quantity)
        {
            this.ItemName = itemName;
            if(!string.IsNullOrEmpty(pathToImageFile) && File.Exists(pathToImageFile))
                this.Image = iTextSharp.text.Image.GetInstance(pathToImageFile);
            this.Description = description;
            this.UnitPrice = unitPrice;
            this.Quantity = quantity;
        }

        public void SetImageFromFile(string pathToImageFile)
        {
            this.Image = iTextSharp.text.Image.GetInstance(pathToImageFile);
        }

    }
}