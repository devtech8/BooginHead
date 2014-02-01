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
    public class CYOCartItemEventListener : IConsumer<EntityUpdated<ShoppingCartItem>>, IConsumer<EntityInserted<ShoppingCartItem>>, IConsumer<EntityDeleted<ShoppingCartItem>>
    {
        private ILogger _logger = null;

        public CYOCartItemEventListener()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
        }

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
            string imageGuid = CYOModel.ExtractGuid(eventMessage.Entity.AttributesXml);
            MoveImageToCartFolder(imageGuid);
        }

        public void HandleEvent(EntityDeleted<ShoppingCartItem> eventMessage)
        {
            // Should we move the proof out of the cart directory?
        }

        private void MoveImageToCartFolder(string imageGuid)
        {

        }
    }
}