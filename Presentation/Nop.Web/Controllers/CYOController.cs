using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Web.Controllers
{
    public class CYOController : Controller
    {
        ILocalizationService _localizationService = null;
        IDownloadService _downloadService = null;

        public CYOController()
        {
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._downloadService = EngineContext.Current.Resolve<IDownloadService>();
        }

        //
        // GET: /CYO/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile()
        {
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                    throw new ArgumentException("No file uploaded");
                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            // TODO: Move fileMaxSize into a config file.
            int fileMaxSize = 10240000;
            if (fileBinary.Length > fileMaxSize)
            {
                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                return Json(new
                {
                    success = false,
                    message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)),
                    downloadGuid = Guid.Empty,
                }, "text/plain");
            }

            var download = new Download()
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            // TODO: Where is the download saved? 
            // TODO: Delete file when session expires.

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadGuid = download.DownloadGuid,
            }, "text/plain");
        }

        [HttpGet]
        public ActionResult GetFileUpload(Guid downloadId)
        {
            var download = _downloadService.GetDownloadByGuid(downloadId);
            if (download == null)
                return Content("Download is not available anymore.");

            if (download.UseDownloadUrl)
            {
                //return result
                return new RedirectResult(download.DownloadUrl);
            }
            else
            {
                if (download.DownloadBinary == null)
                    return Content("Download data is not available anymore.");

                //return result
                string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
                string contentType = fileName.ToLower().EndsWith(".png") ? "image/png" : "image/jpeg";
                return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
            }
        }

    }
}
