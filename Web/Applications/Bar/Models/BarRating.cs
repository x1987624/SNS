//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>bianchx</author>
//<email>bianchx@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖子评分
    /// </summary>
    [TableName("spb_BarRatings")]
    [PrimaryKey("RatingId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "ThreadId,UserId")]
    [Serializable]
    public class BarRating : IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        //todo:需要检查成员初始化的类型是否正确
        public static BarRating New()
        {
            BarRating barRating = new BarRating()
            {
                UserDisplayName = string.Empty,
                Reason = string.Empty,
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow

            };
            return barRating;
        }

        #region 需持久化属性

        /// <summary>
        ///RatingId
        /// </summary>
        public long RatingId { get; protected set; }

        /// <summary>
        ///所属帖子Id
        /// </summary>
        public long ThreadId { get; set; }

        /// <summary>
        ///评分用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public User User
        {
            get
            {
                IUserService service = DIContainer.Resolve<IUserService>();
                return service.GetFullUser(this.UserId);
            }
        }

        /// <summary>
        ///评分用户名
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        ///评的金币值
        /// </summary>
        public int TradePoints { get; set; }

        /// <summary>
        ///评的威望值
        /// </summary>
        public int ReputationPoints { get; set; }

        /// <summary>
        ///理由
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        ///发帖人IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.RatingId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
    }
}