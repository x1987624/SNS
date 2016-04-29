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

namespace Tunynet.Imaging
{
    /// <summary>
    /// 矩形工具类
    /// </summary>
    internal sealed class RectangleUtil
    {

        /// <summary>
        /// 按照停靠位置定位矩形选区（destRect）在矩形容器（sourceRect）中的位置
        /// </summary>
        /// <param name="anchorLocation">矩形选区停靠位置</param>
        /// <param name="sourceRect">矩形容器</param>
        /// <param name="destRect">矩形选区</param>
        public static void PositionRectangle(AnchorLocation anchorLocation, Rectangle sourceRect, ref Rectangle destRect)
        {
            // Position the rectangle based on the anchor location
            switch (anchorLocation)
            {
                // Top -------------------------

                case AnchorLocation.LeftTop:
                    destRect.X = destRect.Y = 0;
                    break;

                case AnchorLocation.MiddleTop:
                    destRect.X = (sourceRect.Width - destRect.Width) / 2;
                    destRect.Y = 0;
                    break;

                case AnchorLocation.RightTop:
                    destRect.X = sourceRect.Width - destRect.Width;
                    destRect.Y = 0;
                    break;


                // Middle -------------------------

                case AnchorLocation.LeftMiddle:
                    destRect.X = 0;
                    destRect.Y = (sourceRect.Height - destRect.Height) / 2;
                    break;

                case AnchorLocation.Middle:
                    destRect.X = (sourceRect.Width - destRect.Width) / 2;
                    destRect.Y = (sourceRect.Height - destRect.Height) / 2;
                    break;

                case AnchorLocation.RightMiddle:
                    destRect.X = sourceRect.Width - destRect.Width;
                    destRect.Y = (sourceRect.Height - destRect.Height) / 2;
                    break;


                // Bottom

                case AnchorLocation.LeftBottom:
                    destRect.X = 0;
                    destRect.Y = sourceRect.Height - destRect.Height;
                    break;

                case AnchorLocation.MiddleBottom:
                    destRect.X = (sourceRect.Width - destRect.Width) / 2;
                    destRect.Y = sourceRect.Height - destRect.Height;
                    break;

                case AnchorLocation.RightBottom:
                    destRect.X = sourceRect.Width - destRect.Width;
                    destRect.Y = sourceRect.Height - destRect.Height;
                    break;
            }
        }


    }
}
