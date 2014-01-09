using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Nop.Web.Models.Custom
{

    public class CYOImageHelper
    {
        private CYOModel cyoModel = null;
        private HttpServerUtilityBase server = null;
        private Guid guid = Guid.Empty;

        Regex cyoUploadedFileName = new Regex(@"([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}\.[a-z]{3,4})");
        Regex legalFileName = new Regex(@"^[\w\-. ]+$");
        Regex legalHexColor = new Regex(@"^#[0-9a-f]{6}$", RegexOptions.IgnoreCase);

        private static PrivateFontCollection customFonts = new PrivateFontCollection();

        public CYOImageHelper(CYOModel cyoModel, HttpServerUtilityBase server)
        {
            this.cyoModel = cyoModel;
            this.server = server;
            this.guid = Guid.NewGuid();
        }

        public string OutputFileName
        {
            get
            {
                return Path.Combine(this.server.MapPath("~/App_Data/cyo/proofs/"), string.Format("{0}.png", this.guid));
            }
        }

        // TODO: Set graphic to proper size.
        // TODO: Deal with zoomed background image.
        // TODO: Deal with background color.
        // TODO: If there's no background image, or if bg image is smaller than binky image, don't crop!
        // TODO: Handle both text1 and text2.
        // TODO: Proper output file name instead of output.png.
        // TODO: Sanitize everything from cyoModel, since we're passing it to the shell.
        public string CreateProof()
        {
            this.ValidateParams();
            
            Image productImage = Image.FromFile(PathToProductImage);
            Image backgroundImage = null;
            Image graphicImage = null;

            string pathToBgImage = PathToBackgroundImage;
            if (pathToBgImage != null)
                backgroundImage = Image.FromFile(pathToBgImage);
            else
            {
                backgroundImage = new Bitmap(productImage.Width, productImage.Height);
                Graphics bg = Graphics.FromImage(backgroundImage);
                if (!string.IsNullOrEmpty(cyoModel.BgColor))
                {
                    Brush brush = new SolidBrush(ColorTranslator.FromHtml(cyoModel.BgColor));
                    bg.FillRectangle(brush, 0, 0, productImage.Width, productImage.Height);
                }
            }
            string pathToGraphic = PathToGraphic;
            if (pathToGraphic != null)
                graphicImage = Image.FromFile(pathToGraphic);


            Graphics g = Graphics.FromImage(backgroundImage);

            g.DrawImage(productImage, 0, 0);
            if (graphicImage != null)
                g.DrawImage(graphicImage, 100, 100);

            if(!string.IsNullOrEmpty(cyoModel.Text1))
            {
                Color color = ColorTranslator.FromHtml(cyoModel.FontColor1);
                SolidBrush brush = new SolidBrush(color);
                Font font = TextFont(1);
                g.DrawString(cyoModel.Text1, font, brush, 150, 150);
            }

            if(!string.IsNullOrEmpty(cyoModel.Text2))
            {
                Color color = ColorTranslator.FromHtml("#009900");
                SolidBrush brush = new SolidBrush(color);
                Font font = TextFont(2);
                g.DrawString(cyoModel.Text2, font, brush, 200, 200);
            }

            backgroundImage.Save(this.OutputFileName);
            return this.OutputFileName;
        }



        # region Validation / Parameter Sanitization

        /// <summary>
        /// Throw an ArgumentException if we get any bad or potentially
        /// dangerous parameters.
        /// </summary>
        private void ValidateParams()
        {
            if (cyoModel.BackgroundIsUploadedImage)
            {
                string filename = this.ImageBaseName(cyoModel.BgImage);
                string path = Path.Combine(this.server.MapPath("~/App_Data/cyo/uploads"), filename);
                if (!File.Exists(path))
                    throw new CYOInvalidDataException("The uploaded background image does not exist.");
            }
            else if (!string.IsNullOrEmpty(cyoModel.BgImage))
            {
                string filename = this.ImageBaseName(cyoModel.BgImage);
                string path = Path.Combine(this.server.MapPath("~/Content/custom/cyo/images"), filename);
                if (!File.Exists(path))
                    throw new CYOInvalidDataException("The selected background image does not exist.");
            }
            if (!string.IsNullOrEmpty(cyoModel.Graphic))
            {
                string filename = this.ImageBaseName(cyoModel.Graphic);
                string path = Path.Combine(this.server.MapPath("~/Content/custom/cyo/images"), filename);
                if (!File.Exists(path))
                    throw new CYOInvalidDataException("The selected graphic does not exist.");
            }
            if (!string.IsNullOrEmpty(cyoModel.FontFamily1)) 
            {
                string filename = this.FormatFontName(cyoModel.FontFamily1);
                string path = Path.Combine(this.server.MapPath("~/App_Data/cyo/fonts"), filename);
                if (!File.Exists(path))
                    throw new CYOInvalidDataException("The selected font for text element #1 does not exist.");
            }
            if (!string.IsNullOrEmpty(cyoModel.FontFamily2)) 
            {
                string filename = this.FormatFontName(cyoModel.FontFamily2);
                string path = Path.Combine(this.server.MapPath("~/App_Data/cyo/fonts"), filename);
                if (!File.Exists(path))
                    throw new CYOInvalidDataException("The selected font for text element #1 does not exist.");
            }
            if (!string.IsNullOrEmpty(cyoModel.FontColor1) && !legalHexColor.IsMatch(cyoModel.FontColor1))
            {
                throw new CYOInvalidDataException(string.Format("Illegal color '{0}' for text element #1.", cyoModel.FontColor1));
            }
            if (!string.IsNullOrEmpty(cyoModel.FontColor2) && !legalHexColor.IsMatch(cyoModel.FontColor2))
            {
                throw new CYOInvalidDataException(string.Format("Illegal color '{0}' for text element #2.", cyoModel.FontColor2));
            }
            if (!string.IsNullOrEmpty(cyoModel.BgColor) && !legalHexColor.IsMatch(cyoModel.BgColor))
            {
                throw new CYOInvalidDataException(string.Format("Illegal background color '{0}'.", cyoModel.BgColor));
            }            
        }


        #endregion

        # region Text/Font

        public int FontSize(int whichText)
        {
            double fontSize = cyoModel.FontSize1;
            if (whichText == 2)
                fontSize = cyoModel.FontSize2;
            double fontSizeInInches = fontSize / cyoModel.PixelsPerInch;
            double fontSizeInPoints = fontSizeInInches * 72.0;
            return System.Convert.ToInt32(Math.Round(fontSizeInPoints, 0, MidpointRounding.AwayFromZero));                                
        }

        public Point TextOffset(int whichText)
        {
            if (whichText == 1)
                return new Point(System.Convert.ToInt32(cyoModel.TextLeft1), System.Convert.ToInt32(cyoModel.TextTop1));
            else
                return new Point(System.Convert.ToInt32(cyoModel.TextLeft2), System.Convert.ToInt32(cyoModel.TextTop2));
        }

        public Font TextFont(int whichText)
        {
            if (customFonts.Families.Length == 0)
                LoadCustomFonts();
            string fontFamilyName = cyoModel.FontFamily1;
            if (whichText == 2)
                fontFamilyName = cyoModel.FontFamily2;
            FontFamily fontFamily = customFonts.Families.First(ff => ff.Name == fontFamilyName);
            return new Font(fontFamily, FontSize(whichText));
            //return new Font(fontFamily, FontSize(whichText), FontStyle.Regular, GraphicsUnit.Point);
        }

        #endregion

        # region Graphic

        public string PathToGraphic
        {
            get
            {
                string pathToGraphic = null;
                if (!string.IsNullOrEmpty(cyoModel.Graphic))
                    pathToGraphic = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.Graphic));
                return pathToGraphic;
            }
        }

        public Point GraphicOffset
        {
            get
            {
                return new Point(System.Convert.ToInt32(cyoModel.GraphicLeft), System.Convert.ToInt32(cyoModel.GraphicTop));
            }
        }

        # endregion

        # region Product

        public string PathToProductImage
        {
            get
            {
                return Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.SampleImage));
            }
        }


        public Point ProductSampleOffset
        {
            get
            {
                return new Point(System.Convert.ToInt32(cyoModel.SampleLeft), System.Convert.ToInt32(cyoModel.SampleTop));
            }
        }

        #endregion

        # region Background

        public string PathToBackgroundImage
        {
            get
            {
                string pathToBgImage = null;
                if (!string.IsNullOrEmpty(cyoModel.BgImage))
                {
                    if (cyoModel.BackgroundIsUploadedImage)
                        pathToBgImage = Path.Combine(this.server.MapPath("~/App_Data/cyo/uploads"), ImageBaseName(cyoModel.BgImage));
                    else
                        pathToBgImage = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.BgImage));
                }
                return pathToBgImage;
            }
        }


        public Point BackgroundImageOffset
        {
            get
            {
                return new Point(System.Convert.ToInt32(cyoModel.BgImageOffsetX), System.Convert.ToInt32(cyoModel.BgImageOffsetY));
            }
        }

        #endregion

        # region Utilities

        public string ImageBaseName(string imageURI)
        {
            Match match = cyoUploadedFileName.Match(imageURI);
            if(match.Success)
                return match.Groups[1].Value;
            string[] parts = imageURI.Split(new char[] { '/' });
            return parts.Last();
        }

        public string FormatFontName(string fontName)
        {
            return string.Format("{0}-Regular.ttf", fontName.Replace(" ", ""));
        }

        private void LoadCustomFonts()
        {
            IEnumerable fontFiles = Directory.EnumerateFiles(this.server.MapPath("~/App_Data/cyo/fonts"), "*.ttf");
            foreach (string filename in fontFiles)
                customFonts.AddFontFile(filename);
        }

        # endregion

    }

    public class CYOInvalidDataException : System.Exception
    {
        public CYOInvalidDataException(string message) : base(message)
        {

        }
    }

}