using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;
using Tunynet.Utilities;

namespace Spacebuilder.CMS
{
    public class ContentAttachmentRepository : Repository<ContentAttachment>
    {
        public IEnumerable<ContentAttachment> GetsByUserId(long userId)
        {
            var sql = PetaPoco.Sql.Builder;
            sql.Select("*")
               .From("spb_cms_ContentAttachments")
               .Where("UserId=@0", userId);
            IEnumerable<ContentAttachment> attachments = CreateDAO().Fetch<ContentAttachment>(sql);
            return attachments;
        }

        public void DeletesByUserId(long userId)
        {
            var sql = PetaPoco.Sql.Builder;
            sql.Append("DELETE  FROM spb_cms_ContentAttachments").Where("UserId=@0", userId);
            CreateDAO().Execute(sql);
        }

        public PagingDataSet<ContentAttachment> Gets(long? userId, string keyword, DateTime? startDate, DateTime? endDate, MediaType? mediaType, int pageSize, int pageIndex)
        {
            var sql = PetaPoco.Sql.Builder;
            if (!String.IsNullOrEmpty(keyword))
                sql.Where("FriendlyFileName like @0", "%" + StringUtility.StripSQLInjection(keyword) + "%");

            if (userId.HasValue && userId.Value > 0)
                sql.Where("UserId = @0", userId.Value);

            if (mediaType != null)
                sql.Where("MediaType = @0", mediaType.Value);

            #region liucg-0726-ReleaseDate 改为 DateCreated

            if (startDate != null)
            {
                sql.Where("DateCreated >= @0", startDate.Value);
            }

            if (endDate != null)
            {
                sql.Where("DateCreated < @0", endDate.Value.AddDays(1));
            }
            #endregion
            sql.OrderBy("AttachmentId  DESC");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }
    }
}