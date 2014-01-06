using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Nop.Web.Models.Custom
{
    public class CYOImageHelper
    {
        private CYOModel cyoModel = null;

        public CYOImageHelper(CYOModel cyoModel)
        {
            this.cyoModel = cyoModel;
        }

        private string GetProofCommand()
        {

            StringBuilder sb = new StringBuilder("convert -crop ");
            //convert -crop 674x479+165+145 \
            //-page +0+0 Desert.jpg \
            //-page +165+145 shield_white.png \
            //-page +331+236 SkullGirl.png \
            //-layers merge +repage \
            //-pointsize 30 \
            //-font "GoogleWebFonts/Allura-Regular.ttf" \
            //-fill blue \
            //-gravity None \
            //-annotate +200+300 'Precious Baby Snookums' \
            //output.png ";

            return sb.ToString();
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
        /// Offset of productSampleImage from 0,0 of background image.
        /// The background image may be bigger or smaller than the binky sample,
        /// with only a portion of the background showing through the middle of
        /// the binky. If the background is larger than the binky sample,
        /// the offset will be positive. If the background is smaller than the
        /// binky, the offset may be negative.
        /// </summary>
        public string ProductOffset
        {
            get
            {
                string productSampleOffset = null;
                if (!string.IsNullOrEmpty(cyoModel.BgImage))
                {
                    productSampleOffset = string.Format("{0}{1}{2}{3}",
                        this.SignFor(cyoModel.BgImageOffsetX), System.Convert.ToInt32(cyoModel.BgImageOffsetX),
                        this.SignFor(cyoModel.BgImageOffsetY), System.Convert.ToInt32(cyoModel.BgImageOffsetY));
                }
                return productSampleOffset;
            }
        }

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

    }
}