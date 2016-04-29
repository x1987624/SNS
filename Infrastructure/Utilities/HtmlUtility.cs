//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-02</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-12-02" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using HtmlAgilityPack;
using System.Collections.Specialized;
using CodeKicker.BBCode;
using System.Web;

namespace Tunynet.Utilities
{
    /// <summary>
    /// Html工具类
    /// </summary>
    public class HtmlUtility
    {
        /// <summary>
        /// 移除html内的Elemtnts/Attributes及&amp;nbsp;，超过charLimit个字符进行截断
        /// </summary>
        /// <param name="rawHtml">待截字的html字符串</param>
        /// <param name="charLimit">最多允许返回的字符数</param>
        public static string TrimHtml(string rawHtml, int charLimit)
        {
            if (string.IsNullOrEmpty(rawHtml))
                return string.Empty;

            string nohtml = StripHtml(rawHtml, true, false);
            nohtml = StripBBTags(nohtml);

            if (charLimit <= 0 || charLimit >= nohtml.Length)
                return nohtml;
            else
                return StringUtility.Trim(nohtml, charLimit);
        }

        /// <summary>
        /// 移除Html标签
        /// </summary>
        /// <param name="rawString">待处理字符串</param>
        /// <param name="removeHtmlEntities">是否移除Html实体</param>
        /// <param name="enableMultiLine">是否保留换行符（<p/><br/>会转换成换行符）</param>
        /// <returns>返回处理后的字符串</returns>
        public static string StripHtml(string rawString, bool removeHtmlEntities, bool enableMultiLine)
        {
            string result = rawString;
            if (enableMultiLine)
            {
                result = Regex.Replace(result, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                result = Regex.Replace(result, "<br(?:\\s*)/>", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            result = result.Replace("\"", "''");
            if (removeHtmlEntities)
            {
                //StripEntities removes the HTML Entities 
                result = Regex.Replace(result, "&[^;]*;", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            return Regex.Replace(result, "<[^>]+>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }


        /// <summary>
        /// 移除Html用于内容预览
        /// </summary>
        /// <remarks>
        /// 将br、p替换为\n，“'”替换为对应Html实体，并过滤所有Html、Xml、UBB标签
        /// </remarks>
        /// <param name="rawString">用于预览的文本</param>
        /// <returns>返回移除换行及html、ubb标签的字符串</returns>
        public static string StripForPreview(string rawString)
        {
            string tempString;

            tempString = rawString.Replace("<br>", "\n");
            tempString = tempString.Replace("<br/>", "\n");
            tempString = tempString.Replace("<br />", "\n");
            tempString = tempString.Replace("<p>", "\n");
            tempString = tempString.Replace("'", "&#39;");

            tempString = StripHtml(tempString, false, false);
            tempString = StripBBTags(tempString);

            return tempString;
        }

        /// <summary>
        /// 清除UBB标签
        /// </summary>
        /// <param name="content">待处理的字符串</param>
        /// <remarks>处理后的字符串</remarks>
        public static string StripBBTags(string content)
        {
            return Regex.Replace(content, @"\[[^\]]*?\]", string.Empty, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 移除script标签
        /// Helper function used to ensure we don't inject script into the db.
        /// </summary>
        /// <remarks>
        /// 移除&lt;script&gt;及javascript:
        /// </remarks>
        /// <param name="rawString">待处理的字符串</param>
        /// <remarks>处理后的字符串</remarks>
        public static string StripScriptTags(string rawString)
        {
            // Perform RegEx
            rawString = Regex.Replace(rawString, "<script((.|\n)*?)</script>", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            rawString = rawString.Replace("\"javascript:", "\"");

            return rawString;
        }

        /// <summary>
        /// 闭合未闭合的Html标签
        /// </summary>
        /// <returns></returns>
        public static string CloseHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            HtmlDocument doc = new HtmlDocument() { OptionAutoCloseOnEnd = true, OptionWriteEmptyNodes = true };
            doc.LoadHtml(html);

            return doc.DocumentNode.WriteTo();
        }

        #region Clean Html

        /// <summary>
        /// Html标签过滤/清除
        /// </summary>
        /// <remarks>需要在Starter中注册TrustedHtml类，也可以通过重写Basic与HtmlEditor方法来自定义过滤规则</remarks>
        /// <param name="rawHtml">需要处理的Html字符串</param>
        /// <param name="level">受信任Html标签严格程度</param>
        public static string CleanHtml(string rawHtml, TrustedHtmlLevel level)
        {
            if (string.IsNullOrEmpty(rawHtml))
                return rawHtml;

            HtmlDocument doc = new HtmlDocument() { OptionAutoCloseOnEnd = true, OptionWriteEmptyNodes = true };

            TrustedHtml trustedHtml = DIContainer.Resolve<TrustedHtml>();
            switch (level)
            {
                case TrustedHtmlLevel.Basic:
                    trustedHtml = trustedHtml.Basic();
                    break;
                case TrustedHtmlLevel.HtmlEditor:
                    trustedHtml = trustedHtml.HtmlEditor();
                    break;
            }

            doc.LoadHtml(rawHtml);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*");

            if (nodes != null)
            {

                string host = string.Empty;
                if (HttpContext.Current != null)
                    host = WebUtility.HostPath(HttpContext.Current.Request.Url);
                Dictionary<string, string> enforcedAttributes;
                nodes.ToList().ForEach(n =>
                {
                    if (trustedHtml.IsSafeTag(n.Name))
                    {
                        //过滤属性
                        n.Attributes.ToList().ForEach(attr =>
                        {
                            if (!trustedHtml.IsSafeAttribute(n.Name, attr.Name, attr.Value))
                                attr.Remove();
                            else if (attr.Value.StartsWith("javascirpt:", StringComparison.InvariantCultureIgnoreCase))
                                attr.Value = "javascirpt:;";
                        });

                        //为标签增加强制添加的属性
                        enforcedAttributes = trustedHtml.GetEnforcedAttributes(n.Name);
                        if (enforcedAttributes != null)
                        {
                            foreach (KeyValuePair<string, string> attr in enforcedAttributes)
                            {
                                if (!n.Attributes.Select(a => a.Name).Contains(attr.Key))
                                {
                                    n.Attributes.Add(attr.Key, attr.Value);
                                }
                                else
                                {
                                    n.Attributes[attr.Key].Value = attr.Value;
                                }
                            }
                        }

                        if (n.Name == "a")
                        {
                            if (n.Attributes.Contains("href"))
                            {
                                string href = n.Attributes["href"].Value;

                                if (href.StartsWith("http://") && !href.ToLowerInvariant().StartsWith(host.ToLower()))
                                {
                                    if (!n.Attributes.Select(a => a.Name).Contains("rel"))
                                    {
                                        n.Attributes.Add("rel", "nofollow");
                                    }
                                    else if (n.Attributes["rel"].Value != "fancybox")
                                    {
                                        n.Attributes["rel"].Value = "nofollow";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (trustedHtml.EncodeHtml)
                            n.HtmlEncode = true;
                        else
                            n.RemoveTag();//移除不允许的Html标签
                    }

                });
            }

            return doc.DocumentNode.WriteTo();
        }

        #endregion

        #region Get HtmlNode

        /// <summary>
        /// 选择单个Html节点
        /// </summary>
        /// <remarks>选择节点时会自动闭合未闭合的标签</remarks>
        /// <param name="html">要操作的html</param>
        /// <param name="xpath">要选择Html元素的XPath</param>
        public static string GetHtmlNode(string html, string xpath)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            HtmlDocument doc = new HtmlDocument() { OptionAutoCloseOnEnd = true, OptionWriteEmptyNodes = true };
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode(xpath);

            if (node == null)
                return string.Empty;

            return node.OuterHtml;
        }

        /// <summary>
        /// 选择多个Html节点
        /// </summary>
        /// <remarks>选择节点时会自动闭合未闭合的标签</remarks>
        /// <param name="html">要操作的Html</param>
        /// <param name="xpath">要选择Html元素的XPath</param>
        public static List<string> GetHtmlNodes(string html, string xpath)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            HtmlDocument doc = new HtmlDocument() { OptionAutoCloseOnEnd = true, OptionWriteEmptyNodes = true };
            doc.LoadHtml(html);

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(xpath);

            if (nodes == null)
                return null;

            return nodes.Select(n => n.OuterHtml).ToList();
        }

        #endregion

        #region BBCodeParser

        /// <summary>
        /// 将内容中的BBCode转换为对应的Html标签
        /// </summary>
        /// <param name="rawString">需要处理的字符串</param>
        /// <param name="bbTag">bbTag实体</param>
        /// <param name="htmlEncode">是否进行htmlEncode</param>
        /// <include file='BBCodeToHtml.xml' path='doc/members/member[@name="M:Tunynet.Utilities.HtmlUtility.BBCodeToHtml"]/example'/>
        public static string BBCodeToHtml(string rawString, BBTag bbTag, bool htmlEncode = false)
        {
            if (string.IsNullOrEmpty(rawString))
                return rawString;

            return BBCodeToHtml(rawString, new List<BBTag>() { bbTag }, htmlEncode);
        }

        /// <summary>
        /// 将内容中的BBCode转换为对应的Html标签
        /// </summary>
        /// <param name="rawString">需要处理的字符串</param>
        /// <param name="bbTags">bbTag实体集合</param>
        /// <param name="htmlEncode">是否进行htmlEncode</param>
        /// <include file='BBCodeToHtml.xml' path='doc/members/member[@name="M:Tunynet.Utilities.HtmlUtility.BBCodeToHtml"]/example'/>
        public static string BBCodeToHtml(string rawString, IList<BBTag> bbTags, bool htmlEncode = false)
        {
            if (string.IsNullOrEmpty(rawString) || bbTags == null)
                return rawString;

            var parser = new BBCodeParser(bbTags);
            return parser.ToHtml(rawString, htmlEncode);
        }

        #endregion

    }
}
