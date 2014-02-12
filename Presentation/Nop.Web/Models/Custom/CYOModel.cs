using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Nop.Web.Models.Custom
{
    /// <summary>
    /// The CYOModel contains information about the customized product.
    /// This model is NOT saved to the database, so don't expect to find
    /// a table for it there.
    /// </summary>
    public class CYOModel
    {
        Regex illegalForDouble = new Regex(@"[^0-9\.\-]");
        public static readonly Regex guidRegex = new Regex(@"([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})", RegexOptions.IgnoreCase);



        #region Constructors

        public CYOModel() { }

        /// <summary>
        /// Constructs a CYOModel from Http request params.
        /// Check the Errors property after constructing to see
        /// if the model is valid.
        /// </summary>
        /// <param name="requestParams"></param>
        public CYOModel(NameValueCollection requestParams)
        {
            ProductSize = requestParams["cyoProductSize"];
            ProductColor = requestParams["cyoProductColor"];
            SampleImage = requestParams["cyoSampleImage"];
            Brand = requestParams["cyoBrand"];
            SampleTop = ParseInt(requestParams["cyoSampleTop"]);
            SampleLeft = ParseInt(requestParams["cyoSampleLeft"]);
            BgImage = requestParams["cyoBgImage"];
            UploadedImage = requestParams["cyoUploadedImage"];
            BgImageZoom = ParseInt(requestParams["cyoBgImageZoom"]);
            BgImageOffsetX = ParseInt(requestParams["cyoBgImageOffsetX"]);
            BgImageOffsetY = ParseInt(requestParams["cyoBgImageOffsetY"]);
            BgColor = requestParams["cyoBgColor"];
            Text1 = requestParams["cyoText1"];
            FontFamily1 = requestParams["cyoFontFamily1"].Replace("\"", "");  // IE sends some fonts with quotes
            FontColor1 = requestParams["cyoFontColor1"];
            FontSize1 = ParseInt(requestParams["cyoFontSize1"]);
            TextTop1 = ParseInt(requestParams["cyoTextTop1"]);
            TextLeft1 = ParseInt(requestParams["cyoTextLeft1"]);
            TextHeight1 = ParseInt(requestParams["cyoTextHeight1"]);
            TextWidth1 = ParseInt(requestParams["cyoTextWidth1"]);
            Text2 = requestParams["cyoText2"];
            FontFamily2 = requestParams["cyoFontFamily2"].Replace("\"", "");  // IE sends some fonts with quotes
            FontColor2 = requestParams["cyoFontColor2"];
            FontSize2 = ParseInt(requestParams["cyoFontSize2"]);
            TextTop2 = ParseInt(requestParams["cyoTextTop2"]);
            TextLeft2 = ParseInt(requestParams["cyoTextLeft2"]);
            TextHeight2 = ParseInt(requestParams["cyoTextHeight2"]);
            TextWidth2 = ParseInt(requestParams["cyoTextWidth2"]);
            Graphic = requestParams["cyoGraphic"];
            GraphicIsBackground = ParseBool(requestParams["cyoGraphicIsBackground"], false);
            GraphicTop = ParseInt(requestParams["cyoGraphicTop"]);
            GraphicLeft = ParseInt(requestParams["cyoGraphicLeft"]);
            GraphicWidth = ParseInt(requestParams["cyoGraphicWidth"]);
            GraphicHeight = ParseInt(requestParams["cyoGraphicHeight"]);
            GraphicZoom = ParseInt(requestParams["cyoGraphicZoom"]);
            PixelsPerInch = ParseInt(requestParams["cyoPixelsPerInch"]);
        }

        #endregion

        # region Static Methods

        public static string ExtractGuid(string str)
        {
            Match m = guidRegex.Match(str);
            if (m.Success)
                return m.Groups[1].Value;
            return null;
        }

        #endregion

        #region Public Properties

        public string ProductSize { get; set; }
        public string ProductColor { get; set; }
        public string SampleImage { get; set; }
        public string Brand { get; set; }
        public int SampleTop { get; set; }
        public int SampleLeft { get; set; }
        public string BgImage { get; set; }
        public string UploadedImage { get; set; }
        public int BgImageZoom { get; set; }
        public int BgImageOffsetX { get; set; }
        public int BgImageOffsetY { get; set; }
        public string BgColor { get; set; }
        public string Text1 { get; set; }
        public string FontFamily1 { get; set; }
        public string FontColor1 { get; set; }
        public int FontSize1 { get; set; }
        public int TextTop1 { get; set; }
        public int TextLeft1 { get; set; }
        public int TextHeight1 { get; set; }
        public int TextWidth1 { get; set; }
        public string Text2 { get; set; }
        public string FontFamily2 { get; set; }
        public string FontColor2 { get; set; }
        public int FontSize2 { get; set; }
        public int TextTop2 { get; set; }
        public int TextLeft2 { get; set; }
        public int TextHeight2 { get; set; }
        public int TextWidth2 { get; set; }
        public string Graphic { get; set; }
        public bool GraphicIsBackground { get; set; }
        public int GraphicTop { get; set; }
        public int GraphicLeft { get; set; }
        public int GraphicWidth { get; set; }
        public int GraphicHeight { get; set; }
        public int GraphicZoom { get; set; }
        public int PixelsPerInch { get; set; }

        /// <summary>
        /// Returns true if the background image was uploaded by the user.
        /// Returns false if the background image is one of Booginhead's
        /// stock image, or if there is no background image.
        /// </summary>
        public bool BackgroundIsUploadedImage
        {
            get
            {
                return (!string.IsNullOrEmpty(this.BgImage) && this.BgImage == this.UploadedImage);
            }
        }

        /// <summary>
        /// Returns a list of errors describing missing or
        /// invalid info. The list will be empty if there
        /// are no errors. 
        /// </summary>
        public List<string> Errors
        {
            get
            {
                List<string> errors = RequiredPropertyErrors();
                errors.AddRange(BackgroundPropertyErrors());
                errors.AddRange(GraphicPropertyErrors());
                errors.AddRange(Text1PropertyErrors());
                errors.AddRange(Text2PropertyErrors());
                return errors;
            }
        }

        #endregion

        #region Public Methods

        public bool IntValueIsMissing(double value)
        {
            return value == Double.MinValue;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a list of errors for TextLine1. If there
        /// are no errors, returns an empty list.
        /// </summary>
        /// <returns></returns>
        private List<string> Text1PropertyErrors()
        {
            List<string> errors = new List<string>();
            if (!string.IsNullOrEmpty(Text1))
            {
                if (string.IsNullOrEmpty(FontColor1))
                    errors.Add("Text color for text line 1 is missing.");
                if (string.IsNullOrEmpty(FontFamily1))
                    errors.Add("Font family for text line 1 is missing.");
                if (IntValueIsMissing(FontSize1))
                    errors.Add("Font size for text line 1 is missing.");
                if (IntValueIsMissing(TextTop1))
                    errors.Add("Top offset for text line 1 is missing.");
                if (IntValueIsMissing(TextLeft1))
                    errors.Add("Left offset for text line 1 is missing.");
                if (IntValueIsMissing(TextHeight1))
                    errors.Add("Container height for text line 1 is missing.");
                if (IntValueIsMissing(TextWidth1)) 
                    errors.Add("Container width for text line 1 is missing.");
            }
            return errors;
        }

        /// <summary>
        /// Returns a list of errors for TextLine2. If there
        /// are no errors, returns an empty list.
        /// </summary>
        /// <returns></returns>
        private List<string> Text2PropertyErrors()
        {
            List<string> errors = new List<string>();
            if (!string.IsNullOrEmpty(Text2))
            {
                if (string.IsNullOrEmpty(FontColor2))
                    errors.Add("Text color for text line 2 is missing.");
                if (string.IsNullOrEmpty(FontFamily2))
                    errors.Add("Font family for text line 2 is missing.");
                if (IntValueIsMissing(FontSize2))
                    errors.Add("Font size for text line 2 is missing.");
                if (IntValueIsMissing(TextTop2))
                    errors.Add("Top offset for text line 2 is missing.");
                if (IntValueIsMissing(TextLeft2))
                    errors.Add("Left offset for text line 2 is missing.");
                if (IntValueIsMissing(TextHeight2))
                    errors.Add("Container height for text line 2 is missing.");
                if (IntValueIsMissing(TextWidth2))
                    errors.Add("Container width for text line 2 is missing.");
            }
            return errors;
        }


        /// <summary>
        /// Returns a list of errors describing missing or 
        /// invalid data related to the graphic image. (This is
        /// the image that displays on top of the background.) These
        /// properties are not required if there is no graphic
        /// image. Returns an empty list if there are no errors.
        /// </summary>
        /// <returns></returns>
        private List<string> GraphicPropertyErrors()
        {
            List<string> errors = new List<string>();
            if (!string.IsNullOrEmpty(Graphic))
            {
                if (IntValueIsMissing(GraphicTop))
                    errors.Add("Graphic top is missing.");
                if (IntValueIsMissing(GraphicLeft))
                    errors.Add("Graphic left is missing.");
                if (IntValueIsMissing(GraphicWidth))
                    errors.Add("Graphic width is missing.");
                if (IntValueIsMissing(GraphicHeight))
                    errors.Add("Graphic height is missing.");
                if (IntValueIsMissing(GraphicZoom))
                    errors.Add("Graphic zoom is missing.");
            }
            return errors;
        }

        /// <summary>
        /// Returns a list of errors describing missing or 
        /// invalid data related to the background image. These
        /// properties are not required if there is no background
        /// image. Returns an empty list if there are no errors.
        /// </summary>
        /// <returns></returns>
        private List<string> BackgroundPropertyErrors()
        {
            List<string> errors = new List<string>();
            if (GraphicIsBackground && string.IsNullOrEmpty(BgImage))
                errors.Add("Background image graphic is missing.");
            if(!string.IsNullOrEmpty(BgImage)) {
                if(IntValueIsMissing(BgImageZoom))
                    errors.Add("Background image zoom is missing.");
                if (IntValueIsMissing(BgImageOffsetX))
                    errors.Add("Background image offset X is missing.");
                if (IntValueIsMissing(BgImageOffsetY))
                    errors.Add("Background image offset Y is missing.");
            }
            return errors;
        }

        /// <summary>
        /// Returns a list of errors for each required property that is 
        /// missing or invalid. An empty list means there were no errors.
        /// </summary>
        /// <returns></returns>
        private List<string> RequiredPropertyErrors()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(ProductSize))
                errors.Add("Please choose a product size.");
            if (string.IsNullOrEmpty(ProductColor))
                errors.Add("Please choose a product color.");
            if (string.IsNullOrEmpty(SampleImage))
                errors.Add("Sample image is missing.");
            if (string.IsNullOrEmpty(Brand))
                errors.Add("Please choose a brand.");
            if (IntValueIsMissing(SampleTop))
                errors.Add("Sample top position is missing.");
            if (IntValueIsMissing(SampleLeft))
                errors.Add("Please choose a product size.");
            return errors;
        }

        private double ParseDouble(string value)
        {
            double result = Double.MinValue;
            if(!string.IsNullOrEmpty(value))
            {
                string legalValue = illegalForDouble.Replace(value, "");
                Double.TryParse(legalValue, out result);
            }
            return result;
        }

        private int ParseInt(string value)
        {
            double result = ParseDouble(value);
            if (result == Double.MinValue)
                return Int32.MinValue;
            return System.Convert.ToInt32(Math.Round(result, MidpointRounding.AwayFromZero));
        }

        private bool ParseBool(string value, bool defaultValue)
        {
            bool result = defaultValue;
            try { result = Boolean.Parse(value); }
            catch (Exception ex) {}
            return result;
        }

        #endregion
    }
}