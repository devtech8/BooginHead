using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System.Configuration;
using System.Web.Configuration;

namespace Nop.Web.Models.Custom
{
    /*
     * This class provides a method to clean up old uploads and proofs
     * from the CYO tool.
     * 
     * To run this every hour:
     * 
     * insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
     * values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOImageCleanupTask, Nop.Web', 1, 0, null, null, null)
     * 
     * TODO: Don't delete proofs that are in a shopping cart!
     * 
     */
    public class CYOImageCleanupTask : ITask
    {
        private string _pathToAppData = null;
        private DateTime _tooOldForUploads = DateTime.MinValue;
        private DateTime _tooOldForProofs = DateTime.MinValue;
        private DateTime _tooOldForInCartImages = DateTime.MinValue;
        private DateTime _tooOldForSentOrderFiles = DateTime.MinValue;
        private ILogger _logger = null;
        private IWebHelper _webHelper = null;

        /// <summary>
        /// Scheduled task to clean up the uploads and proofs
        /// directories under App_Data. 
        /// NopCommerce seems to be able to figure out constructor
        /// params that implement an interface from Nop.Core (and
        /// maybe some other Nop namespaces?)
        /// </summary>
        /// <param name="webHelper"></param>
        public CYOImageCleanupTask(IWebHelper webHelper)
        {
            this._pathToAppData = webHelper.MapPath("~/App_Data/cyo");
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._webHelper = EngineContext.Current.Resolve<IWebHelper>();
        }

        void ITask.Execute()
        {
            bool error = false;
            if (string.IsNullOrEmpty(_pathToAppData))
            {
                _logger.InsertLog(LogLevel.Information, "CYO file cleanup did not run", "The path the App_Data directory was not specified.", null);
                error = true;
            }
            if (!Directory.Exists(_pathToAppData))
            {
                _logger.InsertLog(LogLevel.Information, "CYO file cleanup did not run",
                    string.Format("The path the App_Data directory is incorrect: {0} does not exist.", _pathToAppData), null);
                error = true;
            }
            error = !LoadFileDeletionSettings();
            if (error == true)
                return;
                
            DeleteOldFiles("uploads", this._tooOldForUploads);
            DeleteOldFiles("proofs", this._tooOldForProofs);
            DeleteOldFiles("in_cart", this._tooOldForInCartImages);
            DeleteOldFiles("orders_sent", this._tooOldForSentOrderFiles);
        }

        /// <summary>
        /// Load settings from Web.config describing when files in various
        /// directories should be cleaned out.
        /// </summary>
        /// <returns></returns>
        private bool LoadFileDeletionSettings()
        {
            bool success = true;
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
            try
            {
                int maxAgeForUploads = Int32.Parse((string)config.AppSettings.Settings["MaxAgeForUploads"].Value);
                int maxAgeForProofs = Int32.Parse((string)config.AppSettings.Settings["MaxAgeForProofs"].Value);
                int maxAgeForInCartImages = Int32.Parse((string)config.AppSettings.Settings["MaxAgeForInCartImages"].Value);
                int maxAgeForSentOrderFiles = Int32.Parse((string)config.AppSettings.Settings["MaxAgeForSentOrderFiles"].Value);

                this._tooOldForUploads = DateTime.Now.AddDays(-1 * maxAgeForUploads);
                this._tooOldForProofs = DateTime.Now.AddDays(-1 * maxAgeForProofs);
                this._tooOldForInCartImages = DateTime.Now.AddDays(-1 * maxAgeForInCartImages);
                this._tooOldForSentOrderFiles = DateTime.Now.AddDays(-1 * maxAgeForSentOrderFiles);
            }
            catch (Exception ex)
            {
                this._logger.Error("Error running scheduled image cleanup. The web.config file has missing or bad values for " + 
                    "one of these settings: MaxAgeForUploads, MaxAgeForProofs, MaxAgeForInCartImages, MaxAgeForSentOrderFiles. " +
                    "Each setting should be an integer describing the number of days after which images should be deleted from " +
                    "each of these folders.", ex);
                success = false;
            }
            return success;
        }

        private void DeleteOldFiles(string subdirectory, DateTime deleteFilesOlderThanThis)
        {
            int fileCount = 0;
            string directory = Path.Combine(_pathToAppData, subdirectory);
            if (!Directory.Exists(directory))
            {
                _logger.InsertLog(LogLevel.Error,
                    string.Format("CYO Scheduled Task could not delete from subdirectory {0}", subdirectory),
                    string.Format("Could not delete from directory {0} because it does not exist.", directory), null);
            }
            else
            {
                foreach (string fileName in Directory.EnumerateFiles(directory))
                {
                    if (File.GetLastWriteTime(fileName) < deleteFilesOlderThanThis)
                    {
                        File.Delete(fileName);
                        fileCount++;
                    }
                }
            }
            _logger.InsertLog(LogLevel.Information, "CYO file cleanup completed normally",
                string.Format("Deleted {0} files from directory {1}", fileCount, directory), null);
        }

    }
}