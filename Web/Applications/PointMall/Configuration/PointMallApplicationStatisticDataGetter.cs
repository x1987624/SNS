//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet.Common;
using Tunynet;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 商城应用数据获取器
    /// </summary>
    public class PointMallApplicationStatisticDataGetter : IApplicationStatisticDataGetter
    {
        private PointMallService pointMallService = new PointMallService();

        /// <summary>
        /// 获取商城管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>管理数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetManageableDatas(string tenantTypeId = null)
        {
            List<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> recordManageableData = pointMallService.GetRecordManageableData();

            if (recordManageableData.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
            {
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "积分商城",
                 "申请待审批数", recordManageableData[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个兑换申请待批准",
                    Url = SiteUrls.Instance().ManageRecords(null,null,null,ApproveStatus.Pending)
                });
            }

            return applicationStatisticDatas;
        }

        /// <summary>
        /// 获取商城统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>统计数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetStatisticDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> recordApplicationStatisticData = pointMallService.GetRecordApplicationStatisticData();
            if (recordApplicationStatisticData.ContainsKey("TotalCountGifts"))
            {
                applicationStatisticDatas.Add(new ApplicationStatisticData("TotalCountGifts", "积分商城",
                 "总商品数", recordApplicationStatisticData["TotalCountGifts"])
                {
                    DescriptionPattern = "总商品数:{0}",
                    Url = SiteUrls.Instance().ManageRecords()
                });
            }

            if (recordApplicationStatisticData.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
            {
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "积分商城",
                 "24小时新增申请数", recordApplicationStatisticData[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个申请",
                    Url = SiteUrls.Instance().ManageRecords()
                });
            }

            if (recordApplicationStatisticData.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
            {
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "积分商城",
                 "总申请数", recordApplicationStatisticData[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个商品申请",

                    Url = SiteUrls.Instance().ManageRecords()
                });
            }

            return applicationStatisticDatas;
        }

    }

}