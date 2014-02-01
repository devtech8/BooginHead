using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public CYOOrderListener()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
        }

        void IConsumer<OrderPlacedEvent>.HandleEvent(OrderPlacedEvent eventMessage)
        {
            foreach (var item in eventMessage.Order.OrderItems)
            {
                if (item.Product.ProductTags.First(tag => tag.Name == "CYO") != null)
                {
                    // This is a very short string of XML. 
                    // Skipping document createion & will just extract the regex.                    
                    string imageGuid = CYOModel.ExtractGuid(item.AttributesXml);
                    MoveProofToOrdersFolder(imageGuid);
                    CreateOrderFiles(eventMessage.Order);
                }
            }
        }

        /// <summary>
        /// Moves the proof image into the orders folder, so the ImageCleanupTask won't delete it.
        /// </summary>
        /// <param name="imageGuid"></param>
        private void MoveProofToOrdersFolder(string imageGuid)
        {
            
        }

        /// <summary>
        /// Creates the pipe-delimited order file and the PDF packing slip
        /// and puts these in the orders folder.
        /// </summary>
        /// <param name="order"></param>
        private void CreateOrderFiles(Nop.Core.Domain.Orders.Order order)
        {

        }
    }
}