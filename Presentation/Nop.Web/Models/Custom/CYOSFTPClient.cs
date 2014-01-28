using System;
using System.Collections.Generic;
using System.IO;
using Nop.Core.Domain.Logging;
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
        }


        /// <summary>
        /// Uploads all of the files in the localFiles list to the SFTP server, 
        /// writing them into remoteDirectory.
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="remoteDirectory"></param>
        /// <returns>An SftpResult object describing what what uploaded and what was not.</returns>
        public SftpResult Upload(IEnumerable<string> localFiles, string remoteDirectory)
        {
            SftpResult result = new SftpResult();
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
                            result.FilesSent.Add(localFilePath);
                        }
                        catch (Exception ex)
                        {
                            result.FilesNotSent[localFilePath] = ex.Message;
                        }
                    }
                }
                client.Disconnect();
            }
            return result;
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

    public class SftpResult
    {
        /// <summary>
        /// Contains a list of files that were successfully uploaded
        /// to the remote SFTP server.
        /// </summary>
        public List<string> FilesSent;

        /// <summary>
        /// Contains entries for files that were not sent to the remote
        /// SFTP server. The key is the filename, the value is the exception
        /// message describing why the file was not sent.
        /// </summary>
        public Dictionary<string, string> FilesNotSent;

        /// <summary>
        /// Contains information about the result of an SFTP
        /// upload operation.
        /// </summary>
        public SftpResult() 
        {
            this.FilesSent = new List<string>();
            this.FilesNotSent = new Dictionary<string, string>();
        }
    }
}