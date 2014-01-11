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

        # region Rendering 

        public string CreateProof()
        {
            this.ValidateParams();

            Image productImage = Image.FromFile(PathToProductImage);
            Bitmap bitmap = new Bitmap(productImage.Width, productImage.Height);
            Graphics g = Graphics.FromImage(bitmap);

            // Do this, or your text will look like crap.
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            Image backgroundImage = null;
            

            string pathToBgImage = PathToBackgroundImage;
            bool hasBgImage = !string.IsNullOrEmpty(pathToBgImage);
            if (hasBgImage)
            {
                backgroundImage = Image.FromFile(pathToBgImage);
                // To reflect what the browser shows, we must resize 
                // the background image to the size of the product image
                // because the browser forces the CSS background image
                // to fit into the div that contains the product image.
                if (backgroundImage.Width > productImage.Width)
                {
                    double zoom = productImage.Width / System.Convert.ToDouble(backgroundImage.Width);
                    int width = System.Convert.ToInt32(zoom * backgroundImage.Width);
                    int height = System.Convert.ToInt32(zoom * backgroundImage.Height);
                    backgroundImage = (Image)new Bitmap(backgroundImage, width, height);
                }
                else if (backgroundImage.Height > productImage.Height)
                {
                    double zoom = productImage.Height / System.Convert.ToDouble(backgroundImage.Height);
                    int width = System.Convert.ToInt32(zoom * backgroundImage.Width);
                    int height = System.Convert.ToInt32(zoom * backgroundImage.Height);
                    backgroundImage = (Image)new Bitmap(backgroundImage, width, height);
                }
                // Now apply the zoom.
                if(cyoModel.BgImageZoom != 100) {
                    double zoom = cyoModel.BgImageZoom / 100.0;
                    int width = System.Convert.ToInt32(zoom * backgroundImage.Width);
                    int height = System.Convert.ToInt32(zoom * backgroundImage.Height);
                    backgroundImage = (Image)new Bitmap(backgroundImage, width, height);
                }
                int backgroundX = cyoModel.BgImageOffsetX;// -cyoModel.SampleLeft;
                int backgroundY = cyoModel.BgImageOffsetY;// -cyoModel.SampleTop;
                g.DrawImage(backgroundImage, backgroundX, backgroundY); //, backgroundImage.Height, backgroundImage.Width);                
            }
            else
            {
                if (!string.IsNullOrEmpty(cyoModel.BgColor))
                {
                    Brush brush = new SolidBrush(ColorTranslator.FromHtml(cyoModel.BgColor));
                    g.FillRectangle(brush, 0, 0, productImage.Width, productImage.Height);
                }
            }


            RenderGraphic(g, productImage);
            RenderText(g, 1, cyoModel.Text1, cyoModel.FontColor1, cyoModel.TextLeft1, cyoModel.TextTop1);
            RenderText(g, 2, cyoModel.Text2, cyoModel.FontColor2, cyoModel.TextLeft2, cyoModel.TextTop2);

            g.Dispose();
            bitmap.Save(this.OutputFileName);
            return ImageBaseName(this.OutputFileName);
        }


        /// <summary>
        /// Render the graphic overlay that goes on top of the background.
        /// </summary>
        /// <param name="graphics"></param>
        public void RenderGraphic(Graphics graphics, Image productImage)
        {
            Image graphicImage = null;
            string pathToGraphic = PathToGraphic;
            if (pathToGraphic != null)
                graphicImage = Image.FromFile(pathToGraphic);

            graphics.DrawImage(productImage, 0, 0, productImage.Width, productImage.Height);

            if (graphicImage != null)
            {
                int x = cyoModel.GraphicLeft - cyoModel.SampleLeft;
                int y = cyoModel.GraphicTop - cyoModel.SampleTop;
                graphics.DrawImage(graphicImage, x, y, cyoModel.GraphicWidth, cyoModel.GraphicHeight);
            }
        }

        /// <summary>
        /// Render the text. This goes on top of all other layers,
        /// with text2 being topmost.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="whichText"></param>
        /// <param name="text"></param>
        /// <param name="hexColor"></param>
        /// <param name="leftOffset"></param>
        /// <param name="topOffset"></param>
        public void RenderText(Graphics graphics, int whichText, string text, string hexColor,  int leftOffset, int topOffset)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Color color = ColorTranslator.FromHtml(hexColor);
                SolidBrush brush = new SolidBrush(color);
                Font font = TextFont(whichText);
                int x = leftOffset - cyoModel.SampleLeft;
                int y = topOffset - cyoModel.SampleTop;
                graphics.DrawString(text, font, brush, x, y);
            }
        }

        #endregion

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
                return new Point(cyoModel.TextLeft1, cyoModel.TextTop1);
            else
                return new Point(cyoModel.TextLeft2, cyoModel.TextTop2);
        }

        public Font TextFont(int whichText)
        {
            if (customFonts.Families.Length == 0)
                LoadCustomFonts();
            string fontFamilyName = cyoModel.FontFamily1;
            if (whichText == 2)
                fontFamilyName = cyoModel.FontFamily2;
            FontFamily fontFamily = customFonts.Families.First(ff => ff.Name == fontFamilyName);
            Font font = null;
            int fontSize = FontSize(whichText);
            if (fontFamily.IsStyleAvailable(FontStyle.Regular))
                font = new Font(fontFamily, fontSize, FontStyle.Regular);
            else if (fontFamily.IsStyleAvailable(FontStyle.Bold))
                font = new Font(fontFamily, fontSize, FontStyle.Bold);
            else if (fontFamily.IsStyleAvailable(FontStyle.Italic))
                font = new Font(fontFamily, fontSize, FontStyle.Italic);
            else
                throw new InvalidDataException(string.Format("Font family '{0}' does not support Regular, Bold or Italic styles.", fontFamily.Name));
            return font;
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
                return new Point(cyoModel.GraphicLeft, cyoModel.GraphicTop);
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
                return new Point(cyoModel.SampleLeft, cyoModel.SampleTop);
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
                return new Point(cyoModel.BgImageOffsetX, cyoModel.BgImageOffsetY);
            }
        }

        #endregion

        # region Utilities

        public string OutputFileName
        {
            get
            {
                return Path.Combine(this.server.MapPath("~/App_Data/cyo/proofs/"), string.Format("{0}.png", this.guid));
            }
        }

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