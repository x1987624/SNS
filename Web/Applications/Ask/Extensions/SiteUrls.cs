//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Web.Routing;
using Spacebuilder.Ask;
using Tunynet.Common;

using Tunynet.Utilities;
using System.Collections.Generic;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 问答链接管理
    /// </summary>
    public static class SiteUrlsExtension
    {
        private static readonly string AskAreaName = AskConfig.Instance().ApplicationKey;

        #region 问答操作

        /// <summary>
        /// 问题
        /// </summary>
        public static string AskQuestions(this SiteUrls siteUrls, string tab = null, string tagName = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(tab))
            {
                routeValueDictionary.Add("tab", tab);
            }
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("Questions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 标签浮动内容块
        /// </summary>
        public static string _AskTagContents(this SiteUrls siteUrls, string tagName)
        {
            return CachedUrlHelper.Action("_TagContents", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')) } });
        }

        /// <summary>
        /// 发布/编辑问答
        /// </summary>
        public static string AskQuestionEdit(this SiteUrls siteUrls, long? questionId)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (questionId.HasValue)
            {
                routeValueDictionary.Add("questionId", questionId);
            }

            return CachedUrlHelper.Action("EditQuestion", "ChannelAsk", AskAreaName, routeValueDictionary);
        }


        /// <summary>
        /// 取消关注问题
        /// </summary>
        public static string _AskSubscribeQuestionCancel(this SiteUrls siteUrls, long questionId)
        {
            return CachedUrlHelper.Action("_SubscribeQuestionCancel", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "questionId", questionId } });
        }

        /// <summary>
        /// 关注问题
        /// </summary>
        public static string _AskSubscribeQuestion(this SiteUrls siteUrls, long questionId)
        {
            return CachedUrlHelper.Action("_SubscribeQuestion", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "questionId", questionId } });
        }

        /// <summary>
        /// 取消关注标签
        /// </summary>
        public static string _AskSubscribeTagCancel(this SiteUrls siteUrls, long tagId)
        {
            return CachedUrlHelper.Action("_SubscribeTagCancel", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "tagId", tagId } });
        }

        /// <summary>
        /// 关注标签
        /// </summary>
        public static string _AskSubscribeTag(this SiteUrls siteUrls, long tagId)
        {
            return CachedUrlHelper.Action("_SubscribeTag", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "tagId", tagId } });
        }

        /// <summary>
        /// 设置精华/取消精华
        /// </summary>
        public static string _AskSetEssential(this SiteUrls siteUrls, long questionId, bool isEssential)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "questionId", questionId }, { "isEssential", isEssential } };

            return CachedUrlHelper.Action("_SetEssential", "ChannelAsk", AskAreaName, dic);
        }


        /// <summary>
        /// 取消
        /// </summary>
        public static string _AskCancel(this SiteUrls siteUrls, long questionId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "questionId", questionId } };

            return CachedUrlHelper.Action("_Cancel", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 问题后台管理
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="auditStatus">审核状态</param>
        public static string AskQuestionControlPanelManage(this SiteUrls siteUrls, AuditStatus? auditStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (auditStatus.HasValue)
            {
                dic.Add("auditStatus", auditStatus);
            }

            return CachedUrlHelper.Action("ManageQuestions", "ControlPanelAsk", AskAreaName, dic);

        }

        /// <summary>
        /// 删除回答
        /// </summary>
        public static string _AskDeleteAnswer(this SiteUrls siteUrls, long answerId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "answerId", answerId } };

            return CachedUrlHelper.Action("_DeleteAnswer", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 采纳为满意答案
        /// </summary>
        public static string _AskSetBestAnswer(this SiteUrls siteUrls, long answerId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "answerId", answerId } };

            return CachedUrlHelper.Action("_SetBestAnswer", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 批量/单个设置精华
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isEssential">是否精华</param>
        /// <returns>批量设置精华</returns>
        public static string _AskControlPanelSetEssential(this SiteUrls siteUrls, bool isEssential)
        {
            return CachedUrlHelper.Action("_SetEssential", "ControlPanelAsk", AskAreaName, new RouteValueDictionary { { "isEssential", isEssential } });
        }

        /// <summary>
        /// 批量/单个删除问题
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="questionsIds">问题ID列表</param>
        public static string _AskDeleteQuestion(this SiteUrls siteUrls, long? questionIds = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (questionIds != null)
            {
                dic.Add("questionIds", questionIds);
            }
            return CachedUrlHelper.Action("_DeleteQuestion", "ControlPanelAsk", AskAreaName, dic);
        }


        /// <summary>
        /// 更新问题的审核状态
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApprove">审核状态</param>
        public static string _ApproveQuestion(this SiteUrls siteUrls, bool isApprove)
        {
            return CachedUrlHelper.Action("_ApproveQuestion", "ControlPanelAsk", AskAreaName, new RouteValueDictionary() { { "isApprove", isApprove } });
        }

        /// <summary>
        /// 更新问题的审核状态
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApprove">审核状态</param>
        public static string _ApproveQuestion(this SiteUrls siteUrls, long questionId, bool isApprove)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("questionId", questionId);
            dic.Add("isApprove", isApprove);
            return CachedUrlHelper.Action("_ApproveQuestione", "ControlPanelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 批量/单个删除回答
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="answerIds">回答ID列表</param>
        public static string _DeleteAnswer(this SiteUrls siteUrls, long? answerIds = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (answerIds != null)
            {
                dic.Add("answerIds", answerIds);
            }
            return CachedUrlHelper.Action("_DeleteAnswer", "ControlPanelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <param name="answerId"></param>
        /// <returns></returns>
        public static string _EditAnswer(this SiteUrls siteUrls,long answerId)
        {
            return CachedUrlHelper.Action("_EditAnswer", "ControlPanelAsk", AskAreaName, new RouteValueDictionary { { "answerId", answerId } });
        }

        /// <summary>
        /// 更新回答的审核状态
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApprove">审核状态</param>
        public static string _ApproveAnswer(this SiteUrls siteUrls, bool isApprove)
        {
            return CachedUrlHelper.Action("_ApproveAnswer", "ControlPanelAsk", AskAreaName, new RouteValueDictionary() { { "isApprove", isApprove } });
        }

        /// <summary>
        /// 回答后台管理
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="auditStatus">审核状态</param>
        public static string AskAnswerControlPanelManage(this SiteUrls siteUrls, AuditStatus? auditStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (auditStatus.HasValue)
            {
                dic.Add("auditStatus", auditStatus);
            }

            return CachedUrlHelper.Action("ManageAnswers", "ControlPanelAsk", AskAreaName, dic);

        }
        #endregion

        #region 问答频道

        /// <summary>
        /// 主页
        /// </summary>
        public static string AskHome(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Home", "ChannelAsk", AskAreaName);
        }

        /// <summary>
        /// 待解决问题
        /// </summary>
        public static string _AskUnresolvedQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_UnresolvedQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 精华问题
        /// </summary>
        public static string _AskEssentialQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_EssentialQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 高分悬赏
        /// </summary>
        public static string _AskHighRewardQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_HighRewardsQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 标签分组
        /// </summary>
        public static string AskTags(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Tags", "ChannelAsk", AskAreaName);
        }

        /// <summary>
        /// 标签分组下标签
        /// </summary>
        public static string _AskTagList(this SiteUrls siteUrls, long groupId)
        {
            return CachedUrlHelper.Action("_TagList", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "groupId", groupId } });
        }

        /// <summary>
        /// 标签详细
        /// </summary>
        public static string AskTagDetail(this SiteUrls siteUrls, string tagName)
        {

            return CachedUrlHelper.Action("TagDetail", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')) } });
        }

        /// <summary>
        /// 关注标签的用户全局页
        /// </summary>
        public static string AskTagFollowers(this SiteUrls siteUrls, long tagId)
        {
            return CachedUrlHelper.Action("TagFollowers", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "tagId", tagId } });
        }


        /// <summary>
        /// 最新问题
        /// </summary>
        public static string _AskLatestQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_LatestQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 零回答问题
        /// </summary>
        public static string _AskNoAnswerQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_NoAnswerQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 已解决的问题
        /// </summary>
        public static string _AskResolvedQuestions(this SiteUrls siteUrls, string tagName = null, int pageSize = 10)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            if (!string.IsNullOrEmpty(tagName))
            {
                routeValueDictionary.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("_ResolvedQuestions", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 关注问题的用户
        /// </summary>
        public static string AskQuestionFollowers(this SiteUrls siteUrls, long questionId)
        {
            return CachedUrlHelper.Action("QuestionFollowers", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "questionId", questionId } });
        }

        /// <summary>
        /// 关注问题的用户列表
        /// </summary>
        public static string _AskQuestionFollowers(this SiteUrls siteUrls, long questionId)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("questionId", questionId);

            return CachedUrlHelper.Action("_QuestionFollowers", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 关注问题的用户列表
        /// </summary>
        public static string _AskTagFollowers(this SiteUrls siteUrls, long tagId)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("tagId", tagId);

            return CachedUrlHelper.Action("_TagFollowers", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 排行
        /// </summary>
        public static string AskRank(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Rank", "ChannelAsk", AskAreaName);
        }
        /// <summary>
        /// 问题详细显示页
        /// </summary>
        public static string AskQuestionDetail(this SiteUrls siteUrls, long questionId)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("questionId", questionId);

            return CachedUrlHelper.Action("QuestionDetail", "ChannelAsk", AskAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 问题的回答列表
        /// </summary>
        public static string _AskAnswerList(this SiteUrls siteUrls, long questionId, SortBy_AskAnswer? sortBy,long pageIndex=1)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "questionId", questionId },{"pageIndex",pageIndex} };
            if (sortBy.HasValue)
            {
                dic.Add("sortBy", sortBy);
            }

            return CachedUrlHelper.Action("_AnswerList", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 我的问答
        /// </summary>
        public static string AskUser(this SiteUrls siteUrls, string spaceKey)
        {
            return CachedUrlHelper.Action("AskUser", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "spaceKey", spaceKey } });
        }
        /// <summary>
        /// 我的设置
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _AskUserSettings(this SiteUrls siteUrls, long userId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "userId", userId } };
            return CachedUrlHelper.Action("_AskUserSettings", "ChannelAsk", AskAreaName, dic);
        }
        /// <summary>
        /// 他的提问
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _AskTa(this SiteUrls siteUrls, long userId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "userId", userId } };
            return CachedUrlHelper.Action("_AskTa", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 我的回答
        /// </summary>
        /// <returns></returns>
        public static string _AskUserAnswers(this SiteUrls siteUrls, long userId, int pageSize = 10)
        {
            return CachedUrlHelper.Action("_UserAnswers", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "userId", userId }, { "pageSize", pageSize } });
        }
        /// <summary>
        /// 我的问题
        /// </summary>
        public static string _AskUserQuestions(this SiteUrls siteUrls, long userId, int pageSize = 10)
        {
            return CachedUrlHelper.Action("_UserQuestions", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "userId", userId }, { "pageSize", pageSize } });
        }
        /// <summary>
        /// 用户关注的问题
        /// </summary>
        public static string _AskUserFollowedQuestions(this SiteUrls siteUrls, long userId, int pageSize = 10)
        {
            return CachedUrlHelper.Action("_UserFollowedQuestions", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "userId", userId }, { "pageSize", pageSize } });
        }

        /// <summary>
        /// 用户关注的标签
        /// </summary>
        public static string _AskUserFollowedTags(this SiteUrls siteUrls, long userId, int pageSize = 10)
        {
            return CachedUrlHelper.Action("_UserFollowedTags", "ChannelAsk", AskAreaName, new RouteValueDictionary { { "userId", userId }, { "pageSize", pageSize } });
        }
        /// <summary>
        /// 保存简介
        /// </summary>
        /// <returns></returns>
        public static string _AskSaveUserDescription(this SiteUrls siteUrls, long userId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "userId", userId } };
            return CachedUrlHelper.Action("_SaveUserDescription", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <returns></returns>
        public static string _AskEditAskAnswerControlPanel(this SiteUrls siteUrls, long? AnswerId)
        {
            RouteValueDictionary dic = new RouteValueDictionary { { "AnswerId", AnswerId } };
            return CachedUrlHelper.Action("_EditAnswer", "ControlPanelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 相似问题
        /// </summary>
        /// <returns></returns>
        public static string _AskSimilarQuestions(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_SimilarQuestions", "ChannelAsk", AskAreaName);
        }

        #endregion

        #region 搜索
        /// <summary>
        /// 问答全局搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _AskGlobalSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_GlobalSearch", "ChannelAsk", AskAreaName);
        }

        /// <summary>
        /// 问答快捷搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _AskQuickSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_QuickSearch", "ChannelAsk", AskAreaName);
        }

        /// <summary>
        /// 问答搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string AskPageSearch(this SiteUrls siteUrls, string keyword = "")
        {
            keyword = WebUtility.UrlEncode(keyword);
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(keyword))
            {
                dic.Add("keyword", keyword);
            }
            return CachedUrlHelper.Action("Search", "ChannelAsk", AskAreaName, dic);
        }

        /// <summary>
        /// 问答搜索自动完成
        /// </summary>
        public static string AskSearchAutoComplete(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("SearchAutoComplete", "ChannelAsk", AskAreaName);
        }
        #endregion
    }
}
