//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CodeKicker.BBCode;
using Spacebuilder.Common;
using Tunynet.Common;
using Tunynet.Utilities;
using System;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科正文解析器
    /// </summary>
    public class WikiBodyProcessor : IBodyProcessor
    {

        public string Process(string body, string tenantTypeId, long associateId, long userId)
        {
            AttachmentService attachmentService = new AttachmentService(tenantTypeId);
            IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(associateId);

            if (attachments != null && attachments.Count() > 0)
            {
                IList<BBTag> bbTags = new List<BBTag>();
                string htmlTemplate = "<div class=\"tn-annexinlaid\"><a href=\"javascript:;\" target=\"_blank\" menu=\"#attachement-artdialog-{4}\">{0}</a>（<em>{1}</em>{2}，<em>下载次数：{3}</em>）</div>";

                //解析文本中附件
                IEnumerable<Attachment> attachmentsFiles = attachments.Where(n => n.MediaType != MediaType.Image);
                foreach (var attachment in attachmentsFiles)
                {
                    bbTags.Add(AddBBTag(htmlTemplate, attachment));
                }

                body = HtmlUtility.BBCodeToHtml(body, bbTags);
            }
            body = new ParsedMediaService().ResolveBodyForHtmlDetail(body, ParsedMediaTagGenerate);
            return body;
        }

        #region private method

        /// <summary>
        /// 添加BBTag实体
        /// </summary>
        /// <param name="htmlTemplate">html模板</param>
        /// <param name="attachment">带替换附件</param>
        /// <returns>BBTag实体</returns>
        private BBTag AddBBTag(string htmlTemplate, Attachment attachment)
        {

            BBAttribute bbAttribute = new BBAttribute("attachTemplate", "",
                                                      n =>
                                                      {
                                                          return string.Format(htmlTemplate,
                                                                               attachment.FriendlyFileName,
                                                                               attachment.FriendlyFileLength,
                                                                               attachment.Price > 0 ? "，<em>需要" + attachment.Price + "积分</em>" : "",
                                                                               attachment.DownloadCount,
                                                                               attachment.AttachmentId);
                                                      },
                                                      HtmlEncodingMode.UnsafeDontEncode);

            return new BBTag("attach:" + attachment.AttachmentId, "${attachTemplate}", "", false, BBTagClosingStyle.LeafElementWithoutContent, null, bbAttribute);
        }

        /// <summary>
        /// 生成多媒体内容标签
        /// </summary>
        /// <param name="shrotUrl">短网址</param>
        /// <param name="parsedMedia">多媒体连接实体</param>
        private string ParsedMediaTagGenerate(string shrotUrl, ParsedMedia parsedMedia)
        {
            if (parsedMedia == null)
                return string.Empty;

            if (parsedMedia.MediaType == MediaType.Audio)
            {
                string musicHtml = "<p><a href=\"{0}\" ntype=\"mediaPlay\">{1}<span class=\"tn-icon tn-icon-music tn-icon-inline\"></span></a><br />"
                                   + "<a  href=\"{0}\" ntype=\"mediaPlay\" class=\"tn-button tn-corner-all tn-button-default tn-button-text-icon-primary\">"
                                   + "<span class=\"tn-icon tn-icon-triangle-right\"></span><span class=\"tn-button-text\">音乐播放</span></a></p>";
                return string.Format(musicHtml, SiteUrls.Instance()._MusicDetail(parsedMedia.Alias), shrotUrl);
            }
            else if (parsedMedia.MediaType == MediaType.Video)
            {
                string videoHtml = "<p><a  href=\"{0}\" ntype=\"mediaPlay\">{1}<span class=\"tn-icon tn-icon-movie tn-icon-inline\"></span></a><br />"
                                    + "<a ntype=\"mediaPlay\" href=\"{0}\"><img src=\"{2}\"></a></p>";
                return string.Format(videoHtml, SiteUrls.Instance()._VideoDetail(parsedMedia.Alias), shrotUrl, parsedMedia.ThumbnailUrl);
            }

            return string.Empty;
        }
        #endregion
    }
}