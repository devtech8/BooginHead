using System;
using System.Collections.Generic;
using System.IO;
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
        /// Uploads the file at localFilePath to the SFTP server, writing
        /// it into remoteDirectory.
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="remoteDirectory"></param>
        public void Upload(string localFilePath, string remoteDirectory)
        {
            string fileBaseName = Path.GetFileName(localFilePath);
            string remoteFilePath = string.Format("{0}/{1}", remoteDirectory, fileBaseName);
            using (FileStream fileStream = File.Open(localFilePath, FileMode.Open))
            {
                using (var client = new SftpClient(host, port, login, password))
                {
                    client.Connect();
                    client.UploadFile(fileStream, remoteFilePath);
                }
            }
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