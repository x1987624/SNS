//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Common;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 回答的实体
    /// </summary>
    [TableName("spb_AskAnswers")]
    [PrimaryKey("AnswerId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,QuestionId")]
    [Serializable]
    public class AskAnswer : SerializablePropertiesBase, IAuditable, IEntity
    {        
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static AskAnswer New()
        {
            AskAnswer answer = new AskAnswer()
            {
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return answer;
        }

        #region 需持久化属性

        /// <summary>
        /// 主键
        /// </summary>
        public long AnswerId { get; protected set; }

        /// <summary>
        /// 问题id
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 回答者用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 回答者DisplayName
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 补充说明
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 是否最佳回答
        /// </summary>
        public bool IsBest { get; set; }

        /// <summary>
        ///发布人IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime LastModified { get; set; }

        #endregion

        #region 扩展属性及方法

        /// <summary>
        /// 获取AskAnswer的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new AskAnswerRepository().GetResolvedBody(this.AnswerId);
        }

        /// <summary>
        /// 获取当前回答的页码
        /// </summary>
        public long GetPageIndex(long pageSize)
        {
            return new AskAnswerRepository().GetPageIndexOfCurrentAnswer(this, pageSize);
        }

        /// <summary>
        /// 获取该问题的支持票数
        /// </summary>
        public int SupportCount
        {
            get
            {
                AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().AskAnswer());
                Attitude attitude = attitudeService.Get(this.AnswerId);
                if (attitude != null)
                {
                    return attitude.SupportCount;
                }
                return 0;
            }
        }

        /// <summary>
        /// 获取该回答的问题
        /// </summary>
        public AskQuestion Question
        {
            get
            {
                AskService askService = new AskService();
                AskQuestion askQuestion = askService.GetQuestion(this.QuestionId);
                if (askQuestion != null)
                {
                    return askQuestion;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取当前回答用户
        /// </summary>
        public IUser User
        {
            get
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                IUser user = userService.GetUser(this.UserId);
                if (user != null)
                {
                    return user;
                }
                return user;
            }
        }

        /// <summary>
        /// 获取该问题的支持者
        /// </summary>
        public IEnumerable<IUser> VoteUsers
        {
            get
            {
                AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().AskAnswer());
                IEnumerable<long> userIds = attitudeService.GetTopOperatedUserIds(this.AnswerId, true, 100);
                if (userIds.Count() > 0)
                {
                    IUserService userService = DIContainer.Resolve<IUserService>();
                    return userService.GetUsers(userIds);
                }
                return null;
            }
        }

        /// <summary>
        /// 评论数
        /// </summary>
        [Ignore]
        public int CommentCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().AskAnswer());
                return countService.Get(CountTypes.Instance().CommentCount(), this.AnswerId);
            }
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.AnswerId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Ask_Answer(); }
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        #endregion
    }

    /// <summary>
    /// 问题实体的扩展类
    /// </summary>
    public static class AskAnswerExtensions
    {
        /// <summary>
        /// 将数据库中的信息转换成EditModel实体
        /// </summary>
        /// <param name="askAnswer">回答实体</param>
        /// <returns>编辑回答的EditModel</returns>
        public static AskAnswerEditModel AsEditModel(this AskAnswer askAnswer)
        {
            return new AskAnswerEditModel
            {
                AnswerId = askAnswer.AnswerId,
                QuestionId = askAnswer.QuestionId,
                Body = askAnswer.Body 
            };
        }
    }
}