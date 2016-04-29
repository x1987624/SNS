//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using Spacebuilder.CMS;
using Tunynet;
using Tunynet.UI;
using Tunynet.Events;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 栏目业务逻辑
    /// </summary>
    public class ContentFolderService
    {
        private ContentFolderRepository contentFolderRepository;

        /// <summary>
        /// 构造器
        /// </summary>
        public ContentFolderService()
        {
            contentFolderRepository = new ContentFolderRepository();
        }

        /// <summary>
        /// 创建栏目
        /// </summary>
        /// <param name="contentFolder"></param>
        public void Create(ContentFolder contentFolder)
        {
            if (contentFolder.ParentId > 0)
            {
                ContentFolder parentContentFolder = Get(contentFolder.ParentId);
                if (parentContentFolder != null)
                {
                    contentFolder.Depth = parentContentFolder.Depth + 1;
                    contentFolder.ParentIdList = parentContentFolder.ParentIdList + "," + contentFolder.ParentId;
                    parentContentFolder.ChildCount += 1;

                    contentFolderRepository.Update(parentContentFolder);
                }
                else
                {
                    contentFolder.ParentId = 0;
                }
            }
            contentFolderRepository.Insert(contentFolder);
            if (contentFolder.ContentFolderId > 0)
            {
                contentFolder.DisplayOrder = contentFolder.ContentFolderId;
                contentFolderRepository.Update(contentFolder);
                EntityData.ForType(typeof(Navigation)).RealTimeCacheHelper.IncreaseAreaVersion("OwnerId", 0);
            }

            //执行事件
            EventBus<ContentFolder>.Instance().OnAfter(contentFolder, new CommonEventArgs(EventOperationType.Instance().Create(), ApplicationIds.Instance().CMS()));

        }

        /// <summary>
        /// 更新栏目
        /// </summary>
        /// <remarks>
        /// 不要修改ParentId，如需修改请使用Move()
        /// </remarks>
        /// <param name="contentFolder"></param>
        public void Update(ContentFolder contentFolder)
        {
            contentFolderRepository.Update(contentFolder);
            EntityData.ForType(typeof(Navigation)).RealTimeCacheHelper.IncreaseAreaVersion("OwnerId", 0);
            //执行事件
            EventBus<ContentFolder>.Instance().OnAfter(contentFolder, new CommonEventArgs(EventOperationType.Instance().Update(), ApplicationIds.Instance().CMS()));
        }


        /// <summary>
        /// 删除栏目
        /// </summary>
        /// <param name="contentFolder"></param>
        public void Delete(ContentFolder contentFolder)
        {
            if (contentFolder == null)
                return;

            //更新父栏目的ChildCount
            if (contentFolder.ParentId > 0)
            {
                ContentFolder parentContentFolder = Get(contentFolder.ParentId);
                if (parentContentFolder != null)
                {
                    parentContentFolder.ChildCount -= 1;
                    contentFolderRepository.Update(parentContentFolder);
                }
            }

            ContentItemService contentItemService = new ContentItemService();

            //所有后代栏目
            IEnumerable<ContentFolder> descendantContentFolders = GetDescendants(contentFolder.ContentFolderId);
            if (descendantContentFolders != null)
            {
                foreach (var item in descendantContentFolders)
                {
                    contentItemService.DeleteByFolder(item.ContentFolderId);
                    contentFolderRepository.Delete(item);
                }
            }
            contentFolderRepository.Delete(contentFolder);
            contentItemService.DeleteByFolder(contentFolder.ContentFolderId);
            EntityData.ForType(typeof(Navigation)).RealTimeCacheHelper.IncreaseAreaVersion("OwnerId", 0);

            //执行事件
            EventBus<ContentFolder>.Instance().OnAfter(contentFolder, new CommonEventArgs(EventOperationType.Instance().Delete(), ApplicationIds.Instance().CMS()));
        }


        /// <summary>
        /// 把fromFolderId合并到toFolderId
        /// </summary>
        /// <remarks>
        /// 例如：将栏目fromFolderId合并到栏目toFolderId，那么fromFolderId栏目下的所有子栏目和ContentItem全部归到toFolderID栏目，同时删除fromFolderID栏目
        /// </remarks>
        public void Merge(int fromFolderId, int toFolderId)
        {
            ContentFolder toFolder = Get(toFolderId);
            if (toFolder == null)
                return;

            ContentFolder fromFolder = Get(fromFolderId);
            if (fromFolder == null)
                return;

            ContentItemRepository contentItemRepository = new ContentItemRepository();
            foreach (var childSection in fromFolder.Children)
            {
                childSection.ParentId = toFolderId;
                childSection.Depth = toFolder.Depth + 1;

                if (childSection.Depth == 1)
                    childSection.ParentIdList = childSection.ParentId.ToString();
                else
                    childSection.ParentIdList = toFolder.ParentIdList + "," + childSection.ParentId;

                RecursiveUpdateDepthAndParentIdList(childSection);

                ContentItemQuery contentItemQuery = new ContentItemQuery(CacheVersionType.None)
                {
                    ContentFolderId = childSection.ContentFolderId
                };

                PagingDataSet<ContentItem> contentItems = contentItemRepository.GetContentItems(false, contentItemQuery, int.MaxValue, 1);

                foreach (var contentItem in contentItems)
                {
                    contentItem.ContentFolderId = toFolderId;
                    contentItemRepository.Update(contentItem);
                }
            }

            ContentItemQuery currentContentItemQuery = new ContentItemQuery(CacheVersionType.None)
            {
                ContentFolderId = fromFolderId
            };
            PagingDataSet<ContentItem> currentContentItems = contentItemRepository.GetContentItems(false, currentContentItemQuery, int.MaxValue, 1);

            foreach (var item in currentContentItems)
            {
                item.ContentFolderId = toFolderId;
                contentItemRepository.Update(item);
            }

            if (fromFolder.ParentId > 0)
            {
                ContentFolder fromParentFolder = Get(fromFolder.ParentId);
                if (fromParentFolder != null)
                    fromParentFolder.ChildCount -= 1;
            }

            toFolder.ChildCount += fromFolder.ChildCount;
            contentFolderRepository.Update(toFolder);
            contentFolderRepository.Delete(fromFolder);
            EntityData.ForType(typeof(Navigation)).RealTimeCacheHelper.IncreaseAreaVersion("OwnerId", 0);
        }


        /// <summary>
        /// 把fromFolderId移动到toFolderId，作为toFolderId的子栏目
        /// </summary>
        /// <remarks>
        /// 例如：将栏目fromFolderId合并到栏目toFolderId，那么fromFolderId栏目下的所有子栏目和ContentItem全部归到toFolderId栏目，同时删除fromFolderId栏目
        /// </remarks>
        public void Move(int fromFolderId, int toFolderId)
        {
            ContentFolder fromFolder = Get(fromFolderId);
            if (fromFolder == null)
                return;

            if (fromFolder.ParentId > 0)
            {
                ContentFolder fromParentFolder = Get(fromFolder.ParentId);
                if (fromParentFolder != null)
                {
                    fromParentFolder.ChildCount -= 1;
                    contentFolderRepository.Update(fromParentFolder);
                }
            }

            if (toFolderId > 0)
            {
                ContentFolder toFolder = Get(toFolderId);
                if (toFolder == null)
                    return;

                toFolder.ChildCount += 1;
                contentFolderRepository.Update(toFolder);

                fromFolder.ParentId = toFolderId;
                fromFolder.Depth = toFolder.Depth + 1;
                if (fromFolder.Depth == 1)
                    fromFolder.ParentIdList = fromFolder.ParentId.ToString();
                else
                    fromFolder.ParentIdList = toFolder.ParentIdList + "," + fromFolder.ParentId;
            }
            else //移动到顶层
            {
                fromFolder.Depth = 0;
                fromFolder.ParentIdList = string.Empty;
                fromFolder.ParentId = 0;
            }
            contentFolderRepository.Update(fromFolder);

            if (fromFolder.Children != null)
            {
                foreach (var childFolder in fromFolder.Children)
                {
                    childFolder.Depth = fromFolder.Depth + 1;
                    childFolder.ParentIdList = fromFolder.ParentIdList + "," + fromFolder.ContentFolderId;
                    RecursiveUpdateDepthAndParentIdList(childFolder);
                }
            }
            EntityData.ForType(typeof(Navigation)).RealTimeCacheHelper.IncreaseAreaVersion("OwnerId", 0);
        }

        /// <summary>
        /// 更新后代的Depth和ParentIdList
        /// </summary>
        private void RecursiveUpdateDepthAndParentIdList(ContentFolder parentFolder)
        {
            contentFolderRepository.Update(parentFolder);
            if (parentFolder.ChildCount > 0)
            {
                foreach (var folder in parentFolder.Children)
                {
                    folder.ParentId = parentFolder.ContentFolderId;
                    folder.Depth = parentFolder.Depth + 1;
                    folder.ParentIdList = parentFolder.ParentIdList + "," + parentFolder.ContentFolderId;

                    contentFolderRepository.Update(folder);
                    RecursiveUpdateDepthAndParentIdList(folder);
                }
            }
        }

        /// <summary>
        /// 获取栏目
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        public ContentFolder Get(int contentFolderId)
        {
            return contentFolderRepository.Get(contentFolderId);
        }

        /// <summary>
        /// 获取栏目的累计内容数
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        public int GetCumulateItemCount(int contentFolderId)
        {
            ContentFolder contentFolder = Get(contentFolderId);
            if (contentFolder == null)
                return 0;
            int cumulateItemCount = contentFolder.ContentItemCount;
            IEnumerable<ContentFolder> contentFolders = GetDescendants(contentFolderId);
            foreach (var folder in contentFolders)
            {
                cumulateItemCount += folder.ContentItemCount;
            }
            return cumulateItemCount;
        }



        /// <summary>
        /// 获取顶级栏目
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentFolder> GetRootFolders()
        {
            return GetAllFolders().Where(x => x.ParentId == 0);
        }

        /// <summary>
        /// 获取子栏目
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public IEnumerable<ContentFolder> GetChildren(int parentId)
        {
            return GetAllFolders().Where(x => x.ParentId == parentId);
        }

        /// <summary>
        /// 获取所有后代栏目
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        public IEnumerable<ContentFolder> GetDescendants(int contentFolderId)
        {
            ContentFolder contentFolder = Get(contentFolderId);
            if (contentFolder == null || contentFolder.ChildCount == 0)
                return null;


            string descendantParentIdListPrefix = "," + contentFolder.ContentFolderId.ToString();
            if (contentFolder.ParentId == 0)
                return GetAllFolders().Where(x => x.ParentIdList.StartsWith(contentFolder.ContentFolderId.ToString()));
            else
            {
                //所有后代栏目
             //   IEnumerable<ContentFolder> descendantContentFolders = GetAllFolders().Where(x => x.ParentIdList.EndsWith(descendantParentIdListPrefix));
                return  GetAllFolders().Where(x => x.ParentIdList.EndsWith(descendantParentIdListPrefix));
            }
        }

        /// <summary>
        /// 以缩进排序方式获取所有栏目
        /// </summary>
        public IEnumerable<ContentFolder> GetIndentedFolders()
        {
            IEnumerable<ContentFolder> rootFolders = GetRootFolders();
            List<ContentFolder> organizedFolders = new List<ContentFolder>();
            foreach (var folder in rootFolders)
            {
                organizedFolders.Add(folder);
                OrganizeForIndented(folder, organizedFolders);
            }

            return organizedFolders;
        }

        /// <summary>
        /// 把栏目组织成缩进格式
        /// </summary>
        private static void OrganizeForIndented(ContentFolder parentFolder, List<ContentFolder> organizedFolders)
        {
            if (parentFolder.ChildCount > 0)
            {
                foreach (ContentFolder child in parentFolder.Children)
                {
                    organizedFolders.Add(child);
                    OrganizeForIndented(child, organizedFolders);
                }
            }
        }


        /// <summary>
        /// 获取所有ContentFolder
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ContentFolder> GetAllFolders()
        {
            return contentFolderRepository.GetAll();
        }


    }
}
