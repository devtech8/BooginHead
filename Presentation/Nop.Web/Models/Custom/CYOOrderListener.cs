using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
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

namespace Nop.Web.Models.Custom
{
    public class CYOOrderListener : IConsumer<OrderPaidEvent>
    {
        private ILogger _logger = null;
        private IWebHelper _webHelper = null;
        private string _singlePageTemplate = null;
        private string _multiPageTemplate = null;

        public CYOOrderListener()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
            this._singlePageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/pdf_templates/"), "BH_Packing_Slip_editable.pdf");
            this._multiPageTemplate = Path.Combine(_webHelper.MapPath("~/App_Data/cyo/pdf_templates/"), "BH_MultiPGPackingSlip_editable.pdf");
        }

        /// <summary>
        /// When an order is PAID (not PLACED), create the CYO order files for PRIDE.
        /// This creates the PRIDE files only if 1) the order contains CYO items and 2)
        /// the order was not placed by a wholesale customer.
        /// 
        /// For wholesalers, we create the PRIDE files after the order has been reviewed 
        /// and (possibly) split into separate shipments.
        /// </summary>
        /// <param name="eventMessage"></param>
        void IConsumer<OrderPaidEvent>.HandleEvent(OrderPaidEvent eventMessage)
        {      
            bool customerIsWholesaler = eventMessage.Order.Customer.CustomerRoles
                .FirstOrDefault(cr => cr.Active && cr.SystemName.Equals("Wholesaler", StringComparison.InvariantCultureIgnoreCase)) != null;
            if (!customerIsWholesaler)
            {
                CYOPrideOrderCreator prideOrderCreator = new CYOPrideOrderCreator();
                prideOrderCreator.CreatePRIDEOrderFiles(eventMessage.Order);
            }
        }
    }
}