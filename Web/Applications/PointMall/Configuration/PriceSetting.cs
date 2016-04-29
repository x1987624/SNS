//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------


using Tunynet;
using System;
using Tunynet.Caching;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城设置
    /// </summary>
    public class PriceSetting
    {
        /// <summary>
        /// 私有数据保存
        /// </summary>
        private static Dictionary<int, int> priceIntervals = new Dictionary<int, int>
        {
            {0,1000},
            {1000,2000},
            {2000,3000}
        };

        /// <summary>
        /// 价格区间
        /// </summary>
        public static Dictionary<int, int> PriceIntervals
        {
            get { return priceIntervals; }
            set { priceIntervals = value; }
        }

        /// <summary>
        /// 禁用初始化方法
        /// </summary>
        private PriceSetting() { }

        /// <summary>
        /// 注册设置
        /// </summary>
        /// <param name="xElement">准备解析的Xml</param>
        public static void RegisterSettings(XElement xElement)
        {
            if (xElement != null)
            {
                IEnumerable<XElement> xElements = xElement.Elements("add");
                if (xElements != null && xElements.Count() > 0)
                {
                    priceIntervals.Clear();
                    foreach (var item in xElements)
                    {
                        int minPrice = 0;
                        int maxPrice = 0;
                        var attrMin = item.Attribute("MinPrice");
                        var attrMax = item.Attribute("MaxPrice");

                        if (attrMin != null && attrMax != null)
                        {
                            if (int.TryParse(attrMin.Value, out minPrice) && int.TryParse(attrMax.Value, out maxPrice))
                                priceIntervals.Add(minPrice, maxPrice);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> Get()
        {
            return priceIntervals;
        }
    }
}