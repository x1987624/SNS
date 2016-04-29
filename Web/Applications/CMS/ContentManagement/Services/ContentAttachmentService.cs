//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.FileStore;
using Tunynet.Common.Configuration;
using System.IO;
using Tunynet.Imaging;
using Tunynet.Events;
using System.Drawing;
using Tunynet.Common;
using Tunynet;
using CodeKicker.BBCode;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.CMS
{

    /// <summary>
    /// 附件业务逻辑类
    /// </summary>
    public class ContentAttachmentService
    {
        private IUserService userService = DIContainer.Resolve<IUserService>();

        //contentAttachmentRepository
        private ContentAttachmentRepository contentAttachmentRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ContentAttachmentService()
        {
            this.contentAttachmentRepository = new ContentAttachmentRepository();
            this.StoreProvider = DIContainer.Resolve<IStoreProvider>();
        }

        /// <summary>
        /// 文件存储Provider
        /// </summary>
        public IStoreProvider StoreProvider { get; private set; }

        #region Create & Delete

        /// <summary>
        /// 创建附件
        /// </summary>
        /// <param name="attachment">附件</param>
        /// <param name="contentStream">文件流</param>
        public void Create(ContentAttachment attachment, Stream contentStream)
        {
            if (contentStream == null)
            {
                return;
            }

            StoreProvider.AddOrUpdateFile(attachment.GetRelativePath(), attachment.FileName, contentStream);

            if (contentStream != null)
            {
                contentStream.Dispose();
            }

            EventBus<ContentAttachment>.Instance().OnBefore(attachment, new CommonEventArgs(EventOperationType.Instance().Create()));
            contentAttachmentRepository.Insert(attachment);
            EventBus<ContentAttachment>.Instance().OnAfter(attachment, new CommonEventArgs(EventOperationType.Instance().Create()));
        }

        /// <summary>
        /// 附件重新命名（修改FriendlyFileName）
        /// </summary>
        /// <param name="attachmentId">附件Id</param>
        /// <param name="newFriendlyFileName">新附件名</param>
        public void Rename(long attachmentId, string newFriendlyFileName)
        {
            ContentAttachment attachment = Get(attachmentId);
            if (attachment != null)
            {
                attachment.FriendlyFileName = newFriendlyFileName;
                contentAttachmentRepository.Update(attachment);
            }
        }

        /// <summary>
        /// 附件重新调整售价（修改Price）
        /// </summary>
        /// <param name="attachmentId">附件Id</param>
        /// <param name="price">新售价</param>
        public void UpdatePrice(long attachmentId, int price)
        {
            ContentAttachment attachment = Get(attachmentId);
            if (attachment != null)
            {
                attachment.Price = price;
                contentAttachmentRepository.Update(attachment);
            }
        }

        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="attachmentId">附件Id</param>
        public void Delete(long attachmentId)
        {
            ContentAttachment attachment = Get(attachmentId);
            if (attachment != null)
            {
                Delete(attachment);
            }
        }

        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="attachment">附件</param>
        public void Delete(ContentAttachment attachment)
        {
            DeleteStoredFile(attachment);
            EventBus<ContentAttachment>.Instance().OnBefore(attachment, new CommonEventArgs(EventOperationType.Instance().Delete()));
            contentAttachmentRepository.Delete(attachment);
            EventBus<ContentAttachment>.Instance().OnAfter(attachment, new CommonEventArgs(EventOperationType.Instance().Delete()));
        }

        /// <summary>
        /// 删除UserId相关的附件
        /// </summary>
        /// <param name="userId">上传者Id</param>
        public virtual void DeletesByUserId(long userId)
        {
            IEnumerable<ContentAttachment> attachments = GetsByUserId(userId);
            foreach (var attachment in attachments)
            {
                DeleteStoredFile(attachment);
            }
            contentAttachmentRepository.DeletesByUserId(userId);
        }

        /// <summary>
        /// 删除文件系统中的文件
        /// </summary>
        /// <param name="attachment">附件</param>
        protected void DeleteStoredFile(ContentAttachment attachment)
        {
            StoreProvider.DeleteFile(attachment.GetRelativePath(), attachment.FileName);
        }

        #endregion

        #region Get & Gets

        /// <summary>
        /// 依据attachmentId获取附件
        /// </summary>
        /// <param name="attachmentId">附件Id</param>
        public ContentAttachment Get(long attachmentId)
        {
            return contentAttachmentRepository.Get(attachmentId);
        }

        /// <summary>
        /// 依据attachmentIds组装实体列表
        /// </summary>
        /// <param name="attachmentIds">附件Id集合</param>
        public IEnumerable<ContentAttachment> Gets(IEnumerable<long> attachmentIds)
        {
            return contentAttachmentRepository.PopulateEntitiesByEntityIds(attachmentIds);
        }

        /// <summary>
        /// 依据userId获取附件列表
        /// </summary>
        /// <param name="userId">附件上传人Id</param>
        /// <returns>附件列表</returns>
        public IEnumerable<ContentAttachment> GetsByUserId(long userId)
        {
            return contentAttachmentRepository.GetsByUserId(userId);
        }

        /// <summary>
        /// 搜索附件并分页显示
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="keyword"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="mediaType"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<ContentAttachment> Gets(long? userId = null, string keyword = null, DateTime? startDate = null, DateTime? endDate = null, MediaType? mediaType = null, int pageSize = 20, int pageIndex = 1)
        {
            return contentAttachmentRepository.Gets(userId, keyword, startDate, endDate, mediaType, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取直连URL
        /// </summary>
        /// <param name="attachment">附件</param>
        /// <returns>返回可以http直连该附件的url</returns>
        public string GetDirectlyUrl(ContentAttachment attachment)
        {
            return StoreProvider.GetDirectlyUrl(attachment.GetRelativePath(), attachment.FileName);
        }

        #endregion

    }
}
