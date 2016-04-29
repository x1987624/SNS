//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条应用数据Url获取器
    /// </summary>
    public class WikiApplicationStatisticDataGetter : IApplicationStatisticDataGetter
    {
        private WikiService wikiService = new WikiService();

        /// <summary>
        /// 获取管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>管理数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetManageableDatas(string tenantTypeId = null)
        {
            List<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> manageableDatas = wikiService.GetManageableDatas(tenantTypeId);

            if (manageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "词条",
                 "词条待审核数", manageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个词条待审核",
                    Url = SiteUrls.Instance().WikiPageControlPanelManage(auditStatus: AuditStatus.Pending)
                });
            if (manageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().AgainCount(), "词条",
                 "词条需再审核数", manageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个词条需再审核",
                    Url = SiteUrls.Instance().WikiPageControlPanelManage(auditStatus: AuditStatus.Again)
                });

            //添加词条版本统计
            Dictionary<string, long> manageableDatasForWikiVersion = wikiService.GetManageableDatasForWikiVersion(tenantTypeId);
            if (manageableDatasForWikiVersion.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "词条版本",
                "词条版本待审核数", manageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个词条版本待审核",
                    Url = SiteUrls.Instance().ManageVersion(auditStatus: AuditStatus.Pending)
                });

            if (manageableDatasForWikiVersion.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "词条版本",
                "词条版本再审核数", manageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个词条版本再审核",
                    Url = SiteUrls.Instance().ManageVersion(auditStatus: AuditStatus.Again)
                });

            return applicationStatisticDatas;
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>统计数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetStatisticDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> statisticDatas = wikiService.GetStatisticDatas(tenantTypeId);
            if (statisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "词条",
                 "词条总数", statisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个词条",
                    Url = SiteUrls.Instance().WikiPageControlPanelManage()
                });
            if (statisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "词条",
                 "词条24小时新增数", statisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个词条",
                    Url = SiteUrls.Instance().WikiPageControlPanelManage()
                });
            return applicationStatisticDatas;
        }
    }
}