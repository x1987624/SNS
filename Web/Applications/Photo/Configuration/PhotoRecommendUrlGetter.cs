//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 照片推荐Url获取器
    /// </summary>
    public class PhotoRecommendUrlGetter : IRecommendUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().Photo(); }
        }

        /// <summary>
        /// 详细页面地址
        /// </summary>
        /// <param name="itemId">推荐内容Id</param>
        /// <returns></returns>
        public string RecommendItemDetail(long itemId)
        {
            Photo photo = new PhotoService().GetPhoto(itemId);
            if (photo == null)
                return string.Empty;
            string userName = UserIdToUserNameDictionary.GetUserName(photo.UserId);
            return SiteUrls.Instance().PhotoDetail(itemId);
        }
    }
}