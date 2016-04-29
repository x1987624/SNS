//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-21</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-21" version="0.5">创建</log>
//<log date="2012-02-26" version="0.51" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tunynet.Imaging
{
    /// <summary>
    /// 水印过滤器基类
    /// </summary>
    public abstract class WatermarkFilterBase : IImageFilter
    {

        /// <summary>
        /// 水印所在位置
        /// </summary>
        public AnchorLocation AnchorLocation { get; set; }

        private float _opacity = 0.8F;
        /// <summary>
        /// 不透明度
        /// </summary>
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value < 0)
                    _opacity = 0;
                else if (value > 1)
                    _opacity = 1;
                else
                    _opacity = value;
            }
        }

        /// <summary>
        /// 对传入的inputImage进行处理
        /// </summary>
        /// <param name="inputImage">待处理的图像文件</param>
        /// <param name="isProcessed">是否被处理，否则返回原图</param>
        /// <returns>返回处理后的图像文件</returns>
        public abstract Image Process(Image inputImage, out bool isProcessed);



        /// <summary>
        /// 判断图片是否带有索引像素格式
        /// </summary>
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>
        protected static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare,
                PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed,PixelFormat.Format8bppIndexed };

            if (indexedPixelFormats.Contains(imgPixelFormat))
                return true;
            else
                return false;
        }

    }
}
