//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate></createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-09" version="0.5">新建</log>
//<log date="2012-08-10" version="0.6" author="libsh">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Spacebuilder.Common;
using Tunynet.Utilities;
using Spacebuilder.MobileClient.Common;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 微博实体
    /// </summary>
    [Serializable]
    [TableName("spb_Microblogs")]
    [PrimaryKey("MicroblogId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,OwnerId")]
    public class MicroblogEntity : IEntity, IAuditable
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        //todo:需要检查成员初始化的类型是否正确
        public static MicroblogEntity New()
        {
            MicroblogEntity microblog = new MicroblogEntity()
            {
                Author = string.Empty,
                Body = string.Empty,
                PostWay = PostWay.Web,
                DateCreated = DateTime.UtcNow,
                IP = WebUtility.GetIP()
                
            };

            return microblog;
        }

        #region 需持久化属性

        /// <summary>
        ///标识列  //todo:yangmj20120829
        /// </summary>
        public long MicroblogId { get; set; }

        /// <summary>
        ///微博作者UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///微博作者DisplayName
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///微博拥有者Id（例如：群组Id）
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///原文微博Id
        /// </summary>
        public long OriginalMicroblogId { get; set; }

        /// <summary>
        ///转发微博Id（在转发非原文微博时使用）
        /// </summary>
        public long ForwardedMicroblogId { get; set; }

        /// <summary>
        ///微博内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///被转发数统计
        /// </summary>
        public int ForwardedCount { get; set; }

        /// <summary>
        ///是否包含图片
        /// </summary>
        public bool HasPhoto { get; set; }

        /// <summary>
        ///是否包含视频
        /// </summary>
        public bool HasVideo { get; set; }

        /// <summary>
        ///是否包含音乐
        /// </summary>
        public bool HasMusic { get; set; }

        /// <summary>
        ///发布途径
        /// </summary>
        public PostWay PostWay { get; set; }

        /// <summary>
        ///微博来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///微博来源的访问地址
        /// </summary>
        public string SourceUrl { get; set; }



        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }

        #endregion 需持久化属性

        #region 扩展属性

        /// <summary>
        ///回复数
        /// </summary>
        [Ignore]
        public int ReplyCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().Microblog());
                return countService.Get(CountTypes.Instance().CommentCount(), this.MicroblogId);
            }
        }


        /// <summary>
        /// 被转发的微博    
        /// </summary>
        [Ignore]
        public MicroblogEntity OriginalMicroblog
        {
            get
            {
                long microblogId = OriginalMicroblogId > 0 ? OriginalMicroblogId : ForwardedMicroblogId;
                if (microblogId <= 0)
                    return null;
                MicroblogEntity entity = DIContainer.Resolve<MicroblogService>().Get(microblogId);
                return entity;
            }

        }

        /// <summary>
        /// 微博作者    
        /// </summary>
        [Ignore]
        public IUser User
        {
            get
            {
                IUserService service = DIContainer.Resolve<IUserService>();
                IUser entity = service.GetUser(UserId);
                return entity;
            }
        }

        /// <summary>
        /// 是否为转发微博
        /// </summary>
        [Ignore]
        public bool IsForward
        {
            get { return OriginalMicroblogId > 0; }
        }

        [Ignore]
        public string ImageUrl { get; set; }

        [Ignore]
        public int ImageWidth { get; set; }

        [Ignore]
        public string VideoAlias { get; set; }

        [Ignore]
        public string AudioAlias { get; set; }

        #endregion 扩展属性

        /// <summary>
        /// 审核项标识
        /// </summary>
        [Ignore]
        public string AuditItemKey
        {
            get
            {
                return AuditItemKeys.Instance().CreateMicroblog();
            }
        }

        #region IEntity 成员

        [Ignore]
        object IEntity.EntityId { get { return this.MicroblogId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region 方法

        /// <summary>
        /// 获取MicroblogThread的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new MicroblogRepository().GetResolvedBody(this.MicroblogId);
        }

        #endregion


    }
}
