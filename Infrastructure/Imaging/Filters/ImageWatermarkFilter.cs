//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-22</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-22" version="0.5">创建</log>
//<log date="2012-02-26" version="0.51" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Tunynet.Imaging
{
    /// <summary>
    /// 图像水印过滤器
    /// </summary>
    public class ImageWatermarkFilter : WatermarkFilterBase
    {
        /// <summary>
        /// 作为水印的图像文件
        /// </summary>
        public Image WatermarkImage { get; private set; }

        /// <summary>
        /// 作为水印的图像文件物理路径
        /// </summary>
        public string WatermarkImagePhysicalPath { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="watermarkImagePhysicalPath">作为水印的图像文件物理路径</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        public ImageWatermarkFilter(string watermarkImagePhysicalPath, AnchorLocation anchorLocation)
            : this(watermarkImagePhysicalPath, anchorLocation, 0.6F)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="watermarkImage">作为水印的图像文件</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        public ImageWatermarkFilter(Image watermarkImage, AnchorLocation anchorLocation)
            : this(watermarkImage, anchorLocation, 0.6F)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="watermarkImagePhysicalPath">作为水印的图像文件物理路径</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        /// <param name="opacity">不透明度</param>
        public ImageWatermarkFilter(string watermarkImagePhysicalPath, AnchorLocation anchorLocation, float opacity)
        {
            this.WatermarkImagePhysicalPath = watermarkImagePhysicalPath;
            base.AnchorLocation = anchorLocation;
            base.Opacity = opacity;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="watermarkImage">作为水印的图像文件</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        /// <param name="opacity">不透明度</param>
        public ImageWatermarkFilter(Image watermarkImage, AnchorLocation anchorLocation, float opacity)
        {
            this.WatermarkImage = watermarkImage;
            base.AnchorLocation = anchorLocation;
            base.Opacity = opacity;
        }

        /// <summary>
        /// 在传入的inputImage添加图像水印
        /// </summary>
        /// <param name="inputImage">待处理的图像文件</param>
        /// <param name="isProcessed">是否被处理，否则返回原图</param>
        /// <returns>返回处理后的图像文件</returns>
        public override Image Process(Image inputImage, out bool isProcessed)
        {
            if (WatermarkImage == null && string.IsNullOrEmpty(WatermarkImagePhysicalPath))
            {
                isProcessed = false;
                return inputImage;
            }

            Image watermarkImage;
            if (WatermarkImage != null)
                watermarkImage = WatermarkImage;
            else
                watermarkImage = Image.FromFile(WatermarkImagePhysicalPath);

            Image outputImage;
            Graphics g;

            //如果图片格式不支持添加水印则直接返回原图像文件
            if (IsPixelFormatIndexed(inputImage.PixelFormat))
            {
                //使用临时 GDI+ 位图
                Bitmap tempImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);
                g = Graphics.FromImage(tempImage);
                g.DrawImage(inputImage, 0, 0);
                outputImage = tempImage;
            }
            else
            {
                g = Graphics.FromImage(inputImage);
                outputImage = inputImage;
            }

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle watermarkArea = GetWatermarkArea(inputImage, watermarkImage);
            ImageAttributes imageAttr = BuildImageAttributes();

            g.DrawImage(watermarkImage, watermarkArea, 0, 0, watermarkImage.Width, watermarkImage.Height, GraphicsUnit.Pixel, imageAttr);

            isProcessed = true;
            return outputImage;
        }

        /// <summary>
        /// 获取水印区域
        /// </summary>
        /// <param name="inputImage">待加水印的图像</param>
        /// <param name="watermarkImage">用为水印的图像</param>
        /// <returns>返回水印矩形区域</returns>
        private Rectangle GetWatermarkArea(Image inputImage, Image watermarkImage)
        {
            Rectangle imageArea = new Rectangle(Point.Empty, inputImage.Size);
            Rectangle watermarkArea = new Rectangle(Point.Empty, watermarkImage.Size);
            RectangleUtil.PositionRectangle(this.AnchorLocation, imageArea, ref watermarkArea);

            //为边缘留出偏移量
            int offsetX = (int)((float)inputImage.Width * (float).01);
            int offsetY = (int)((float)inputImage.Height * (float).01);

            switch (AnchorLocation)
            {
                case AnchorLocation.LeftTop:
                    watermarkArea.Offset(offsetX, offsetY);
                    break;
                case AnchorLocation.MiddleTop:
                    watermarkArea.Offset(0, offsetY);
                    break;
                case AnchorLocation.RightTop:
                    watermarkArea.Offset(-offsetX, offsetY);
                    break;
                case AnchorLocation.LeftMiddle:
                    watermarkArea.Offset(offsetX, 0);
                    break;
                case AnchorLocation.Middle:
                    break;
                case AnchorLocation.RightMiddle:
                    watermarkArea.Offset(-offsetX, 0);
                    break;
                case AnchorLocation.LeftBottom:
                    watermarkArea.Offset(offsetX, -offsetY);
                    break;
                case AnchorLocation.MiddleBottom:
                    watermarkArea.Offset(0, -offsetY);
                    break;
                case AnchorLocation.RightBottom:
                    watermarkArea.Offset(-offsetX, -offsetY);
                    break;
            }
            return watermarkArea;
        }


        /// <summary>
        /// Builds the image attributes.
        /// </summary>
        private ImageAttributes BuildImageAttributes()
        {
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = this.Opacity;

            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            return imageAttr;
        }


    }
}
