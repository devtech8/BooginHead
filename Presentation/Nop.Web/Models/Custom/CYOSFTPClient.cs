using System;
using System.Collections.Generic;
using System.IO;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Nop.Web.Models.Custom
{
    public class CYOSFTPClient
    {
        private string host = null;
        private int port = 22;
        private string login = null;
        private string password = null;
        private ILogger logger = null;


        /// <summary>
        /// SFTP Client for sending files to PRIDE. See the Web.config
        /// file for host, login, and other necessary params.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        public CYOSFTPClient(string host, int port, string login, string password)
        {
            this.host = host;
            this.port = port;
            this.login = login;
            this.password = password;
            try { logger = EngineContext.Current.Resolve<ILogger>(); }
            catch (Exception) { /* EngineContext not available when running NUnit tests */ }
        }


        /// <summary>
        /// Uploads all of the files in the localFiles list to the SFTP server, 
        /// writing them into remoteDirectory.
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="remoteDirectory"></param>
        /// <returns>The number of files successfully uploaded.</returns>
        public int Upload(List<string> localFiles, string remoteDirectory)
        {
            int filesUploaded = 0;
            using (var client = new SftpClient(host, port, login, password))
            {
                client.Connect();
                foreach (string localFilePath in localFiles)
                {
                    string fileBaseName = Path.GetFileName(localFilePath);
                    string remoteFilePath = string.Format("{0}/{1}", remoteDirectory, fileBaseName);
                    using (FileStream fileStream = File.Open(localFilePath, FileMode.Open))
                    {
                        try
                        {
                            client.UploadFile(fileStream, remoteFilePath);
                            string message = string.Format("Uploaded file {0}", localFilePath);
                            if (logger != null)
                                logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "SFTP Uploaded Succeeded", message, null);
                            filesUploaded++;
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("Failed file: {0}. Error: {1}", localFilePath, ex.Message);
                            if (logger != null)
                                logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "SFTP Upload Failed", message, null);
                        }
                    }
                }
                client.Disconnect();
            }
            return filesUploaded;
        }

        public IEnumerable<SftpFile> ListDirectory(string remotePath)
        {
            IEnumerable<SftpFile> files = null;
            using (var client = new SftpClient(host, port, login, password))
            {
                client.Connect();
                files = client.ListDirectory(remotePath);
                client.Disconnect();
            }
            return files;
        }
    }

}