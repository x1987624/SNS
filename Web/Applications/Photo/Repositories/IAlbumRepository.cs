//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的相册仓储接口
    /// </summary>
    public interface IAlbumRepository : IRepository<Album>
    {
        /// <summary>
        /// 删除用户时，指定用户接管当前用户的相册以及相册下照片
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="takeOverUser"></param>
        void TakeOver(long userId, User takeOverUser);

        /// <summary>
        /// 获取相册列表（用于频道）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        PagingDataSet<Album> GetAlbums(string tenantTypeId, long? ownerId, SortBy_Album sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 获取相册列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">用户ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        PagingDataSet<Album> GetUserAlbums(string tenantTypeId, long userId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 获取相册列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        PagingDataSet<Album> GetOwnerAlbums(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 管理员获取相册数据
        /// </summary>
        /// <remarks>无需缓存</remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="nameKeyword">相册名称</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        PagingDataSet<Album> GetAlbumsForAdmin(string tenantTypeId, string nameKeyword, long? userId, long? ownerId, AuditStatus? auditStatus, int pageSize, int pageIndex);

        /// <summary>
        /// 获取相册统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>返回相册统计数据</returns>
        Dictionary<string, long> GetAlbumApplicationStatisticData(string tenantTypeId);

        /// <summary>
        /// 获取相册管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>返回相册管理数据</returns>
        Dictionary<string, long> GetAlbumManageableData(string tenantTypeId);
    }
}