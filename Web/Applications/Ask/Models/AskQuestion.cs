//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Utilities;
using Spacebuilder.Common;
using Spacebuilder.Ask.Resources;
using System.Linq;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问题实体
    /// </summary>
    [TableName("spb_AskQuestions")]
    [PrimaryKey("QuestionId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,OwnerId")]
    [Serializable]
    public class AskQuestion : SerializablePropertiesBase, IAuditable, IEntity
    {   
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static AskQuestion New()
        {
            AskQuestion question = new AskQuestion()
            {
                IsEssential = false,
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return question;
        }

        #region 需持久化属性

        /// <summary>
        /// 主键
        /// </summary>
        public long QuestionId { get; protected set; }

        /// <summary>
        /// 租户类型id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        /// 所有者id
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 提问者DisplayName
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 悬赏积分
        /// </summary>
        public int Reward { get; set; }

        /// <summary>
        /// 回答数
        /// </summary>
        public int AnswerCount { get; set; }

        /// <summary>
        /// 最后回答用户Id
        /// </summary>
        public long LastAnswerUserId { get; set; }

        /// <summary>
        /// 最后回答用户DisplayName
        /// </summary>
        public string LastAnswerAuthor { get; set; }

        /// <summary>
        /// 最后回答时间
        /// </summary>
        public DateTime? LastAnswerDate { get; set; }

        /// <summary>
        /// 状态（待解决、已解决、取消）
        /// </summary>
        public QuestionStatus Status { get; set; }

        /// <summary>
        /// 是否精华
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public bool IsEssential { get; set; }

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

        /// <summary>
        /// 最后更新者用户Id
        /// </summary>
        public long LastModifiedUserId { get; set; }

        /// <summary>
        /// 最后更新者的DisplayName
        /// </summary>
        public string LastModifier { get; set; }

        #endregion

        #region 序列化属性
        [Ignore]
        public bool IsTrated
        {
            get
            {
                return GetExtendedProperty<bool>("IsTrated",false);
            }
            set
            {
                SetExtendedProperty("IsTrated", value);
            }
        }
        #endregion
        #region 扩展属性及方法

        /// <summary>
        /// 增加的悬赏积分
        /// </summary>
        [Ignore]
        public int AddedReward { get; set; }

        /// <summary>
        /// 定向提问用户ID列表
        /// </summary>
        [Ignore]
        public long AskUserId
        {
            get
            {
                //IEnumerable<string> askUserIds = GetExtendedProperty<IEnumerable<string>>("AskUserIds");
                return GetExtendedProperty<long>("AskUserId");
            }
            set { SetExtendedProperty("AskUserId", value); }
        }

        /// <summary>
        /// 获取AskQuestion的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new AskQuestionRepository().GetResolvedBody(this.QuestionId);
        }

        /// <summary>
        /// 获取该问题的支持票数
        /// </summary>
        public int VoteCount
        {
            get
            {
                return new AskService().GetAnswersAttitudesCount(this.QuestionId,TenantTypeIds.Instance().AskAnswer());
                
            }
        }

        /// <summary>
        /// 浏览数
        /// </summary>
        [Ignore]
        public int HitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
                return countService.Get(CountTypes.Instance().HitTimes(), this.QuestionId);
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
                CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
                return countService.Get(CountTypes.Instance().CommentCount(), this.QuestionId);
            }
        }

        /// <summary>
        /// 标签
        /// </summary>
        [Ignore]
        public IEnumerable<Tag> Tags
        {
            get
            {
                TagService tagService = new TagService(TenantTypeIds.Instance().AskQuestion());
                return tagService.GetTopTagsOfItem(this.QuestionId, 100);
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
        /// 根据是否已解决返回带前缀的标题
        /// </summary>
        [Ignore]
        public string ResolvedSubject
        {
            get
            {
                return this.Status == QuestionStatus.Resolved ? this.Subject+" ["+Resource.Resolved_Subject_Prefix+"]" : this.Subject;
            }
        }

        #endregion


        #region IEntity 成员

        object IEntity.EntityId { get { return this.QuestionId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Ask_Question(); }
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        #endregion
    }

}