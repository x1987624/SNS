using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用的圈人仓储接口
    /// </summary>
    public interface IPhotoLabelRepository : IRepository<PhotoLabel>
    {
        /// <summary>
        /// 获取圈人对象列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="photoId">照片ID</param>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <returns>返回圈人列表</returns>
        IEnumerable<PhotoLabel> GetLabelsOfPhoto(long photoId, string tenantTypeId);
    }
}
