using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Web.Models.Custom
{
    public class CYOOrderListener : IConsumer<OrderPlacedEvent>
    {
        void IConsumer<OrderPlacedEvent>.HandleEvent(OrderPlacedEvent eventMessage)
        {
            foreach (var item in eventMessage.Order.OrderItems)
            {
                if (item.Product.ProductTags.First(tag => tag.Name == "CYO") != null)
                {
                    // This is a very short string of XML. 
                    // Skipping document createion & will just extract the regex.
                    string xml = item.AttributesXml;
                    //string imageGuid = ExtractGuid(item.AttributesXml);
                    //MoveProofToOrdersFolder(imageGuid);
                    //CreateOrderFiles(order);
                }
            }
        }
    }
}