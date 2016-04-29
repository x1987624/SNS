
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Tunynet;
using Tunynet.Common;
using Tunynet.Utilities;
using Tunynet.UI;

using System.Text.RegularExpressions;
using Spacebuilder.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科相关词条提取类
    /// </summary>
    public static class WikiExtractEntryUtilities
    {

        private static Dictionary<string, TagBuilder> WikiPages = null;
        private static byte[] fastCheck = new byte[char.MaxValue];
        private static BitArray charCheck = new BitArray(char.MaxValue);
        private static string strCensorWords = string.Empty;
        private static int maxWordLength = 0;
        private static int minWordLength = int.MaxValue;
        static WikiService wikiService = new WikiService();

        private static IEnumerable<WikiPage> wordToReplaces = null;
        //new string[] { "词条1词条2词条3词条4" }.ToList();
        static string tags = "classidstyletitledirlanqxmlaccesskeytabindexplaceholderindexaspanhtmlbodytitlepbrblockquotedldtddolullidivpreemh1h2h3h4h5h6ttcitystrongimghrtabletrtdthforminputselectoptiontextareabcdefghigklmnopqrstuvwxyz";



        /// <summary>
        /// 提取产品的百科词条
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string WikiExtractEntry(this string content)
        {
            return WikiExtractEntry(content, string.Empty);
        }

        /// <summary>
        /// 提取产品的百科词条
        /// </summary>
        /// <param name="content"></param>
        /// <param name="currentPage">当前名称,可以为空</param>
        /// <returns></returns>
        public static string WikiExtractEntry(this string content, string currentPage)
        {

            if (string.IsNullOrEmpty(content))
                return content;

            //提取出文本里的a标签
            Dictionary<string, string> aTagBuilder = new Dictionary<string, string>();
            Regex regex = new Regex("<a.*?</a>");//new Regex(@"style=\\.*?\"">")
            MatchCollection matchStrs = regex.Matches(content);
            int PlaceholderIndex = 0;
            foreach (System.Text.RegularExpressions.Match item in matchStrs)
            {
                if (!aTagBuilder.Values.Contains(item.Value))
                {
                    string key = "<==Place-holder-Index" +PlaceholderIndex +"==>";
                    string value = item.Value;
                    aTagBuilder.Add(key, value);
                   content= content.Replace(value, key);
                   PlaceholderIndex++;
                }
            }

            //提取出文本里的style
            //Dictionary<string, string> styleTagBuilder = new Dictionary<string, string>();
            Regex styleRegex = new Regex(@"style=\\.*?\"">");
            MatchCollection styleMatchStrs = styleRegex.Matches(content);
            //int stylePlaceholderIndex = 0;
            foreach (System.Text.RegularExpressions.Match item in styleMatchStrs)
            {
                if (!aTagBuilder.Values.Contains(item.Value))
                {
                    string key = "<==Place-holder-Index" + PlaceholderIndex + "==>";
                    string value = item.Value;
                    aTagBuilder.Add(key, value);
                    content = content.Replace(value, key);
                    PlaceholderIndex++;
                }
            }

            GetAllWikiPages();

            string formattingPost = content;

            List<string> needReplacedSub = new List<string>();

            int index = 0;
            for (index = 0; index < formattingPost.Length; index++)
            {
                if ((fastCheck[formattingPost[index]] & 1) == 0)
                {
                    while (index < formattingPost.Length - 1 && (fastCheck[formattingPost[++index]] & 1) == 0) ;
                }

                //单字节检测
                if (minWordLength == 1 && charCheck[formattingPost[index]])
                {
                    needReplacedSub.Add(formattingPost[index].ToString());
                    continue;
                }
                //多字节检测
                for (int j = 1; j <= Math.Min(maxWordLength, formattingPost.Length - index - 1); j++)
                {
                    //快速排除
                    if ((fastCheck[formattingPost[index + j]] & (1 << Math.Min(j, 7))) == 0)
                    {
                        break;
                    }

                    if (j + 1 >= minWordLength)
                    {
                        string sub = formattingPost.Substring(index, j + 1);

                        if (WikiPages.ContainsKey(sub))
                        {
                            needReplacedSub.Add(sub);
                            //记录新位置
                            index += j;
                            break;
                        }
                    }
                }
            }

            List<string> _needReplacedSub = needReplacedSub.Distinct().ToList();
            _needReplacedSub.Remove(currentPage);

            //定义一个字典,存放单字的不需要替换的百科词条
            Dictionary<string, string> simpleWiki = new Dictionary<string, string>();
            bool IsSimpleWiki = false;

            for (int i = 0; i < _needReplacedSub.Count; i++)
            {
                //判断要替换的词条title里有没有接下来要替换的词条,保证标签结构不被破坏
                bool IsCanReplace = true;

                string replaceString = WikiPages[_needReplacedSub[i]].ToString();
                for (int k = i + 1; k < _needReplacedSub.Count; k++)
                {

                    if (replaceString.Contains(_needReplacedSub[k]) == true)
                    {
                        //如果有,不能直接替换
                        IsCanReplace = false;
                        //如果字符串里可以插入字符串
                        if (_needReplacedSub[k].Length > 1)
                        {
                            //破坏掉title中的不需要被替换的词条
                            string _replaceStringFormat = _needReplacedSub[k].Insert(1, "<Place-holder>");
                            replaceString = replaceString.Replace(_needReplacedSub[k], _replaceStringFormat);
                        }
                        else
                        {
                            //如果是一个字的词条,如"差"
                            IsSimpleWiki = true;
                            replaceString = replaceString.Replace(_needReplacedSub[k], ChineseToPinYin.ToPinYin(_needReplacedSub[k]));
                            try
                            {
                                simpleWiki.Add(ChineseToPinYin.ToPinYin(_needReplacedSub[k]), _needReplacedSub[k]);
                            }
                            catch { }
                        }
                    }
                }
                if (!IsCanReplace)
                {
                    formattingPost = formattingPost.Replace(_needReplacedSub[i], replaceString);
                    continue;
                }
                formattingPost = formattingPost.Replace(_needReplacedSub[i], WikiPages[_needReplacedSub[i]].ToString());
            }

            //替换出都有用拼音占用的
            if (IsSimpleWiki)
            {
                foreach (string item in simpleWiki.Keys.Reverse())
                {
                    formattingPost = formattingPost.Replace(item, simpleWiki[item]);
                }
            }
            //替换出所以a标签
            foreach (string item in aTagBuilder.Keys.Reverse())
            {
                formattingPost = formattingPost.Replace(item, aTagBuilder[item]);                
            }

            return formattingPost.Replace("<Place-holder>", string.Empty);
        }


        /// <summary>
        /// 获取所有词条
        /// </summary>
        private static void GetAllWikiPages()
        {

            WikiPages = new Dictionary<string, TagBuilder>();
            wordToReplaces = wikiService.GetTopsForExtractEntry(TenantTypeIds.Instance().Wiki(), int.MaxValue, null, null, SortBy_WikiPage.StageHitTimes).ToList();//.Select(n => n.Title);
            if (wordToReplaces != null && wordToReplaces.Count() > 0)
            {
                foreach (WikiPage wordAndReplace in wordToReplaces)
                {
                    if (tags.IndexOf(wordAndReplace.Title.ToLower()) >= 0)
                        continue;

                    string word = wordAndReplace.Title;//wordSplitArray[0];
                    TagBuilder a = new TagBuilder("a");
                    string hrefUrl = CachedUrlHelper.Action("PageDetail", "ChannelWiki", "Wiki", new System.Web.Routing.RouteValueDictionary { { "pageId", wordAndReplace.PageId } });
                    a.MergeAttribute("href", hrefUrl);
                    a.MergeAttribute("target","_blank");
                    a.MergeAttribute("title", "“" + HtmlUtility.TrimHtml(wordAndReplace.ResolvedBody, 300).Replace("\"", "'").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n") + "”");
                    a.InnerHtml = wordAndReplace.Title;
                    
                    maxWordLength = Math.Max(maxWordLength, word.Length);
                    minWordLength = Math.Min(minWordLength, word.Length);
                    for (int i = 0; i < 7 && i < word.Length; i++)
                    {
                        fastCheck[word[i]] |= (byte)(1 << i);
                    }

                    for (int i = 7; i < word.Length; i++)
                    {
                        fastCheck[word[i]] |= 0x80;
                    }

                    if (word.Length == 1)
                    {
                        charCheck[word[0]] = true;
                    }

                    WikiPages[word] = a;
                }
            }

        }



        /// <summary>
        /// 返回文本中出现的词条
        /// </summary>
        /// <param name="content"></param>
        /// <param name="currentPage">当前名称,可以为空</param>
        /// <returns></returns>
        public static List<string> WikiExtractEntrys(this string content, int? topNumber=null)
        {

            if (string.IsNullOrEmpty(content))
                return null;

            //提取出文本里的a标签
            Dictionary<string, string> aTagBuilder = new Dictionary<string, string>();
            Regex regex = new Regex("<a.*?</a>");//new Regex(@"style=\\.*?\"">")
            MatchCollection matchStrs = regex.Matches(content);
            int PlaceholderIndex = 0;
            foreach (System.Text.RegularExpressions.Match item in matchStrs)
            {
                if (!aTagBuilder.Values.Contains(item.Value))
                {
                    string key = "<==Place-holder-Index" + PlaceholderIndex + "==>";
                    string value = item.Value;
                    aTagBuilder.Add(key, value);
                    content = content.Replace(value, key);
                    PlaceholderIndex++;
                }
            }

            //提取出文本里的style
            //Dictionary<string, string> styleTagBuilder = new Dictionary<string, string>();
            Regex styleRegex = new Regex(@"style=\\.*?\"">");
            MatchCollection styleMatchStrs = styleRegex.Matches(content);
            //int stylePlaceholderIndex = 0;
            foreach (System.Text.RegularExpressions.Match item in styleMatchStrs)
            {
                if (!aTagBuilder.Values.Contains(item.Value))
                {
                    string key = "<==Place-holder-Index" + PlaceholderIndex + "==>";
                    string value = item.Value;
                    aTagBuilder.Add(key, value);
                    content = content.Replace(value, key);
                    PlaceholderIndex++;
                }
            }

            GetAllWikiPages();

            string formattingPost = content;

            List<string> needReplacedSub = new List<string>();

            int index = 0;
            for (index = 0; index < formattingPost.Length; index++)
            {
                if ((fastCheck[formattingPost[index]] & 1) == 0)
                {
                    while (index < formattingPost.Length - 1 && (fastCheck[formattingPost[++index]] & 1) == 0) ;
                }

                //单字节检测
                if (minWordLength == 1 && charCheck[formattingPost[index]])
                {
                    needReplacedSub.Add(formattingPost[index].ToString());
                    continue;
                }
                //多字节检测
                for (int j = 1; j <= Math.Min(maxWordLength, formattingPost.Length - index - 1); j++)
                {
                    //快速排除
                    if ((fastCheck[formattingPost[index + j]] & (1 << Math.Min(j, 7))) == 0)
                    {
                        break;
                    }

                    if (j + 1 >= minWordLength)
                    {
                        string sub = formattingPost.Substring(index, j + 1);

                        if (WikiPages.ContainsKey(sub))
                        {
                            needReplacedSub.Add(sub);
                            //记录新位置
                            index += j;
                            break;
                        }
                    }
                }
            }

            List<string> _needReplacedSub = needReplacedSub.Distinct().ToList();
            
            //带链接的词条集合
            List<string> wikis = null;

            if (_needReplacedSub != null && _needReplacedSub.Count > 0)
            {
                wikis = new List<string>();
                int forCount=_needReplacedSub.Count;
                if (topNumber.HasValue&&topNumber.Value>0)
                    forCount = topNumber.Value;
                for (int i = 0; i < _needReplacedSub.Count; i++)
                {
                    wikis.Add(WikiPages[_needReplacedSub[i]].ToString());
                    //formattingPost = formattingPost.Replace(_needReplacedSub[i], WikiPages[_needReplacedSub[i]].ToString());
                }
            }
            return wikis;
        }
    }
}
