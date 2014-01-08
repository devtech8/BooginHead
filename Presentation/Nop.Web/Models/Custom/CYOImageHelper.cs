using System;
using System.Collections.Generic;
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

        # region GetProofCommand 

        // TODO: Deal with resized graphic.
        // TODO: Deal with zoomed background image.
        // TODO: Deal with background color.
        // TODO: If there's no background image, or if bg image is smaller than binky image, don't crop!
        // TODO: Handle both text1 and text2.
        // TODO: Proper output file name instead of output.png.
        // TODO: Sanitize everything from cyoModel, since we're passing it to the shell.
        public string GetProofCommand()
        {
            // The first part of the command should look something
            // like the following, with no spaces between the numbers.
            // convert -crop 674x479+165+145
            StringBuilder sb = new StringBuilder("convert -crop ");
            sb.Append(this.ProductSampleDimensions);
            sb.Append(this.ProductSampleOffset);

            // If there is a background image, add it to the proof.
            // The param will look something like this:
            // -page +0+0 Desert.jpg
            sb.Append(this.BackgroundParam);

            // Add the product (the binky) to the image.
            // The param should look something like this.
            // -page +165+145 shield_white.png 
            sb.Append(this.ProductParam);

            // Add the param to include the graphic, if
            // one is specified. It should look something
            // like this:
            // -page +331+236 SkullGirl.png
            sb.Append(this.GraphicParam);

            // Returns the following param, if we need it.
            // -layers merge +repage 
            sb.Append(this.LayersParam);

            // The following params are added only if the
            // user specified some text.

            // Add param for font size, like so:
            // -pointsize 30 
            sb.Append(this.FontSizeParam);

            // -font "cyo/fonts/Allura-Regular.ttf" 
            sb.Append(this.FontFamilyParam);

            // -fill blue 
            sb.Append(this.FontColorParam);

            //-gravity None \
            sb.Append(this.FontGravityParam);

            //-annotate +200+300 'Precious Baby Snookums' \
            sb.Append(this.TextPositionAndContentParam);

            //output.png ";
            sb.Append(this.OutputFileName);

            return sb.ToString();
        }

        # endregion

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

        /// <summary>
        /// Returns a string describing the font size, 
        /// in points.
        /// </summary>
        public string FontSizeParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Text1))
                {
                    double fontSizeInInches = cyoModel.FontSize1 / cyoModel.PixelsPerInch;
                    double fontSizeInPoints = fontSizeInInches * 72.0;
                    int pointsize = System.Convert.ToInt32(Math.Round(fontSizeInPoints, 0, MidpointRounding.AwayFromZero));
                    return string.Format("-pointsize {0} ", pointsize);
                }
                return "";
            }
        }

        /// <summary>
        /// Returns a string specifying the font family used
        /// to render the user's custom text.
        /// </summary>
        public string FontFamilyParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Text1))
                {
                    string pathToFont = Path.Combine(this.server.MapPath("~/App_Data/cyo/fonts"), this.FormatFontName(cyoModel.FontFamily1));
                    return string.Format("-font \"{0}\" ", pathToFont);
                }
                return "";
            }
        }

        /// <summary>
        /// Returns a string specifying the font color. 
        /// ImageMagick supports essentially the same colors
        /// as CSS: named colors like "red" and hex colors.
        /// </summary>
        public string FontColorParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Text1))
                {
                    return string.Format("-fill \"{0}\" ", cyoModel.FontColor1);
                }
                return "";
            }
        }


        /// <summary>
        /// Returns a string telling ImageMagic not to use any
        /// special "gravity" for the text. The gravity can be
        /// something like "center", "south" or "northwest" to
        /// indicate that the position of the text should be 
        /// relative to ceter, an edge, or a corner. We don't
        /// want any special gravity for the text. We'll position
        /// it relative to the NorthEast corner, just like every
        /// other item we're positioning.
        /// </summary>
        public string FontGravityParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Text1))
                {
                    return "-gravity None ";
                }
                return "";
            }
        }


        /// <summary>
        /// Returns a string describing the position and content
        /// of the text.
        /// </summary>
        public string TextPositionAndContentParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Text1))
                {
                    return string.Format("-annotate {0} \"{1}\" ", this.TextOffset, cyoModel.Text1.Replace("\"", "\\\""));
                }
                return "";
            }
        }


        /// <summary>
        /// Returns a string describing how far to offset the text
        /// from the top left corner of the image.
        /// </summary>
        public string TextOffset
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                        this.SignFor(cyoModel.TextLeft1), System.Convert.ToInt32(cyoModel.TextLeft1),
                        this.SignFor(cyoModel.TextTop1), System.Convert.ToInt32(cyoModel.TextTop1));
            }
        }

        #endregion

        #region Layers

        /// <summary>
        /// If we have more than one layer, return a param to 
        /// merge and repage the layers.
        /// </summary>
        public string LayersParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.BgImage) || !string.IsNullOrEmpty(cyoModel.Graphic))
                {
                    return "-layers merge +repage ";
                }
                return "";
            }
        }

        # endregion

        # region Graphic

        /// <summary>
        /// Returns a string telling ImageMagick to add the graphic
        /// overlay to the image. The graphic appears on top of any
        /// other images, but below the text. The graphic may not be
        /// specified, in which case this returns an empty string.
        /// </summary>
        public string GraphicParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.Graphic))
                {
                    string pathToGraphic = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.Graphic));
                    return string.Format("-page {0} \"{1}\" ", this.GraphicOffset, pathToGraphic);
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the offset of the custom graphic from 0,0 of full image
        /// in the form of a string that ImageMagick understands.
        /// </summary>
        public string GraphicOffset
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                        this.SignFor(cyoModel.GraphicLeft), System.Convert.ToInt32(cyoModel.GraphicLeft),
                        this.SignFor(cyoModel.GraphicTop), System.Convert.ToInt32(cyoModel.GraphicTop));
            }
        }

        # endregion

        # region Product

        /// <summary>
        /// Returns a string telling ImageMagick to add the product 
        /// (the binky) to the proof.
        /// </summary>
        public string ProductParam
        {
            get
            {
                string pathToSampleImage = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.SampleImage));
                return string.Format("-page {0} \"{1}\" ", this.ProductSampleOffset, ImageBaseName(pathToSampleImage));
            }
        }

        /// <summary>
        /// Returns a string describing the dimensions of the selected sample binky image.
        /// </summary>
        public string ProductSampleDimensions
        {
            get
            {
                string productSampleDimensions = string.Format("{0}x{1}", CYOModel.BOOGINHEAD_IMAGE_WIDTH, CYOModel.BOOGINHEAD_IMAGE_HEIGHT);
                if (cyoModel.Brand == "nuk")
                    productSampleDimensions = string.Format("{0}x{1}", CYOModel.NUK_IMAGE_WIDTH, CYOModel.NUK_IMAGE_HEIGHT);
                return productSampleDimensions;
            }
        }

        /// <summary>
        /// Offset of productSampleImage from 0,0 of full image.
        /// The background image may be bigger or smaller than the binky sample,
        /// with only a portion of the background showing through the middle of
        /// the binky. If the background is larger than the binky sample,
        /// the offset will be positive. If the background is smaller than the
        /// binky, the offset may be negative.
        /// </summary>
        public string ProductSampleOffset
        {
            get
            {
                return string.Format("{0}{1}{2}{3} ",
                        this.SignFor(cyoModel.SampleLeft), System.Convert.ToInt32(cyoModel.SampleLeft),
                        this.SignFor(cyoModel.SampleTop), System.Convert.ToInt32(cyoModel.SampleTop));
            }
        }

        #endregion

        # region Background

        /// <summary>
        /// Returns a string telling ImageMagick to add the background image
        /// to the proof. Returns an empty string if there is no background
        /// image to add.
        /// </summary>
        public string BackgroundParam
        {
            get
            {
                if (!string.IsNullOrEmpty(cyoModel.BgImage))
                {
                    string pathToBgImage = null;
                    if (cyoModel.BackgroundIsUploadedImage)
                        pathToBgImage = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/uploads"), ImageBaseName(cyoModel.BgImage));
                    else
                        pathToBgImage = Path.Combine(this.server.MapPath("~/Content/Custom/cyo/images"), ImageBaseName(cyoModel.BgImage));
                    return string.Format("-page {0} \"{1}\" ", this.BackgroundImageOffset, pathToBgImage);
                }
                return "";
            }
        }


        /// <summary>
        /// Returns the offset, from the top left corner, of the background image.
        /// </summary>
        public string BackgroundImageOffset
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                        this.SignFor(cyoModel.BgImageOffsetX), System.Convert.ToInt32(cyoModel.BgImageOffsetX),
                        this.SignFor(cyoModel.BgImageOffsetX), System.Convert.ToInt32(cyoModel.BgImageOffsetY));
            }
        }

        #endregion

        # region Utilities

        /// <summary>
        /// Returns "+" or "-" based on the value of the input param.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SignFor(double value)
        {
            string sign = (value >= 0 ? "+" : "-");
            return sign;
        }

        /// <summary>
        /// Returns "+" or "-" based on the value of the input param.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SignFor(int value)
        {
            string sign = (value >= 0 ? "+" : "-");
            return sign;
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

        # endregion

    }

    public class CYOInvalidDataException : System.Exception
    {
        public CYOInvalidDataException(string message) : base(message)
        {

        }
    }

}