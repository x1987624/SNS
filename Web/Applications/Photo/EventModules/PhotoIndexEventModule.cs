//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Photo.EventModules
{
    /// <summary>
    /// 处理相册全文检索索引的EventMoudle
    /// </summary>
    public class PhotoIndexEventModule : IEventMoudle
    {
        private PhotoSearcher photoSearcher = null;

        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        public void RegisterEventHandler()
        {
            EventBus<Photo>.Instance().After += new CommonEventHandler<Photo, CommonEventArgs>(Photo_After);
            EventBus<string, TagEventArgs>.Instance().BatchAfter += new BatchEventHandler<string, TagEventArgs>(AddTagsToPhoto_BatchAfter);

        }

        #region 照片增量索引

        //照片增加、删除、更新、设置精华、取消精华
        private void Photo_After(Photo photo, CommonEventArgs eventArgs)
        {
            if (photo == null)
            {
                return;
            }
            if (photoSearcher == null)
            {
                photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            }
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                photoSearcher.Insert(photo);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Update() || eventArgs.EventOperationType == EventOperationType.Instance().Approved() || eventArgs.EventOperationType == EventOperationType.Instance().Disapproved())
            {
                photoSearcher.Update(photo);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                photoSearcher.Delete(photo);
            }
        }

        /// <summary>
        /// 为照片添加标签时触发
        /// </summary>
        private void AddTagsToPhoto_BatchAfter(IEnumerable<string> senders, TagEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().Photo())
            {
                long photoId = eventArgs.ItemId;
                if (photoSearcher == null)
                {
                    photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
                }
                photoSearcher.Update(new PhotoService().GetPhoto(photoId));
            }
        }

        #endregion
    }
}