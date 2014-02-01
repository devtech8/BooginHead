using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Web.Models.Custom
{
    public class CYOCartItemEventListener : IConsumer<EntityUpdated<ShoppingCartItem>>, IConsumer<EntityInserted<ShoppingCartItem>>, IConsumer<EntityDeleted<ShoppingCartItem>>
    {
        public void HandleEvent(EntityUpdated<ShoppingCartItem> eventMessage)
        {
            // We don't care if item is updated
        }

        /// <summary>
        /// When user adds a custom pacifier to the cart, move the proof
        /// into the cart directory, so it doesn't get deleted.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void HandleEvent(EntityInserted<ShoppingCartItem> eventMessage)
        {
            string attributes = eventMessage.Entity.AttributesXml;          
        }

        public void HandleEvent(EntityDeleted<ShoppingCartItem> eventMessage)
        {
            // Should we move the proof out of the cart directory?
        }
    }
}