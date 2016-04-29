using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的圈人仓储实现
    /// </summary>
    public class PhotoLabelRepository : Repository<PhotoLabel>, IPhotoLabelRepository
    {
        /// <summary>
        /// 获取圈人对象列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="photoId">照片ID</param>
        /// <returns>返回圈人列表</returns>
        public IEnumerable<PhotoLabel> GetLabelsOfPhoto(long photoId, string tenantTypeId)
        {
            return GetTopEntities(PrimaryMaxRecords, CachingExpirationType.ObjectCollection, () =>
              {
                  string cc = string.Format("GetLabelsOfPhoto::{0}", RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "PhotoId", photoId));
                  return cc;
              }, () =>
              {
                  Sql sql = Sql.Builder
                      .Select("*")
                      .From("spb_PhotoLabels")
                      .Where("PhotoId = @0", photoId);

                  if (!string.IsNullOrEmpty(tenantTypeId))
                      sql.Where("TenantTypeId = @0", tenantTypeId);

                  return sql;
              });
        }
    }
}
