//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用数据获取器
    /// </summary>
    public class PhotoApplicationStatisticDataGetter : IApplicationStatisticDataGetter
    {
        private PhotoService photoService = new PhotoService();

        #region 获取相册管理数据

        /// <summary>
        /// 获取相册管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>管理数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetManageableDatas(string tenantTypeId = null)
        {
            List<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> albumManageableDatas = photoService.GetAlbumManageableData(tenantTypeId);
            Dictionary<string, long> photoManageableDatas = photoService.GetPhotoManageableData();

            #region 相册
            if (albumManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "相册",
                 "相册待审核数", albumManageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个相册待审核",
                    Url = SiteUrls.Instance().AlbumControlPanelManage(auditStatus: AuditStatus.Pending)
                });
            if (albumManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().AgainCount(), "相册",
                 "相册需再审核数", albumManageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个相册需再审核",
                    Url = SiteUrls.Instance().AlbumControlPanelManage(auditStatus: AuditStatus.Again)
                });
            #endregion

            #region 照片
            if (photoManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().PendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().PendingCount(), "照片",
                 "照片待审核数", photoManageableDatas[ApplicationStatisticDataKeys.Instance().PendingCount()])
                {
                    DescriptionPattern = "{0}个照片待审核",
                    Url = SiteUrls.Instance().PhotoControlPanelManage(auditStatus: AuditStatus.Pending)
                });
            if (photoManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().AgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().AgainCount(), "照片",
                 "照片需再审核数", photoManageableDatas[ApplicationStatisticDataKeys.Instance().AgainCount()])
                {
                    DescriptionPattern = "{0}个照片需再审核",
                    Url = SiteUrls.Instance().PhotoControlPanelManage(auditStatus: AuditStatus.Again)
                });
            #endregion

            return applicationStatisticDatas;
        }

        /// <summary>
        /// 获取相册统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>统计数据列表</returns>
        public IEnumerable<ApplicationStatisticData> GetStatisticDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> albumStatisticDatas = photoService.GetAlbumApplicationStatisticData(tenantTypeId);
            Dictionary<string, long> photoStatisticDatas = photoService.GetPhotoApplicationStatisticData();

            #region 相册
            if (albumStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "相册",
                 "相册总数", albumStatisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个相册",
                    Url = SiteUrls.Instance().AlbumControlPanelManage()
                });
            if (albumStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "相册",
                 "相册24小时新增数", albumStatisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个相册",
                    Url = SiteUrls.Instance().AlbumControlPanelManage()
                });
            #endregion

            #region 照片
            if (photoStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "照片",
                 "照片总数", photoStatisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个照片",
                    Url = SiteUrls.Instance().PhotoControlPanelManage()
                });
            if (photoStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "照片",
                 "照片24小时新增数", photoStatisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个照片",
                    Url = SiteUrls.Instance().PhotoControlPanelManage()
                });
            #endregion

            return applicationStatisticDatas;
        }
        #endregion
    }

}