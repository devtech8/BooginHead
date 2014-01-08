using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        // Height & width of product sample images.
        public static readonly int BOOGINHEAD_IMAGE_WIDTH = 472;
        public static readonly int BOOGINHEAD_IMAGE_HEIGHT = 335;
        public static readonly int NUK_IMAGE_WIDTH = 472;
        public static readonly int NUK_IMAGE_HEIGHT = 335;

        Regex illegalForDouble = new Regex(@"[^0-9\.\-]");

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
            SampleTop = ParseDouble(requestParams["cyoSampleTop"]);
            SampleLeft = ParseDouble(requestParams["cyoSampleLeft"]);
            BgImage = requestParams["cyoBgImage"];
            UploadedImage = requestParams["cyoUploadedImage"];
            BgImageZoom = ParseDouble(requestParams["cyoBgImageZoom"]);
            BgImageOffsetX = ParseDouble(requestParams["cyoBgImageOffsetX"]);
            BgImageOffsetY = ParseDouble(requestParams["cyoBgImageOffsetY"]);
            BgColor = requestParams["cyoBgColor"];
            Text1 = requestParams["cyoText1"];
            FontFamily1 = requestParams["cyoFontFamily1"];
            FontColor1 = requestParams["cyoFontColor1"];
            FontSize1 = ParseDouble(requestParams["cyoFontSize1"]);
            TextTop1 = ParseDouble(requestParams["cyoTextTop1"]);
            TextLeft1 = ParseDouble(requestParams["cyoTextLeft1"]);
            TextHeight1 = ParseDouble(requestParams["cyoTextHeight1"]);
            TextWidth1 = ParseDouble(requestParams["cyoTextWidth1"]);
            Text2 = requestParams["cyoText2"];
            FontFamily2 = requestParams["cyoFontFamily2"];
            FontColor2 = requestParams["cyoFontColor2"];
            FontSize2 = ParseDouble(requestParams["cyoFontSize2"]);
            TextTop2 = ParseDouble(requestParams["cyoTextTop2"]);
            TextLeft2 = ParseDouble(requestParams["cyoTextLeft2"]);
            TextHeight2 = ParseDouble(requestParams["cyoTextHeight2"]);
            TextWidth2 = ParseDouble(requestParams["cyoTextWidth2"]);
            Graphic = requestParams["cyoGraphic"];
            GraphicIsBackground = ParseBool(requestParams["cyoGraphicIsBackground"], false);
            GraphicTop = ParseDouble(requestParams["cyoGraphicTop"]);
            GraphicLeft = ParseDouble(requestParams["cyoGraphicLeft"]);
            GraphicWidth = ParseDouble(requestParams["cyoGraphicWidth"]);
            GraphicHeight = ParseDouble(requestParams["cyoGraphicHeight"]);
            GraphicZoom = ParseDouble(requestParams["cyoGraphicZoom"]);
            PixelsPerInch = ParseDouble(requestParams["cyoPixelsPerInch"]);
        }

        #endregion

        #region Public Properties

        public string ProductSize { get; set; }
        public string ProductColor { get; set; }
        public string SampleImage { get; set; }
        public string Brand { get; set; }
        public double SampleTop { get; set; }
        public double SampleLeft { get; set; }
        public string BgImage { get; set; }
        public string UploadedImage { get; set; }
        public double BgImageZoom { get; set; }
        public double BgImageOffsetX { get; set; }
        public double BgImageOffsetY { get; set; }
        public string BgColor { get; set; }
        public string Text1 { get; set; }
        public string FontFamily1 { get; set; }
        public string FontColor1 { get; set; }
        public double FontSize1 { get; set; }
        public double TextTop1 { get; set; }
        public double TextLeft1 { get; set; }
        public double TextHeight1 { get; set; }
        public double TextWidth1 { get; set; }
        public string Text2 { get; set; }
        public string FontFamily2 { get; set; }
        public string FontColor2 { get; set; }
        public double FontSize2 { get; set; }
        public double TextTop2 { get; set; }
        public double TextLeft2 { get; set; }
        public double TextHeight2 { get; set; }
        public double TextWidth2 { get; set; }
        public string Graphic { get; set; }
        public bool GraphicIsBackground { get; set; }
        public double GraphicTop { get; set; }
        public double GraphicLeft { get; set; }
        public double GraphicWidth { get; set; }
        public double GraphicHeight { get; set; }
        public double GraphicZoom { get; set; }
        public double PixelsPerInch { get; set; }

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

        public bool DoubleValueIsMissing(double value)
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
                if (DoubleValueIsMissing(FontSize1))
                    errors.Add("Font size for text line 1 is missing.");
                if (DoubleValueIsMissing(TextTop1))
                    errors.Add("Top offset for text line 1 is missing.");
                if (DoubleValueIsMissing(TextLeft1))
                    errors.Add("Left offset for text line 1 is missing.");
                if (DoubleValueIsMissing(TextHeight1))
                    errors.Add("Container height for text line 1 is missing.");
                if (DoubleValueIsMissing(TextWidth1)) 
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
                if (DoubleValueIsMissing(FontSize2))
                    errors.Add("Font size for text line 2 is missing.");
                if (DoubleValueIsMissing(TextTop2))
                    errors.Add("Top offset for text line 2 is missing.");
                if (DoubleValueIsMissing(TextLeft2))
                    errors.Add("Left offset for text line 2 is missing.");
                if (DoubleValueIsMissing(TextHeight2))
                    errors.Add("Container height for text line 2 is missing.");
                if (DoubleValueIsMissing(TextWidth2))
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
                if (DoubleValueIsMissing(GraphicTop))
                    errors.Add("Graphic top is missing.");
                if (DoubleValueIsMissing(GraphicLeft))
                    errors.Add("Graphic left is missing.");
                if (DoubleValueIsMissing(GraphicWidth))
                    errors.Add("Graphic width is missing.");
                if (DoubleValueIsMissing(GraphicHeight))
                    errors.Add("Graphic height is missing.");
                if (DoubleValueIsMissing(GraphicZoom))
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
                if(DoubleValueIsMissing(BgImageZoom))
                    errors.Add("Background image zoom is missing.");
                if (DoubleValueIsMissing(BgImageOffsetX))
                    errors.Add("Background image offset X is missing.");
                if (DoubleValueIsMissing(BgImageOffsetY))
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
            if (DoubleValueIsMissing(SampleTop))
                errors.Add("Sample top position is missing.");
            if (DoubleValueIsMissing(SampleLeft))
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