using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.IO;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;


namespace Nop.Web.Models.Custom
{
    /*
     * This class provides a method to upload CYO order files to PRIDE
     * via SFTP. This is a scheduled task.
     * 
     * To run this every hour:
     * 
     * insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
     * values ('Send CYO orders to PRIDE', 3600, 'Nop.Web.Models.Custom.CYOFileTransferTask, Nop.Web', 1, 0, null, null, null)
     * 
     */

    public class CYOFileTransferTask : ITask
    {

        private ILogger _logger = null;
        private string _pathToAppData = null;
        private string _pathToWebConfig = null;
        private string _unsentOrdersDir = null;
        private string _sentOrdersDir = null;
        private string _sftpLogin = null;
        private string _sftpPassword = null;
        private string _sftpHost = null;
        private int _sftpPort = 0;
        private string _remoteUploadDir = null;

        public CYOFileTransferTask(IWebHelper webHelper)
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._pathToAppData = webHelper.MapPath("~/App_Data/cyo");
            
            this._pathToWebConfig = Path.Combine(Path.GetDirectoryName(webHelper.MapPath("~/Web.config")), "Web.config");  
        }

        void ITask.Execute()
        {
            if (EnsurePreConditions())
            {
                IEnumerable<string> unsentFiles = Directory.EnumerateFiles(_unsentOrdersDir);
                CYOSFTPClient sftp = new CYOSFTPClient(this._sftpHost, this._sftpPort, this._sftpLogin, this._sftpPassword);
            }
            else
            {
                throw new ApplicationException("CYO file transfer failed. See prior warnings in log.");
            }
        }

        private bool EnsurePreConditions()
        {
            bool ok = true;
            if (string.IsNullOrEmpty(_pathToAppData))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run", "The path the App_Data directory was not specified.", null);
                ok = false;
            }
            if (!Directory.Exists(_pathToAppData))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                    string.Format("The path the App_Data directory is incorrect: {0} does not exist.", _pathToAppData), null);
                ok = false;
            }
            this._unsentOrdersDir = Path.Combine(_pathToAppData, "orders_unsent");
            this._sentOrdersDir = Path.Combine(_pathToAppData, "orders_sent");
            if (!Directory.Exists(_unsentOrdersDir))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    string.Format("The unsent orders directory at {0} does not exist.", _unsentOrdersDir), null);
                ok = false;
            }
            if (!Directory.Exists(_sentOrdersDir))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    string.Format("The sent orders directory at {0} does not exist.", _sentOrdersDir), null);
                ok = false;
            }
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration(this._pathToWebConfig);
                this._sftpHost = (string)config.AppSettings.Settings["PRIDE_SFTP_HOST"].Value;
                this._sftpLogin = (string)config.AppSettings.Settings["PRIDE_SFTP_LOGIN"].Value;
                this._sftpPassword = (string)config.AppSettings.Settings["PRIDE_SFTP_PASSWORD"].Value;
                this._sftpPort = Int32.Parse((string)config.AppSettings.Settings["PRIDE_SFTP_PORT"].Value);
                this._remoteUploadDir = (string)config.AppSettings.Settings["PRIDE_SFTP_UPLOAD_DIR"].Value;
            }
            catch (Exception ex)
            {
                _logger.InsertLog(LogLevel.Error, "CYO file transfer did not run",
                                    string.Format("Got the following error while trying to read PRIDE_SFTP values from Web.config: {0}", ex.Message), null);
                ok = false;
            }
            if (string.IsNullOrEmpty(this._sftpHost))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_HOST from Web.config", null);
                ok = false;
            }
            if (string.IsNullOrEmpty(this._sftpLogin))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_LOGIN from Web.config", null);
                ok = false;
            }
            if (string.IsNullOrEmpty(this._sftpLogin))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_LOGIN from Web.config", null);
                ok = false;
            }
            if (string.IsNullOrEmpty(this._sftpPassword))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_PASSWORD from Web.config", null);
                ok = false;
            }
            if (this._sftpPort == 0)
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_PORT from Web.config, or number was invalid. (Hint: try 2021)", null);
                ok = false;
            }

            return ok;
        }
    }
}