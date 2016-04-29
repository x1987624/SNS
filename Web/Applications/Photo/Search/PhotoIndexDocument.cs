//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Documents;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 照片索引文档
    /// </summary>
    public class PhotoIndexDocument
    {
        #region 索引字段
        /// <summary>
        /// 照片id
        /// </summary>
        public static readonly string PhotoId = "PhotoId";

        /// <summary>
        /// 相册id
        /// </summary>
        public static readonly string AlbumId = "AlbumId";

        /// <summary>
        /// 租户类型
        /// </summary>
        public static readonly string TenantTypeId = "TenantTypeId";

        /// <summary>
        /// 用户id
        /// </summary>
        public static readonly string UserId = "UserId";

        /// <summary>
        ///作者 
        /// </summary>
        public static readonly string Author = "Author";

        /// <summary>
        /// 描述
        /// </summary>
        public static readonly string Description = "Description";

        /// <summary>
        /// 创建时间
        /// </summary>
        public static readonly string DateCreated = "DateCreated";

        /// <summary>
        /// 标签
        /// </summary>
        public static readonly string Tag = "Tag";

        /// <summary>
        /// 审核状态
        /// </summary>
        public static readonly string AuditStatus = "AuditStatus";

        /// <summary>
        /// 隐私状态
        /// </summary>
        public static readonly string PrivacyStatus = "PrivacyStatus";


        #endregion

        /// <summary>
        /// 将Photo转换成Document
        /// </summary>
        /// <param name="photo">Photo类型实体</param>
        /// <returns>Document类型实体</returns>
        public static Document Convert(Photo photo)
        {
            Document doc = new Document();

            doc.Add(new Field(PhotoIndexDocument.PhotoId, photo.PhotoId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.AlbumId, photo.AlbumId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.UserId, photo.UserId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.TenantTypeId,photo.TenantTypeId,Field.Store.YES,Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.Author, photo.Author.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.Description, photo.Description.ToLower(), Field.Store.NO, Field.Index.ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.DateCreated, DateTools.DateToString(photo.DateCreated, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.AuditStatus,((int)photo.AuditStatus).ToString(),Field.Store.YES,Field.Index.NOT_ANALYZED));
            doc.Add(new Field(PhotoIndexDocument.PrivacyStatus,((int)photo.PrivacyStatus).ToString(),Field.Store.YES,Field.Index.NOT_ANALYZED));

            foreach (var tag in photo.Tags)
            {
                doc.Add(new Field(PhotoIndexDocument.Tag, tag.TagName.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
            }
            return doc;
        }

        /// <summary>
        /// 将Photo批量转换成Document
        /// </summary>
        /// <param name="photos">待转换的Photo集合</param>
        /// <returns>得到的Document集合</returns>
        public static IEnumerable<Document> Convert(IEnumerable<Photo> photos)
        {
            List<Document> docs = new List<Document>();
            foreach (Photo photo in photos)
            {
                Document doc = Convert(photo);
                docs.Add(doc);
            }
            return docs;
        }

    }
}