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
    /// 相册推荐Url获取器
    /// </summary>
    public class AlbumRecommendUrlGetter : IRecommendUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().Album(); }
        }

        /// <summary>
        /// 详细页面地址
        /// </summary>
        /// <param name="itemId">推荐内容Id</param>
        /// <returns></returns>
        public string RecommendItemDetail(long itemId)
        {
            Album album = new PhotoService().GetAlbum(itemId);
            if (album == null)
                return string.Empty;
            string userName = UserIdToUserNameDictionary.GetUserName(album.UserId);
            return SiteUrls.Instance().AlbumDetailList(userName,itemId);
        }
    }
}