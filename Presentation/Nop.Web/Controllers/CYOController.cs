using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
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
            string proofUrl = null;
            string errorMessage = null;
            try
            {
                string imageFileName = imageHelper.CreateProof();
                proofUrl = Url.Action("ViewProof", new { fileName = imageFileName });
                AddProofToRecentDesigns(proofUrl);
                SaveProofData(cyoModel, imageFileName);
            }
            catch (Exception ex)
            {
                // TODO: Log error
                errorMessage = ex.Message;
            }
            return Json(new {proofUrl = proofUrl, errorMessage = errorMessage});
        }

        /// <summary>
        /// Save the data used to generate this proof.
        /// </summary>
        /// <param name="cyoModel"></param>
        /// <param name="imageFileName"></param>
        private void SaveProofData(CYOModel cyoModel, string imageFileName)
        {
            string path = Path.Combine(Server.MapPath("~/App_Data/cyo/proofs/"), imageFileName.Replace(".png", ".json"));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            byte[] proofData = System.Text.Encoding.UTF8.GetBytes(serializer.Serialize(cyoModel));
            using (FileStream fs = System.IO.File.Open(path, FileMode.Create))
            {
                fs.Write(proofData, 0, proofData.Length);
            }
        }

        [HttpGet]
        public ActionResult ViewProof(string fileName)
        {
            string filePath = Path.Combine(Server.MapPath("~/App_Data/cyo/proofs"), fileName);
            if (!System.IO.File.Exists(filePath))
            {
                filePath = Path.Combine(Server.MapPath("~/App_Data/cyo/in_cart"), fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(Server.MapPath("~/App_Data/cyo/orders_unsent"), fileName);
                    if (!System.IO.File.Exists(filePath))
                    {
                        filePath = Path.Combine(Server.MapPath("~/App_Data/cyo/orders_sent"), fileName);
                        return Content(string.Format("File '{0}' not found.", fileName));
                    }
                }
            }
            string contentType = "image/png";
            // Late addition: This endpoint serves the JSON data files for proofs as well as the proofs themselves.
            if (fileName.EndsWith("json"))
                contentType = "application/json";
            return new FileContentResult(System.IO.File.ReadAllBytes(filePath), contentType);
        }


        # region Recent Designs

        /// <summary>
        /// Adds the specified proof URL to the list of this user's 
        /// recent designs.
        /// </summary>
        /// <param name="proofUrl"></param>
        private void AddProofToRecentDesigns(string proofUrl)
        {
            List<string> recentDesigns = this.RecentDesigns;
            if (recentDesigns.Count() == 3)
                recentDesigns.RemoveAt(0);
            recentDesigns.Add(proofUrl);
            HttpCookie cookie = GetRecentDesignsCookie();
            cookie.Value = string.Join("|", recentDesigns);
            cookie.Expires = DateTime.Now.AddDays(5);
            Response.Cookies.Set(cookie);
        }


        /// <summary>
        /// Returns the URLs of the user's recent designs. These
        /// are URLs for the proofs the user has created recently.
        /// There will be a max of 3 recent URLs.
        /// </summary>
        private List<string> RecentDesigns
        {
            get
            {
                HttpCookie cookie = GetRecentDesignsCookie();
                if (!string.IsNullOrEmpty(cookie.Value))
                {
                    List<string> relativeUrls = cookie.Value.Split('|').ToList<string>();
                    // Remove URLs for items that have been deleted from the proofs folder.
                    // Items in that folder are deleted after a number of days.
                    for (int i = relativeUrls.Count - 1; i >= 0; i--)
                    {
                        string url = relativeUrls[i];
                        // Url looks like this:
                        // /Booginhead/CYO/ViewProof?fileName=ed557fe1-4fc6-46ba-95fa-a71f3fefa7e3.png
                        // The basename is at the end.
                        string fileBaseName = url.Split(new char[] { '=' }).Last(); 
                        string localPath = Path.Combine(Server.MapPath("~/App_Data/cyo/proofs/"), fileBaseName);
                        if (!System.IO.File.Exists(localPath))
                            relativeUrls.RemoveAt(i);
                    }
                    return relativeUrls;
                }
                else
                    return new List<string>();
            }
        }

        /// <summary>
        /// Returns the cookie containing the list of recent proof URLs.
        /// NopCommerce does not appear to use ASP.NET session by default,
        /// so we're storing this data in a cookie.
        /// </summary>
        /// <returns></returns>
        private HttpCookie GetRecentDesignsCookie()
        {
            if (Request.Cookies["CYORecentDesigns"] != null)
                return Request.Cookies["CYORecentDesigns"];
            HttpCookie cookie = new HttpCookie("CYORecentDesigns");
            cookie.Expires = DateTime.Now.AddDays(5);
            return cookie;
        }

        # endregion

    }
}
