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
using System.Drawing.Imaging;

namespace Tunynet.Imaging
{
    /// <summary>
    /// 文字水印过滤器
    /// </summary>
    public class TextWatermarkFilter : WatermarkFilterBase
    {
        /// <summary>
        /// 水印文字
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="text">水印文字</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        public TextWatermarkFilter(string text, AnchorLocation anchorLocation)
            : this(text, anchorLocation, 0.6F)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="text">水印文字</param>
        /// <param name="anchorLocation">水印在图像上的停靠位置 </param>
        /// <param name="opacity">不透明度</param>
        public TextWatermarkFilter(string text, AnchorLocation anchorLocation, float opacity)
        {
            this.Text = text;
            base.AnchorLocation = anchorLocation;
            base.Opacity = opacity;
        }

        /// <summary>
        /// 在传入的inputImage添加文字水印
        /// </summary>
        /// <param name="inputImage">待处理的图像文件</param>
        /// <param name="isProcessed">是否被处理，否则返回原图</param>
        /// <returns>返回处理后的图像文件</returns>
        public override Image Process(Image inputImage, out bool isProcessed)
        {
            Image outputImage;
            Graphics g;           

            //如果图片带有索引像素格式,不能直接通过原图创建 Graphics 对象
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
            g.InterpolationMode = InterpolationMode.High;

            Font crFont;
            Rectangle watermarkArea = GetWatermarkArea(g, inputImage, out crFont);

            StringFormat stringFormat = new StringFormat();
            //stringFormat.Alignment = StringAlignment.Near;

            //SolidBrush:定义单色画笔。画笔用于填充图形形状，如矩形、椭圆、扇形、多边形和封闭路径。
            //这个画笔为描绘阴影的画笔，呈灰色
            int m_alpha = Convert.ToInt32(256F * this.Opacity);
            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(m_alpha, 0, 0, 0));

            //描绘文字信息，这个图层向右和向下偏移一个像素，表示阴影效果
            //DrawString 在指定矩形并且用指定的 Brush 和 Font 对象绘制指定的文本字符串
            g.DrawString(Text, crFont, semiTransBrush2, watermarkArea.X + 1F, watermarkArea.Y + 1F, stringFormat);

            //从四个 ARGB 分量（alpha、红色、绿色和蓝色）值创建 Color 结构，这里设置透明度为153
            //这个画笔为描绘正式文字的笔刷，呈白色
            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));
            g.DrawString(Text, crFont, semiTransBrush, watermarkArea.X, watermarkArea.Y, stringFormat);

            semiTransBrush2.Dispose();
            semiTransBrush.Dispose();

            isProcessed = true;
            return outputImage;
        }

        /// <summary>
        /// 获取水印区域
        /// </summary>
        /// <param name="graphics">画布</param>
        /// <param name="inputImage">图像</param>
        /// <param name="watermarkFont">水印文字使用的字体</param>
        /// <returns>返回水印矩形区域</returns>
        protected virtual Rectangle GetWatermarkArea(Graphics graphics, Image inputImage, out Font watermarkFont)
        {
            //根据图片的大小我们来确定添加上去的文字的大小
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4, 3, 2, 1 };

            //字体
            watermarkFont = null;
            Size watermarkSize = Size.Empty;

            //利用一个循环语句来选择我们要添加文字的型号
            //直到它的长度比图片的宽度小
            for (int i = 0; i < sizes.Length; i++)
            {
                watermarkFont = new Font("arial", sizes[i], FontStyle.Bold);

                //测量用指定的 Font 对象绘制并用指定的 StringFormat 对象格式化的指定字符串。
                watermarkSize = graphics.MeasureString(Text, watermarkFont).ToSize();

                if (watermarkSize.Width < (inputImage.Width * 0.8))
                    break;
            }

            Rectangle imageArea = new Rectangle(Point.Empty, inputImage.Size);
            Rectangle watermarkArea = new Rectangle(Point.Empty, watermarkSize);
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


    }
}
