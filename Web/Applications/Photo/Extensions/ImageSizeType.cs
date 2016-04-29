//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 图片尺寸类型
    /// </summary>
    public static class ImageSizeTypeExtension
    {
        /// <summary>
        /// 100*100像素等比例缩放，主要用于封面缩略图
        /// </summary>
        public static string P100(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P100";
        }

        /// <summary>
        /// 160*160像素等比例缩放，主要用于封面缩略图
        /// </summary>
        public static string P160(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P160";
        }

        /// <summary>
        /// 200*200像素裁剪方图（先等比例缩放再裁剪），主要用于封面缩略图
        /// </summary>
        public static string P200(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P200";
        }

        /// <summary>
        /// 240像素等宽等比例缩放（即按照宽度缩放，不按照高度缩放），主要用于瀑布流
        /// </summary>
        public static string P240(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P240";
        }

        /// <summary>
        /// 320像素等宽等比例缩放（即按照宽度缩放，不按照高度缩放），主要用于瀑布流的鼠标悬浮显示
        /// </summary>
        public static string P320(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P320";
        }

        /// <summary>
        /// 800像素等宽等比例缩放（即按照宽度缩放，不按照高度缩放），主要用于相册阅读模式及照片详情页
        /// </summary>
        public static string P800(this ImageSizeTypeKeys auditItemKeys)
        {
            return "P800";
        }

    }

}
