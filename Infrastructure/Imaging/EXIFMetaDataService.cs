//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-21</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-21" version="0.5">创建</log>
//<log date="2012-02-21" version="0.6" author="libsh">Add the concrete realization of the read method</log>
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
    /// 照片信息处理
    /// </summary>
    public class EXIFMetaDataService
    {
        /// <summary>
        /// 读取图像的EXIF信息
        /// </summary>
        /// <param name="imageStream">图像文件流</param>
        /// <returns>
        /// 返回读取的EXIF信息字典（key=PropertyItem.Id）
        /// </returns>
        public Dictionary<int, string> Read(Stream imageStream)
        {
            if ((imageStream == null) || (!imageStream.CanRead))
                throw new ArgumentException("imageStream isn't validate", "imageStream");

            Image image = Image.FromStream(imageStream);
            Dictionary<int, string> properties = null;

            if (image.PropertyItems != null)
            {
                properties = new Dictionary<int, string>();
                foreach (var propId in EnabledEXIFIds)
                {
                    if (image.PropertyIdList.Contains(propId))
                    {
                        properties[propId] = GetValueOfType(image.GetPropertyItem(propId));
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// 读取图像的EXIF信息
        /// </summary>
        /// <param name="imageStream">图像文件流</param>
        /// <param name="propId">16进制元数据Id</param>
        /// <returns>
        /// 返回根据propId读取的EXIF信息
        /// </returns>
        public string Read(Stream imageStream, int propId)
        {
            if ((imageStream == null) || (!imageStream.CanRead))
                throw new ArgumentException("imageStream isn't validate", "imageStream");

            Image image = Image.FromStream(imageStream);
            string prop = string.Empty;

            if (image.PropertyItems != null)
            {
                prop = GetValueOfType(image.GetPropertyItem(propId));
            }

            return prop;
        }


        //启用的EXIF Id集合
        //ExifId参考：http://msdn.microsoft.com/zh-cn/library/ms534416%28vs.85%29.html
        private int[] EnabledEXIFIds = new int[] { 0x010F /* 设备制造厂商 */, 0x0110/* 相机型号 */, 0x9003/* 拍摄日期 */, 
                                                   0xa002/* 宽度 */, 0xa003/* 高度 */, 0x9205/* 最大光圈 */, 0x920a /* 焦距 */, 
                                                   0x829A/* 曝光时间 */, 0x8824/* 感光度 */,0x9209/* 闪光灯 */ };

        #region 获取图像元数据的值

        /// <summary>
        /// 以元数据值的对应类型来获取值
        /// </summary>
        /// <param name="propItem">要获取信息的图像文件元数据属性</param>
        /// <returns></returns>
        private static string GetValueOfType(PropertyItem propItem)
        {
            switch (propItem.Type)
            {
                case 1:
                    return GetValueOfType1(propItem.Value);
                case 2:
                    return GetValueOfType2(propItem.Value);
                case 3:
                    return GetValueOfType3(propItem.Value);
                case 4:
                    return GetValueOfType4(propItem.Value);
                case 5:
                    return GetValueOfType5(propItem.Value);
                case 7:
                    return GetValueOfType7(propItem.Value, propItem.Id);
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取字节值
        /// </summary>
        /// <param name="value">元数据的值</param>
        /// <returns></returns>
        private static string GetValueOfType1(byte[] value)
        {
            return System.Text.Encoding.ASCII.GetString(value);
        }

        /// <summary>
        /// 获取空终止 ASCII 字符串值
        /// </summary>
        /// <param name="value">元数据的值</param>
        /// <returns></returns>
        private static string GetValueOfType2(byte[] value)
        {
            return System.Text.Encoding.ASCII.GetString(value);
        }

        /// <summary>
        /// 获取无符号的16 位整型值
        /// </summary>
        /// <param name="value">元数据的值</param>
        /// <returns></returns>
        private static string GetValueOfType3(byte[] value)
        {
            if (value.Length != 2)
                return string.Empty;

            return Convert.ToUInt16(value[1] << 8 | value[0]).ToString();
        }

        /// <summary>
        ///获取无符号的32 位整型值
        /// </summary>
        /// <param name="value">元数据的值</param>
        /// <returns></returns>
        private static string GetValueOfType4(byte[] value)
        {
            if (value.Length != 4)
                return string.Empty;

            return Convert.ToUInt32(value[3] << 24 | value[2] << 16 | value[1] << 8 | value[0]).ToString();
        }

        /// <summary>
        /// 获取无符号的32位整型对数值
        /// </summary>
        /// <remarks>
        /// 每一对都表示一个分数；第一个整数是分子，第二个整数是分母
        /// </remarks>
        /// <param name="value">元数据的值</param>
        /// <returns></returns>
        private static string GetValueOfType5(byte[] value)
        {
            if (value.Length != 8)
                return string.Empty;

            UInt32 denominator = 0, molecular = 0;

            denominator = Convert.ToUInt32(value[7] << 24 | value[6] << 16 | value[5] << 8 | value[4]);
            molecular = Convert.ToUInt32(value[3] << 24 | value[2] << 16 | value[1] << 8 | value[0]);

            return molecular.ToString() + "/" + denominator.ToString();
        }

        /// <summary>
        /// 获取有符号的32 位整型数值
        /// </summary>
        /// <param name="value">元数据的值</param>
        /// <param name="propId">元数据Id</param>
        /// <returns></returns>
        private static string GetValueOfType7(byte[] value, int propId)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                if (propId == 0x9000 || propId == 0xA000)
                {
                    sb.Append(((char)value[i]).ToString());
                }
                else
                {
                    sb.Append(value[i].ToString());
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
