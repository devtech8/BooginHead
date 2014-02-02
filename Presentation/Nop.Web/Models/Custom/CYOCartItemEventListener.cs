using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Logging;
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
        private IWebHelper _webHelper = null;

        public CYOCartItemEventListener()
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
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
            string imageGuid = null;
            try
            {
                imageGuid = CYOModel.ExtractGuid(eventMessage.Entity.AttributesXml);
                CopyImageToCartFolder(imageGuid);
            }
            catch (Exception ex)
            {
                _logger.InsertLog(LogLevel.Error, 
                    "Could not copy CYO proof to in_cart folder.", 
                    string.Format("Customer Id: {0}, Image Guid: {1} , Error: {2}", eventMessage.Entity.Customer.Id, imageGuid, ex.Message), 
                    null);
            }
        }

        public void HandleEvent(EntityDeleted<ShoppingCartItem> eventMessage)
        {
            // Should we move the proof out of the cart directory?
        }

        private void CopyImageToCartFolder(string imageGuid)
        {
            string sourcePath = this._webHelper.MapPath("~/App_Data/cyo/proofs/");
            string destPath = this._webHelper.MapPath("~/App_Data/cyo/in_cart/");
            string sourceFile = Path.Combine(sourcePath, string.Format("{0}.png", imageGuid));
            string destFile = Path.Combine(destPath, string.Format("{0}.png", imageGuid));
            File.Copy(sourceFile, destFile);
        }
    }
}