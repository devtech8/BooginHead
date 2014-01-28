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
using System.Text;


namespace Nop.Web.Models.Custom
{
    /*
     * This class provides a method to upload CYO order files to PRIDE
     * via SFTP. This is a scheduled task.
     * 
     * To run this every hour:
     * 
     * insert into ScheduleTask (Name, Seconds, Type, Enabled, StopOnError, LastStartUTC, LastEndUTC, LastSuccessUTC)
     * values ('Send CYO orders to PRIDE', 1200, 'Nop.Web.Models.Custom.CYOFileTransferTask, Nop.Web', 1, 0, null, null, null)
     * 
     */

    public class CYOFileTransferTask : ITask
    {

        private ILogger _logger = null;
        private string _pathToAppData = null;
        private string _unsentOrdersDir = null;
        private string _sentOrdersDir = null;
        private string _sftpUser = null;
        private string _sftpPassword = null;
        private string _sftpHost = null;
        private int _sftpPort = 0;
        private string _remoteUploadDir = null;

        /// <summary>
        /// Scheduled task to send CYO order files to PRIDE.
        /// </summary>
        /// <param name="webHelper"></param>
        public CYOFileTransferTask(IWebHelper webHelper)
        {
            this._logger = EngineContext.Current.Resolve<ILogger>();
            this._pathToAppData = webHelper.MapPath("~/App_Data/cyo");            
        }

        /// <summary>
        /// Send all unsent orders to PRIDE's SFTP server, then move those files
        /// from App_Data/cyo/orders_unsent into App_Data/cyo/orders_sent.
        /// </summary>
        void ITask.Execute()
        {
            if (EnsurePreConditions())
            {
                List<string> filesToSend = new List<string>(Directory.EnumerateFiles(_unsentOrdersDir));
                if (filesToSend.Count() == 0)
                {
                    _logger.InsertLog(LogLevel.Information, "CYO file transfer completed OK - Sent 0 files", 
                        "The CYO file transfer task completed successfully, but there no files to send.", null);
                }
                else
                {
                    CYOSFTPClient sftp = new CYOSFTPClient(this._sftpHost, this._sftpPort, this._sftpUser, this._sftpPassword);
                    try 
                    { 
                        SftpResult sftpResult = sftp.Upload(filesToSend, this._remoteUploadDir);
                        MoveSentFiles(sftpResult.FilesSent);
                        if (sftpResult.FilesSent.Count == filesToSend.Count())
                        {
                            _logger.InsertLog(LogLevel.Information,
                                string.Format("CYO file transfer completed OK - Sent {0} files", sftpResult.FilesSent.Count),
                                "The CYO file transfer task completed successfully.", null);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder("The following files were not sent to the SFTP server. The scheduled taks will try again on the next run." + Environment.NewLine);
                            foreach (string filename in sftpResult.FilesNotSent.Keys)
                            {
                                sb.Append(string.Format("{0}: {1}{2}", filename, sftpResult.FilesNotSent[filename], Environment.NewLine));
                            }
                            _logger.InsertLog(LogLevel.Warning,
                                string.Format("CYO file transfer completed, but only {0} of {1} files were sent.", sftpResult.FilesSent.Count, filesToSend.Count()),
                                sb.ToString(), null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.InsertLog(LogLevel.Error, "CYO file transfer failed with error from sftp client", ex.Message, null);
                    }                    
                }
            }
            else
            {
                throw new ApplicationException("CYO file transfer failed. See prior warnings in log.");
            }
        }


        /// <summary>
        /// Move all of the files that were successfully uploaded from the
        /// orders_unsent directory into the orders_sent directory.
        /// </summary>
        /// <param name="sentFiles"></param>
        private void MoveSentFiles(IEnumerable<string> sentFiles)
        {
            foreach (string filename in sentFiles)
            {
                string newname = filename.Replace("orders_unsent", "orders_sent");
                File.Move(filename, newname);
            }
        }

        /// <summary>
        /// Make sure all pre-conditions are met. If any pre-conditions are not met,
        /// log the problem, return false, and stop the job.
        /// </summary>
        /// <returns></returns>
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
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
                this._sftpHost = (string)config.AppSettings.Settings["PRIDE_SFTP_HOST"].Value;
                this._sftpUser = (string)config.AppSettings.Settings["PRIDE_SFTP_USER"].Value;
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
            if (string.IsNullOrEmpty(this._sftpUser))
            {
                _logger.InsertLog(LogLevel.Warning, "CYO file transfer did not run",
                                    "Could not read PRIDE_SFTP_LOGIN from Web.config", null);
                ok = false;
            }
            if (string.IsNullOrEmpty(this._sftpUser))
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