//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-23</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Common;
using Tunynet.Utilities;
using CodeKicker.BBCode;
using Spacebuilder.Common;
using System.Text.RegularExpressions;
using Tunynet;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖子正文解析器
    /// </summary>
    public class BarBodyProcessor : IBodyProcessor
    {
        public string Process(string body, string tenantTypeId, long associateId, long userId)
        {
            
            
            

            //解析附件、视频、音频、@用户、表情
            //todo:需要封装附件、视频、音频、@用户、表情解析的辅助方法，参照表情解析完成；
            //性能优化采用敏感词的替换算法，由宝声协助完成

            //解析at用户
            AtUserService atUserService = new AtUserService(tenantTypeId);
            body = atUserService.ResolveBodyForDetail(body, associateId, userId, AtUserTagGenerate);

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
            
            body = new EmotionService().EmoticonTransforms(body);
            body = new ParsedMediaService().ResolveBodyForHtmlDetail(body, ParsedMediaTagGenerate);

            return body;
        }

        #region private method

        /// <summary>
        /// 生成at用户标签
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="displayName">显示名</param>
        private string AtUserTagGenerate(string userName, string displayName)
        {
            return string.Format("<a href=\"{1}\" target=\"_blank\" title=\"{0}\">@{0}</a> ", displayName, SiteUrls.Instance().SpaceHome(userName));
        }

        /// <summary>
        /// 添加BBTag实体
        /// </summary>
        /// <param name="htmlTemplate">html模板</param>
        /// <param name="attachment">带替换附件</param>
        /// <returns></returns>
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
        /// <param name="shortUrl">短网址</param>
        /// <param name="parsedMedia">多媒体网址信息</param>
        private string ParsedMediaTagGenerate(string shortUrl, ParsedMedia parsedMedia)
        {
            if (parsedMedia == null)
                return string.Empty;

            if (parsedMedia.MediaType == MediaType.Audio)
            {
                string musicHtml = "<p><a href=\"{0}\" ntype=\"mediaPlay\">{1}<span class=\"tn-icon tn-icon-music tn-icon-inline\"></span></a><br />"
                                   + "<a  href=\"{0}\" ntype=\"mediaPlay\" class=\"tn-button tn-corner-all tn-button-default tn-button-text-icon-primary\">"
                                   + "<span class=\"tn-icon tn-icon-triangle-right\"></span><span class=\"tn-button-text\">音乐播放</span></a></p>";
                return string.Format(musicHtml, SiteUrls.Instance()._MusicDetail(parsedMedia.Alias), shortUrl);
            }
            else if (parsedMedia.MediaType == MediaType.Video)
            {
                string videoHtml = "<p><a  href=\"{0}\" ntype=\"mediaPlay\">{1}<span class=\"tn-icon tn-icon-movie tn-icon-inline\"></span></a><br />"
                                    + "<a ntype=\"mediaPlay\" href=\"{0}\"><img src=\"{2}\"></a></p>";
                return string.Format(videoHtml, SiteUrls.Instance()._VideoDetail(parsedMedia.Alias), shortUrl, parsedMedia.ThumbnailUrl);
            }

            return string.Empty;
        }

        #endregion

    }
}