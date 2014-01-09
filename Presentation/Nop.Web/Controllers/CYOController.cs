using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Models.Custom;

namespace Nop.Web.Controllers
{
    public class CYOController : Controller
    {
        ILocalizationService _localizationService = null;
        Regex validFileName = new Regex(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}\.[a-z]{3,4}$");

        public CYOController()
        {
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
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

            string uniqueFileName = string.Format("{0}{1}", Guid.NewGuid(), fileExtension);
            string path = Path.Combine(Server.MapPath("~/App_Data/cyo/uploads"), uniqueFileName);
            FileStream imageFile = null;
            try
            {
                imageFile = System.IO.File.OpenWrite(path);
                imageFile.Write(fileBinary, 0, fileBinary.Length);
            }
            catch (Exception ex)
            {
                // TODO: Log this exception
                
                return Json(new
                {
                    success = false,
                    message = string.Format("Error saving file: " + ex.Message),
                    fileName = Guid.Empty,
                }, "text/plain");
            }
            finally
            {
                if (imageFile != null)
                    imageFile.Close();
            }

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                fileName = uniqueFileName,
            }, "text/plain");
        }

        [HttpGet]
        public ActionResult GetFileUpload(string fileName)
        {
            if (!validFileName.IsMatch(fileName))
                return Content(string.Format("Filename '{0}' is not valid.", fileName));

            string filePath = Path.Combine(Server.MapPath("~/App_Data/cyo/uploads"), fileName);

            if (!System.IO.File.Exists(filePath))
                return Content(string.Format("File '{0}' not found.", fileName));

            string contentType = "image/jpeg";
            if (fileName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
                contentType = "image/gif";
            else if (fileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                contentType = "image/png";
            return new FileContentResult(System.IO.File.ReadAllBytes(filePath), contentType);
        }

        /// <summary>
        /// This will return the proof image. It's currently just returning
        /// the data that the user submitted.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateProof()
        {
            CYOModel cyoModel = new CYOModel(Request.Params);
            CYOImageHelper imageHelper = new CYOImageHelper(cyoModel, Server);
            //string command = imageHelper.GetProofCommand();
            string outputFile = imageHelper.CreateProof();
            return Json(new {outputFile = outputFile});
        }

    }
}
