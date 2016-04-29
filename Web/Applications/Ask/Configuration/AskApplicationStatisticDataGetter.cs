//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问题应用数据Url获取器
    /// </summary>
    public class AskQuestionApplicationStatisticDataGetter : IApplicationStatisticDataGetter
    {
        private AskService askService = new AskService();

        /// <summary>
        /// 获取问题管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>管理数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetManageableDatas(string tenantTypeId = null)
        {
            List<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> questionManageableDatas = askService.GetQuestionManageableData(tenantTypeId);
            Dictionary<string, long> answerManageableDatas = askService.GetAnswerManageableData();

            #region 问题
            if (questionManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "问题",
                 "问题待审核数", questionManageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个问题待审核",
                    Url = SiteUrls.Instance().AskQuestionControlPanelManage(auditStatus: AuditStatus.Pending)
                });
            if (questionManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().AgainCount(), "问题",
                 "问题需再审核数", questionManageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个问题需再审核",
                    Url = SiteUrls.Instance().AskQuestionControlPanelManage(auditStatus: AuditStatus.Again)
                });
            #endregion

            #region 回答
            if (answerManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "回答",
                 "回答待审核数", answerManageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个回答待审核",
                    Url = SiteUrls.Instance().AskAnswerControlPanelManage(auditStatus: AuditStatus.Pending)
                });
            if (answerManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().AgainCount(), "回答",
                 "回答需再审核数", answerManageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个回答需再审核",
                    Url = SiteUrls.Instance().AskAnswerControlPanelManage(auditStatus: AuditStatus.Again)
                });
            #endregion

            return applicationStatisticDatas;
        }

        /// <summary>
        /// 获取问题统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>统计数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetStatisticDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> questionStatisticDatas = askService.GetQuestionApplicationStatisticData(tenantTypeId);
            Dictionary<string, long> answerStatisticDatas = askService.GetAnswerApplicationStatisticData();

            #region 问题
            if (questionStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "问题",
                 "问题总数", questionStatisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个问题",

                    Url = SiteUrls.Instance().AskQuestionControlPanelManage()
                });
            if (questionStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "问题",
                 "问题24小时新增数", questionStatisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个问题",
                    Url = SiteUrls.Instance().AskQuestionControlPanelManage()
                });
            #endregion

            #region 回答
            if (answerStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "回答",
                 "回答总数", answerStatisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个回答",
                    Url = SiteUrls.Instance().AskAnswerControlPanelManage()
                });
            if (answerStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "回答",
                 "回答24小时新增数", answerStatisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个回答",
                    Url = SiteUrls.Instance().AskAnswerControlPanelManage()
                });
            #endregion

            return applicationStatisticDatas;
        }
    }
}