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

namespace Nop.Web.Models.Custom
{
    /*
     * To run this every hour:
     * 
     * insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
     * values ('Clean up CYO files', 3600, 'Nop.Web.Models.Custom.CYOScheduledTask, Nop.Web', 1, 0, null, null, null)
     * 
     */
    public class CYOScheduledTask : ITask
    {
        private string _pathToAppData = null;
        private DateTime _tooOld = DateTime.MinValue;
        private ILogger _logger = null;
        private int _maxAgeInHours = 24;

        /// <summary>
        /// Scheduled task to clean up the uploads and proofs
        /// directories under App_Data every 24 hours. 
        /// NopCommerce seems to be able to figure out constructor
        /// params that implement an interface from Nop.Core (and
        /// maybe some other Nop namespaces?)
        /// </summary>
        /// <param name="webHelper"></param>
        public CYOScheduledTask(IWebHelper webHelper)
        {
            this._pathToAppData = webHelper.MapPath("~/App_Data/cyo");
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._tooOld = DateTime.Now.AddHours(-1 * _maxAgeInHours);
        }

        void ITask.Execute()
        {
            if(string.IsNullOrEmpty(_pathToAppData))
                _logger.InsertLog(LogLevel.Information, "CYO file cleanup did not run", "The path the App_Data directory was not specified.", null);
            if (!Directory.Exists(_pathToAppData))
                _logger.InsertLog(LogLevel.Information, "CYO file cleanup did not run", 
                    string.Format("The path the App_Data directory is incorrect: {0} does not exist.", _pathToAppData), null);
            DeleteOldFiles("uploads");
            DeleteOldFiles("proofs");
        }


        private void DeleteOldFiles(string subdirectory)
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
                    if (File.GetLastWriteTime(fileName) < _tooOld)
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