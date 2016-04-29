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
    /// 用于缩放的图像处理过滤器
    /// </summary>
    public class ResizeFilter : IImageFilter
    {

        #region 属性

        /// <summary>
        /// 期望图像尺寸
        /// </summary>
        public Size TargetSize { get; set; }

        /// <summary>
        /// 图像缩放方式
        /// </summary>
        public ResizeMethod ResizeMethod { get; set; }

        /// <summary>
        /// 矩形选区停靠位置
        /// </summary>
        public AnchorLocation AnchorLocation { get; set; }

        /// <summary>
        /// 缩放或旋转图像时使用的算法
        /// </summary>
        public InterpolationMode InterpoliationMode { get; set; }

        /// <summary>
        /// 缩放或旋转图像时使用的算法
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        #endregion



        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">缩放后的宽度</param>
        /// <param name="height">缩放后的高度</param>
        public ResizeFilter(int width, int height)
            : this(width, height, ResizeMethod.KeepAspectRatio)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">缩放后的宽度</param>
        /// <param name="height">缩放后的高度</param>
        /// <param name="resizeMethod">缩放方式</param>
        public ResizeFilter(int width, int height, ResizeMethod resizeMethod)
            : this(width, height, resizeMethod, AnchorLocation.Middle)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">缩放后的宽度</param>
        /// <param name="height">缩放后的高度</param>
        /// <param name="resizeMethod">缩放方式</param>
        /// <param name="anchorLocation">如需裁剪时，矩形选区停靠位置</param>
        public ResizeFilter(int width, int height, ResizeMethod resizeMethod, AnchorLocation anchorLocation)
        {
            this.TargetSize = new Size(width, height);
            this.ResizeMethod = resizeMethod;
            this.AnchorLocation = anchorLocation;
            this.InterpoliationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
        }

        #endregion


        /// <summary>
        /// 对传入的inputImage进行缩放
        /// </summary>
        /// <param name="inputImage">待处理的图像文件</param>
        /// <param name="isProcessed">是否被处理，否则返回原图</param>
        /// <returns>返回处理后的图像文件</returns>
        public Image Process(Image inputImage, out bool isProcessed)
        {
            //如果原图宽、高均小于请求缩放的宽、高，则直接返回原图            
            if (this.TargetSize.Height > inputImage.Height && this.TargetSize.Width > inputImage.Width)
            {
                isProcessed = false;
                return inputImage;
            }

            Size bitmapSize = Size.Empty;
            Size outputSize = GetNewSize(inputImage, this.TargetSize, this.ResizeMethod, out bitmapSize);

            Bitmap outputBmp = new Bitmap(bitmapSize.Width, bitmapSize.Height);

            using (Graphics g = Graphics.FromImage(outputBmp))
            {
                g.InterpolationMode = this.InterpoliationMode;
                g.SmoothingMode = this.SmoothingMode;

                Rectangle destRect = new Rectangle(new Point(0, 0), outputSize);
                Rectangle sourceRect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

                float outputAspect = (float)outputSize.Width / (float)outputSize.Height;

                if (this.ResizeMethod == ResizeMethod.Crop)
                    sourceRect = GetLargestInset(sourceRect, outputAspect, this.AnchorLocation);

                g.DrawImage(inputImage, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            inputImage.Dispose();
            isProcessed = true;

            return outputBmp;
        }


        /// <summary>
        /// 计算实际缩放后的图像尺寸
        /// </summary>
        /// <param name="img">The image to resize</param>
        /// <param name="requestedSize">请求缩放的图像尺寸</param>
        /// <param name="resizeMethod">图像缩放方式</param>
        /// <param name="bitmapSize">推荐输出的bitmap尺寸</param>
        /// <returns>
        /// 返回实际缩放后的图像尺寸
        /// </returns>
        protected virtual Size GetNewSize(Image img, Size requestedSize, ResizeMethod resizeMethod, out Size bitmapSize)
        {
            Size outputSize = new Size();

            if (img.Width <= requestedSize.Width && img.Height <= requestedSize.Height)
            {
                outputSize.Width = img.Width;
                outputSize.Height = img.Height;
            }
            else
            {
                switch (resizeMethod)
                {
                    case ResizeMethod.KeepAspectRatio:
                        {
                            float imgRatio = (float)img.Width / (float)img.Height;
                            float requestedRatio = (float)requestedSize.Width / (float)requestedSize.Height;

                            if (imgRatio <= requestedRatio)
                            {
                                outputSize.Width = (int)((float)requestedSize.Height * imgRatio);
                                outputSize.Height = requestedSize.Height;
                            }
                            else
                            {
                                outputSize.Width = requestedSize.Width;
                                outputSize.Height = (int)((float)requestedSize.Width / imgRatio);
                            }
                        }
                        break;

                    case ResizeMethod.Absolute:
                    case ResizeMethod.Crop:
                        {
                            outputSize = requestedSize;
                            if (outputSize.Width > img.Width)
                            {
                                outputSize.Width = img.Width;
                            }
                            if (outputSize.Height > img.Height)
                            {
                                outputSize.Height = img.Height;
                            }
                        }
                        break;
                }
            }

            bitmapSize = outputSize;

            return outputSize;
        }


        /// <summary>
        /// 按照期望的宽高比获取sourceRect中最大矩形区域        
        /// </summary>
        /// <param name="sourceRect">源矩形区域</param>
        /// <param name="desiredAspect">期望的宽高比</param>
        /// <param name="anchorLocation">矩形选区停靠位置</param>
        /// <returns>返回在sourceRect内满足desiredAspect的最大矩形区域</returns>
        protected virtual Rectangle GetLargestInset(Rectangle sourceRect, float desiredAspect, AnchorLocation anchorLocation)
        {
            Rectangle destRect = default(Rectangle);

            float sourceAspect = (float)sourceRect.Width / (float)sourceRect.Height;
            float ratioScale = desiredAspect / sourceAspect;

            if (sourceAspect > desiredAspect)
            {
                destRect.Width = (int)((float)sourceRect.Width * ratioScale);
                destRect.Height = sourceRect.Height;
            }
            else
            {
                destRect.Width = sourceRect.Width;
                destRect.Height = (int)((float)sourceRect.Height / ratioScale);
            }

            RectangleUtil.PositionRectangle(anchorLocation, sourceRect, ref destRect);

            return destRect;
        }


    }

    /// <summary>
    /// 图像缩放方式
    /// </summary>
    /// <remarks>
    /// 不执行放大操作
    /// </remarks>
    public enum ResizeMethod
    {
        /// <summary>
        /// 按绝对尺寸缩放
        /// </summary>
        /// <remarks>
        /// 按指定的尺寸进行缩放,不保证宽高比率，可能导致图像失真
        /// </remarks>
        Absolute = 0,

        /// <summary>
        /// 保持原图像宽高比缩放
        /// </summary>
        /// <remarks>
        /// 保持原图像宽高比进行缩放，不超出指定宽高构成的矩形范围
        /// </remarks>
        KeepAspectRatio = 1,

        /// <summary>
        /// 裁剪图像
        /// </summary>
        /// <remarks>
        /// 保持原图像宽高比
        /// </remarks>        
        Crop = 3
    }



}
