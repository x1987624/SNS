//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2013</copyright>
//<version>V0.5</verion>
//<createdate>2013-6-24</createdate>
//<author>yangmj</author>
//<email>yangmj@tunynet.com</email>
//<log date="2013-6-24" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.Common;


namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 编辑Wiki的EditModel
    /// </summary>
    public class WikiPageEditModel
    {
        /// <summary>
        ///词条Id
        /// </summary>
        public long PageId { get; set; }

        /// <summary>
        ///拥有者Id（独立百科为0；所属为群组时为群组Id）
        /// </summary>
        public long? OwnerId { get; set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        /// 站点日志分类
        /// </summary>
        [Required(ErrorMessage = "请选择分类")]
        public long SiteCategoryId { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public IEnumerable<string> TagNames { get; set; }

        /// <summary>
        ///是否锁定词条
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///词条名称
        /// </summary>
        [WaterMark(Content = "在此输入词条名称")]
        [Required(ErrorMessage = "请输入词条名称")]
        [Remote("ValidateTitle", "ChannelWiki", AdditionalFields = "PageId", ErrorMessage = "已有同名词条")]
        [StringLength(TextLengthSettings.TEXT_SUBJECT_MAXLENGTH, MinimumLength = TextLengthSettings.TEXT_SUBJECT_MINLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        #region WikiPageVersion

        /// <summary>
        /// VersionId
        /// </summary>
        public long VersionId { get; set; }

        /// <summary>
        ///摘要
        /// </summary>
        [Display(Name = "摘要")]
        [StringLength(TextLengthSettings.TEXT_DESCRIPTION_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        /// <summary>
        ///内容
        /// </summary>
        [Required(ErrorMessage = "请输入内容")]
        [StringLength(TextLengthSettings.TEXT_BODY_MAXLENGTH, MinimumLength = TextLengthSettings.TEXT_BODY_MINLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [AllowHtml]
        [DataType(DataType.Html)]
        public string Body { get; set; }

        /// <summary>
        ///修改原因
        /// </summary>
        [Display(Name = "修改原因")]
        [Required(ErrorMessage = "请输入修改原因")]
        [StringLength(TextLengthSettings.TEXT_DESCRIPTION_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [DataType(DataType.MultilineText)]
        public string Reason{ get; set; }

        #endregion

        private long featuredImageAttachmentId = 0;
        /// <summary>
        /// 标题图对应的附件Id
        /// </summary>
        public long FeaturedImageAttachmentId
        {
            get { return featuredImageAttachmentId; }
            set { featuredImageAttachmentId = value; }
        }

        /// <summary>
        /// 转换成WikiPage
        /// </summary>
        public WikiPage AsWikiPage()
        {
            WikiPage page = WikiPage.New();
            WikiService service = new WikiService();

            if (this.PageId < 1)//创建
            {
                if (this.OwnerId.HasValue)
                    page.OwnerId = this.OwnerId.Value;
                else
                    page.OwnerId = UserContext.CurrentUser.UserId;

                page.UserId = UserContext.CurrentUser.UserId;
                page.Author = UserContext.CurrentUser.DisplayName;


                page.Title = this.Title;

            }
            else//编辑词条
            {
                page = service.Get(this.PageId);
            }
            page.IsLocked = this.IsLocked;

            page.FeaturedImageAttachmentId = this.FeaturedImageAttachmentId;
            if (page.FeaturedImageAttachmentId > 0)
            {
                Attachment attachment = new AttachmentService(TenantTypeIds.Instance().WikiPage()).Get(this.FeaturedImageAttachmentId);
                if (attachment != null)
                {
                    page.FeaturedImage = attachment.GetRelativePath() + "\\" + attachment.FileName;
                }
                else
                {
                    page.FeaturedImageAttachmentId = 0;
                }
            }
            else
            {
                page.FeaturedImage = string.Empty;
            }

            return page;
        }

        /// <summary>
        /// 转换成WikiPageVersion
        /// </summary>
        public WikiPageVersion AsWikiPageVersion()
        {
            WikiPageVersion pageVersion = WikiPageVersion.New();

            if (this.PageId > 0)
            {
                WikiService service = new WikiService();
                WikiPage page = service.Get(this.PageId);
                if (page != null)
                    pageVersion.Title = page.Title;
            }
            else
            {
                pageVersion.Title = this.Title;
            }

            if (this.OwnerId.HasValue)
                pageVersion.OwnerId = this.OwnerId.Value;
            else
                pageVersion.OwnerId = UserContext.CurrentUser.UserId;

            pageVersion.UserId = UserContext.CurrentUser.UserId;
            pageVersion.Author = UserContext.CurrentUser.DisplayName;
            pageVersion.PageId = this.PageId;

            if (!string.IsNullOrEmpty(this.Summary))
                pageVersion.Summary = this.Summary;
            pageVersion.Body = this.Body;
            if (!string.IsNullOrEmpty(this.Reason))
                pageVersion.Reason = this.Reason;

            return pageVersion;
        }
    }

    public static class WikiPageExtensions
    {
        /// <summary>
        /// 转为PageEditModel（包括了WikiPageVersion）
        /// </summary>
        /// <param name="page">词条</param>
        /// <param name="version">词条版本</param>
        /// <returns></returns>
        public static WikiPageEditModel AsEditModel(this WikiPage page)
        {
            return new WikiPageEditModel
            {
                PageId = page.PageId,
                OwnerId = page.OwnerId,
                TenantTypeId = page.TenantTypeId,
                Title = page.Title,
                FeaturedImageAttachmentId = page.FeaturedImageAttachmentId,
                SiteCategoryId = page.SiteCategory.CategoryId,
                Summary = page.LastestVersion.Summary,
                Body = page.Body,
                VersionId = page.LastestVersion.VersionId,
                IsLocked = page.IsLocked
            };
        }

    }
}