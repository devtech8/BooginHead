using System;
using System.IO;
using System.Collections.Generic;
using Renci.SshNet.Sftp;
using Nop.Web.Models.Custom;
using NUnit.Framework;

namespace CYO.Tests
{
    [TestFixture]
    class CYOSFTPClientTests
    {

        // These values are in the Web.config file of the Nop.Web project.
        // Ideally, we should read them from there. But to speed up dev/testing,
        // I'm putting them here for now.
        private string host = "fx.prideindustries.com";
        private int port = 2021;
        private string login = "fxbooginhead";
        private string password = "yulmej?Coss7";
        private string remoteUploadDir = "from-booginhead";

        [Test]
        public void UploadTest()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "SFTP_Test_File.txt");
            CYOSFTPClient client = new CYOSFTPClient(host, port, login, password);
            List<string> localFiles = new List<string>() { filePath, filePath };
            SftpResult sftpResult = client.Upload(localFiles, remoteUploadDir);
            Assert.AreEqual(localFiles.Count, sftpResult.FilesSent.Count);
        }

        [Test]
        public void ListDirectoryTest()
        {
            CYOSFTPClient client = new CYOSFTPClient(host, port, login, password);
            IEnumerable<SftpFile> files = client.ListDirectory(remoteUploadDir);
            Assert.NotNull(files);
            foreach (SftpFile file in files)
                Console.WriteLine(file.FullName);
        }

    }
}
