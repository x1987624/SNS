using System.Collections.Generic;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Wiki.EventModules
{
    /// <summary>
    /// 处理日志索引的EventMoudle
    /// </summary>
    public class WikiIndexEventModule : IEventMoudle
    {
        private WikiService wikiService = new WikiService();
        private CategoryService categoryService = new CategoryService();
        private TagService tagService = new TagService(TenantTypeIds.Instance().Wiki());

        //因为EventModule.RegisterEventHandler()在web启动时初始化，而wikiSearcher的构造函数依赖于WCF服务（分布式搜索部署情况下），此时WCF服务尚无法连接，因此wikiSearcher不能在此处构建，只能再下面的方法中构建
        private WikiSearcher wikiSearcher = null;

        public void RegisterEventHandler()
        {
            EventBus<WikiPage>.Instance().After += new CommonEventHandler<WikiPage, CommonEventArgs>(WikiThread_After);

            EventBus<string, TagEventArgs>.Instance().BatchAfter += new BatchEventHandler<string, TagEventArgs>(AddTagsToWiki_BatchAfter);
            EventBus<Tag>.Instance().Before += new CommonEventHandler<Tag, CommonEventArgs>(DeleteUpdateTags_Before);
            EventBus<ItemInTag>.Instance().After += new CommonEventHandler<ItemInTag, CommonEventArgs>(DeleteItemInTags);

            EventBus<string, TagEventArgs>.Instance().BatchAfter += new BatchEventHandler<string, TagEventArgs>(AddCategoriesToWiki_BatchAfter);
            EventBus<Category>.Instance().Before += new CommonEventHandler<Category, CommonEventArgs>(DeleteUpdateCategories_Before);
        }

        //todo:wanf 分类 及 标签 索引
        #region 分类增量索引

        /// <summary>
        /// 为日志添加分类时触发
        /// </summary>
        private void AddCategoriesToWiki_BatchAfter(IEnumerable<string> senders, TagEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().Wiki())
            {
                long WikiPageId = eventArgs.ItemId;
                if (wikiSearcher == null)
                {
                    wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
                }
                wikiSearcher.Update(wikiService.Get(WikiPageId));
            }
        }

        /// <summary>
        /// 删除和更新分类时触发
        /// </summary>
        private void DeleteUpdateCategories_Before(Category sender, CommonEventArgs eventArgs)
        {
            if (sender.TenantTypeId == TenantTypeIds.Instance().Wiki())
            {
                if (eventArgs.EventOperationType == EventOperationType.Instance().Delete() || eventArgs.EventOperationType == EventOperationType.Instance().Update())
                {
                    IEnumerable<long> wikiPageIds = categoryService.GetItemIds(sender.CategoryId, false);
                    if (wikiSearcher == null)
                    {
                        wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
                    }
                    wikiSearcher.Update(wikiService.GetWikiPages(wikiPageIds));
                }
            }
        }
        #endregion

        #region 标签增量索引

        /// <summary>
        /// 为日志添加标签时触发
        /// </summary>
        private void AddTagsToWiki_BatchAfter(IEnumerable<string> senders, TagEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().Wiki())
            {
                long WikiPageId = eventArgs.ItemId;
                if (wikiSearcher == null)
                {
                    wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
                }
                wikiSearcher.Update(wikiService.Get(WikiPageId));
            }
        }
        /// <summary>
        /// 删除和更新标签时触发
        /// </summary>
        private void DeleteUpdateTags_Before(Tag sender, CommonEventArgs eventArgs)
        {
            if (sender.TenantTypeId == TenantTypeIds.Instance().Wiki())
            {
                if (eventArgs.EventOperationType == EventOperationType.Instance().Delete() || eventArgs.EventOperationType == EventOperationType.Instance().Update())
                {
                    //根据标签获取所有使用该标签的(内容项)日志
                    IEnumerable<long> WikiPageId = tagService.GetItemIds(sender.TagName, null);
                    if (wikiSearcher == null)
                    {
                        wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
                    }
                    wikiSearcher.Update(wikiService.GetWikiPages(WikiPageId));
                }
            }
        }
        private void DeleteItemInTags(ItemInTag sender, CommonEventArgs eventArgs)
        {
            if (sender.TenantTypeId == TenantTypeIds.Instance().Wiki())
            {
                long WikiPageId = sender.ItemId;
                if (wikiSearcher == null)
                {
                    wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
                }
                wikiSearcher.Update(wikiService.Get(WikiPageId));
            }
        }
        #endregion

        #region 日志增量索引
        /// <summary>
        /// 日志增量索引
        /// </summary>
        private void WikiThread_After(WikiPage wikiPage, CommonEventArgs eventArgs)
        {
            if (wikiPage == null)
            {
                return;
            }

            if (wikiSearcher == null)
            {
                wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
            }

            //添加索引
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {                
                wikiSearcher.Insert(wikiPage);                
            }

            //删除索引
            if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                wikiSearcher.Delete(wikiPage.PageId);
            }

            //更新索引
            if (eventArgs.EventOperationType == EventOperationType.Instance().Update())
            {
                wikiSearcher.Update(wikiPage);
            }
        }
        #endregion
    }
}