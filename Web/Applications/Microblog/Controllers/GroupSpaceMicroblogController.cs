﻿//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-11-30</createdate>
//<author>zhaok</author>
//<email>zhaok@tunynet.com</email>
//<log date="2012-11-30" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
using System.Web.Mvc;
using Spacebuilder.Group;
using Tunynet;
using Tunynet.Common;
using Tunynet.UI;
using Tunynet.Utilities;
using DevTrends.MvcDonutCaching;
using Spacebuilder.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 群组微博
    /// </summary>
    [TitleFilter(TitlePart = "群组微博", IsAppendSiteName = true)]
    [GroupSpaceAuthorize]
    [Themed(PresentAreaKeysOfBuiltIn.GroupSpace, IsApplication = true)]
    public class GroupSpaceMicroblogController : Controller
    {

        #region Service

        public IPageResourceManager pageResourceManager { get; set; }
        public MicroblogService microblogService { get; set; }
        public GroupService groupService { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().Microblog());

        #endregion

        #region 微博

        /// <summary>
        /// 微博详细页
        /// </summary>
        [HttpGet]
        public ActionResult Detail(string spaceKey, long microblogId)
        {
            GroupEntity group = groupService.Get(spaceKey);
            if (group == null) return HttpNotFound();

            MicroblogEntity entity = microblogService.Get(microblogId);
            if (entity == null) return HttpNotFound();

            pageResourceManager.InsertTitlePart("群组微博详细页");

            ViewData["group"] = group;
            return View(entity);
        }

        #endregion

        #region 话题

        /// <summary>
        /// 群组话题
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _TopGroupTopics(string spaceKey, int topNumber, SortBy_Tag? sortBy)
        {
            IEnumerable<Tag> tags = new List<Tag>();
            if (sortBy == SortBy_Tag.PreWeekItemCountDesc)
            {
                tags = new TagService(TenantTypeIds.Instance().Tag()).GetTopTags(topNumber, null, sortBy);
            }
            else
            {
                tags = tagService.GetTopTags(topNumber, null, sortBy ?? SortBy_Tag.DateCreatedDesc);
            }
            return View(tags);
        }

        /// <summary>
        /// 话题详情页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TopicDetail(string spaceKey, string tagName)
        {
            Tag tag = tagService.Get(tagName);
            if (tag == null) return HttpNotFound();

            GroupEntity group = groupService.Get(spaceKey);
            if (group == null) return HttpNotFound();

            FavoriteService FavoriteService = new FavoriteService(TenantTypeIds.Instance().Tag());

            pageResourceManager.InsertTitlePart(group.GroupName);
            pageResourceManager.InsertTitlePart(tag.TagName);
            if (!string.IsNullOrEmpty(tag.Description))
            {
                pageResourceManager.SetMetaOfDescription(HtmlUtility.StripHtml(tag.Description, false, false));
            }

            return View(tag);
        }

        #endregion
    }
}