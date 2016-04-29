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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tunynet.Imaging
{
    /// <summary>
    /// 图像处理器
    /// </summary>
    /// <remarks>
    /// <para>注意事项：</para>
    /// <list type="bullet">
    ///     <item>暂不对GIF动画做任何处理（以后计划仅对GIF动画进行缩放，可参考：www.codeplex.com/GifLib）</item>
    /// </list>
    /// </remarks>
    public class ImageProcessor
    {

        private List<IImageFilter> _filters = null;
        /// <summary>
        /// 图像处理过滤器列表
        /// </summary>
        public List<IImageFilter> Filters
        {
            get
            {
                if (_filters == null)
                    _filters = new List<IImageFilter>();
                return _filters;
            }
        }

        /// <summary>
        /// 根据ImageSettings对图片进行 缩放/剪切/水印 等操作
        /// </summary>
        /// <param name="inputStream">图像文件流</param>
        public Stream Process(Stream inputStream)
        {
            if ((inputStream == null) || (!inputStream.CanRead))
                throw new ArgumentException("inputStream isn't validate", "inputStream");

            inputStream.Seek(0, SeekOrigin.Begin);
            bool isProcessed = false;

            Image image = Image.FromStream(inputStream);
            ImageFormat imageFormat = image.RawFormat;

            //GIF动画不做任何处理
            if (IsGIFAnimation(image))
            {
                inputStream.Seek(0, SeekOrigin.Begin);
                return inputStream;
            }

            //使用图像处理过滤器处理图片
            foreach (var imageFilter in Filters)
            {
                bool processedInFilter;
                image = imageFilter.Process(image, out processedInFilter);
                if (processedInFilter)
                {
                    isProcessed = true;
                }
            }

            //如果未经处理，直接返回输入流
            if (!isProcessed)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
                return inputStream;
            }

            MemoryStream outputStream = new MemoryStream();

            //对于gif格式，保存为jpeg
            if (imageFormat.Guid == ImageFormat.Gif.Guid)
            {
                image.Save(outputStream, ImageFormat.Jpeg);
            }
            else
            {
                EncoderParameters codecParams = new EncoderParameters(1);
                codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);

                ImageCodecInfo codecInfo = GetImageCodecInfo(imageFormat);
                image.Save(outputStream, codecInfo, codecParams);
                codecParams.Dispose();
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        /// <summary>
        /// Jpeg压缩质量
        /// </summary>
        /// <remarks>取值区间0到100</remarks>
        public int JpegQuality
        {
            get { return jpegQuality; }
            set
            {
                if (value > 0 && value <= 100)
                    jpegQuality = value;
            }
        }
        private int jpegQuality = 92;


        #region 快捷操作

        /// <summary>
        /// 缩放图像
        /// </summary>
        /// <param name="inputStream">图像文件流</param>
        /// <param name="width">缩放后的宽度</param>
        /// <param name="height">缩放后的高度</param>
        /// <param name="resizeMethod">缩放方式</param>
        /// <returns>返回缩放后的图像文件流</returns>
        public static Stream Resize(Stream inputStream, int width, int height, ResizeMethod resizeMethod)
        {
            ImageProcessor imageProcessor = new ImageProcessor();
            ResizeFilter resizeFilter = new ResizeFilter(width, height, resizeMethod);
            imageProcessor.Filters.Add(resizeFilter);

            return imageProcessor.Process(inputStream);
        }

        /// <summary>
        /// 裁剪图像
        /// </summary>
        /// <param name="inputStream">图像文件流</param>
        /// <param name="cropArea">原图待裁剪的矩形选区</param>
        /// <param name="descWidth">裁剪后图像的宽度</param>
        /// <param name="descHeight">裁剪后图像的高度</param>
        /// <returns>返回裁剪后的图像文件流</returns>
        public static Stream Crop(Stream inputStream, Rectangle cropArea, int descWidth, int descHeight)
        {
            ImageProcessor imageProcessor = new ImageProcessor();
            CropFilter cropFilter = new CropFilter(cropArea, descWidth, descHeight);
            imageProcessor.Filters.Add(cropFilter);
            return imageProcessor.Process(inputStream);
        }

        #endregion


        #region Help Methods

        /// <summary>
        /// Gets ImageCodecInfo for the specified ImageFormat
        /// </summary>
        /// <param name="imageFormat">The ImageFormat of the picture.</param>
        /// <returns>System.Drawing.Imaging.ImageCodecInfo</returns>
        private static ImageCodecInfo GetImageCodecInfo(ImageFormat imageFormat)
        {
            ImageCodecInfo[] imageCodecInfos = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo imageCodecInfo in imageCodecInfos)
            {
                if (imageCodecInfo.FormatID == imageFormat.Guid)
                    return imageCodecInfo;
            }
            return null;
        }

        /// <summary>
        /// 图像是否GIF动画
        /// </summary>
        /// <param name="image">待检测的图像</param>
        /// <returns>是GIF动画返回true，否则返回false</returns>
        public static bool IsGIFAnimation(Image image)
        {
            //首先判断图片是否是GIF动画，如果是动画则不对图片进行改动
            foreach (Guid guid in image.FrameDimensionsList)
            {
                FrameDimension dimension = new FrameDimension(guid);
                if (image.GetFrameCount(dimension) > 1)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

    }
}
