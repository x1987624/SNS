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
using System.Drawing.Drawing2D;

namespace Tunynet.Imaging
{
    /// <summary>
    /// 用于裁剪的图片处理过滤器
    /// </summary>
    public class CropFilter : IImageFilter
    {
        /// <summary>
        /// 期望图像尺寸
        /// </summary>
        public Size TargetSize { get; private set; }

        /// <summary>
        /// 原图待裁剪的矩形选区
        /// </summary>
        public Rectangle CropArea { get; private set; }

        /// <summary>
        /// 缩放或旋转图像时使用的算法
        /// </summary>
        public InterpolationMode InterpoliationMode { get; set; }

        /// <summary>
        /// 缩放或旋转图像时使用的算法
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cropArea">原图待裁剪的矩形选区</param>
        /// <param name="descWidth">裁剪后图像的宽度</param>
        /// <param name="descHeight">裁剪后图像的高度</param>
        public CropFilter(Rectangle cropArea, int descWidth, int descHeight)
        {
            this.CropArea = cropArea;
            this.TargetSize = new Size(descWidth, descHeight);
            this.InterpoliationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
        }


        /// <summary>
        /// 对传入的inputImage进行裁剪
        /// </summary>
        /// <param name="inputImage">待处理的图像文件</param>
        /// <param name="isProcessed">是否被处理，否则返回原图</param>
        /// <returns>返回处理后的图像文件</returns>
        public Image Process(Image inputImage, out bool isProcessed)
        {
            //如果原图宽、高均小于请求裁剪的宽、高，则直接返回原图
            if (this.TargetSize.Height > inputImage.Height && this.TargetSize.Width > inputImage.Width)
            {
                isProcessed = false;
                return inputImage;
            }

            Size imageSize = inputImage.Size;
            Rectangle srcRect = this.CropArea;

            int x2 = srcRect.X + srcRect.Width;
            if (x2 > inputImage.Width)
                srcRect.Width -= (x2 - inputImage.Width);

            int y2 = srcRect.Y + srcRect.Height;
            if (y2 > inputImage.Height)
                srcRect.Height -= (y2 - inputImage.Height);

            Bitmap outputBitmap = new Bitmap(TargetSize.Width, TargetSize.Height);
            using (Graphics g = Graphics.FromImage(outputBitmap))
            {
                g.InterpolationMode = this.InterpoliationMode;
                g.SmoothingMode = this.SmoothingMode;

                Rectangle destRect = new Rectangle(0, 0, TargetSize.Width, TargetSize.Height);
                g.DrawImage(inputImage, destRect, srcRect, GraphicsUnit.Pixel);
            }

            inputImage.Dispose();
            isProcessed = true;

            return outputBitmap;
        }

    }
}
