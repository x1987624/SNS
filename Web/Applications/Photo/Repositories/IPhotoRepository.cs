//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的照片仓储接口
    /// </summary>
    public interface IPhotoRepository : IRepository<Photo>
    {
        /// <summary>
        /// 移动相片
        /// </summary>
        /// <param name="photo">移动相片</param>
        /// <param name="targetAlbum">接收的相册</param>
        /// <returns>移动是否成功</returns>
        bool MovePhoto(Photo photo, Album targetAlbum);

        /// <summary>
        /// 相册隐私状态更改，同步更改相册下隐私状态
        /// </summary>
        /// <param name="albumId"></param>
        void UpdatePrivacyStatus(long albumId);

        /// <summary>
        /// 设置精华
        /// </summary>
        /// <param name="photo">照片对象</param>
        /// <param name="isEssential">是否精华</param>
        void SetEssential(long photoId, bool isEssential);

        /// <summary>
        /// 获取照片列表（用于频道页面）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="tagName">标签</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        PagingDataSet<Photo> GetPhotos(string tenantTypeId, string tagName, bool? isEssential, SortBy_Photo sortBy, DateTime? createDateTime, int pageSize, int pageIndex);

        /// <summary>
        /// 获取用户照片列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        PagingDataSet<Photo> GetUserPhotos(string tenantTypeId, long? userId, bool ignoreAuditAndPrivacy, string tagName, long? albumId, bool? isEssential, SortBy_Photo sortBy, DateTime? createDateTime ,int pageSize, int pageIndex);

        /// <summary>
        /// 获取所有者照片列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">拥有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        PagingDataSet<Photo> GetOwnerPhotos(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, string tagName, long? albumId, bool? isEssential, SortBy_Photo sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 管理员获取照片数据
        /// </summary>
        /// <param name="tenantTypeIds">租户类型ID</param>
        /// <param name="descriptionKeyword">照片描述</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回管理员分页照片数据</returns>
        PagingDataSet<Photo> GetPhotosForAdmin(string tenantTypeIds, string descriptionKeyword, long? userId, long? ownerId, bool? isEssential, AuditStatus? auditStatus, int pageSize, int pageIndex);

        /// <summary>
        /// 照片统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回照片统计数据</returns>
        Dictionary<string, long> GetPhotoApplicationStatisticData(string tenantTypeId);

        /// <summary>
        /// 照片管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回照片管理数据</returns>
        Dictionary<string, long> GetPhotoManageableData(string tenantTypeId);

        /// <summary>
        /// 获取用户关注照片
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        PagingDataSet<Photo> GetPhotosOfFollowedUsers(long userId, int pageSize, int pageIndex);
   }
}