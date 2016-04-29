//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using System;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答权限验证
    /// </summary>
    public static class AuthorizerExtension
    {
        /// <summary>
        /// 创建问题
        /// </summary>
        public static bool Question_Create(this Authorizer authorizer)
        {
            string errorMessage = string.Empty;
            return authorizer.Question_Create(out errorMessage);
        }

        /// <summary>
        /// 创建问题
        /// </summary>
        /// <remarks>
        /// 登录用户可以创建问题
        /// </remarks>
        public static bool Question_Create(this Authorizer authorizer, out string errorMessage)
        {
            errorMessage = "没有权限提问";
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                errorMessage = "您需要先登录，才能创建问题";
                return false;
            }
            bool result = authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().Ask_Create());
            if (!result && currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreateQuestionDenied;
            return result;
        }

        /// <summary>
        /// 编辑问题/取消问题
        /// </summary>
        /// <remarks>
        /// 1.如果问题未解决（答案被采纳前），提问者及管理员都可以对问题进行编辑，如果修改悬赏分值仅允许追加悬赏分值
        /// 2.如果问题已解决（答案被采纳后），则仅允许管理员对问题进行编辑，不允许修改悬赏分值
        /// </remarks>
        public static bool Question_Edit(this Authorizer authorizer, AskQuestion question)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }

            //如果问题未解决
            if (question.Status == QuestionStatus.Unresolved)
            {
                if (question.UserId == currentUser.UserId || authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
                {
                    return true;
                }
            }
            //如果问题已解决
            if (question.Status == QuestionStatus.Resolved)
            {
                if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <remarks>
        /// 管理员可以删除问题
        /// </remarks>
        public static bool Question_Delete(this Authorizer authorizer, AskQuestion question)
        {
            if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将问题设置为精华或取消精华
        /// </summary>
        /// <remarks>
        /// 管理员可以对问题设为精华或取消精华
        /// </remarks>
        public static bool Question_SetEssential(this Authorizer authorizer)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }
            if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 创建问题
        /// </summary>
        public static bool Answer_Create(this Authorizer authorizer, AskQuestion question)
        {
            string errorMessage = string.Empty;
            return authorizer.Answer_Create(question, out errorMessage);
        }


        /// <summary>
        /// 创建回答
        /// </summary>
        /// <remarks>
        /// 1.登录用户可以回答问题
        /// 2.问题提问者不得回答自己的问题
        /// 3.每个回答者针对每个问题只能回答一次
        /// </remarks>
        public static bool Answer_Create(this Authorizer authorizer, AskQuestion question, out string errorMessage)
        {
            errorMessage = string.Empty;
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                errorMessage = "您需要先登录，才能回答";
                return false;
            }
            if (question.UserId == currentUser.UserId)
            {
                errorMessage = "提问者不可以回答自己的问题";
                return false;
            }
            if (question.Status != QuestionStatus.Unresolved)
            {
                errorMessage = "只有未解决状态的问题，才能回答";
                return false;
            }
            AskAnswer answer = new AskService().GetUserAnswerByQuestionId(currentUser.UserId, question.QuestionId);
            if (answer != null)
            {
                errorMessage = "您已经回答过一次，不可重复回答";
                return false;
            }
            bool result = authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().Ask_CreateAnswer());
            if (!result && currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreateAnswerDenied;
            return result;
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <remarks>
        /// 1.如果问题已解决（答案被采纳后），不允许回答者编辑，其余情况允许回答者编辑
        /// 2.管理员可以编辑所有的回答
        /// </remarks>
        public static bool Answer_Edit(this Authorizer authorizer, AskQuestion question, AskAnswer answer)
        {
            if (question.Status == QuestionStatus.Unresolved)
            {
                if (UserContext.CurrentUser != null && UserContext.CurrentUser.UserId == answer.UserId)
                {
                    return true;
                }
            }
            if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除回答
        /// </summary>
        /// <remarks>
        /// 管理员可以删除所有回答
        /// </remarks>
        public static bool Answer_Delete(this Authorizer authorizer)
        {
            if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 管理员或用户可以修改自己的设置
        /// </summary>
        public static bool Ask_UserSetting(this Authorizer authorizer, long userId)
        {
            if (userId == 0)
            {
                return false;
            }
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }
            if (currentUser.UserId == userId)
            {
                return true;
            }
            if (authorizer.IsAdministrator(AskConfig.Instance().ApplicationId))
            {
                return true;
            }

            return false;
        }
    }
}