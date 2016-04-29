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

namespace Tunynet.Imaging
{
    /// <summary>
    /// 矩形选区停靠位置
    /// </summary>
    public enum AnchorLocation
    {
        /// <summary>
        /// 左上
        /// </summary>
        LeftTop = 0,

        /// <summary>
        /// 中上
        /// </summary>
        MiddleTop = 1,

        /// <summary>
        /// 右上
        /// </summary>
        RightTop = 2,

        /// <summary>
        /// 左中部
        /// </summary>
        LeftMiddle = 3,

        /// <summary>
        /// 居中
        /// </summary>
        Middle = 4,

        /// <summary>
        /// 右中部
        /// </summary>
        RightMiddle = 5,

        /// <summary>
        /// 左下部
        /// </summary>
        LeftBottom = 6,

        /// <summary>
        /// 中下部
        /// </summary>
        MiddleBottom = 7,

        /// <summary>
        /// 右下
        /// </summary>
        RightBottom = 8
    }
}
