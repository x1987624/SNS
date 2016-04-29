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
using Tunynet;
using Tunynet.Caching;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 站点论坛设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class BarSettings:IEntity
    {
        #region 帖子

        private int threadSubjectMaxLength = 64;

        /// <summary>
        /// 帖子标题的最高长度
        /// </summary>
        public int ThreadSubjectMaxLength
        {
            get { return threadSubjectMaxLength; }
            set
            {
                if (value < 1)
                    threadSubjectMaxLength = 1;               
                else
                    threadSubjectMaxLength = value;
            }
        }

        private int threadBodyMaxLength = 200000;

        /// <summary>
        /// 帖子内容长度
        /// </summary>
        public int ThreadBodyMaxLength
        {
            get { return threadBodyMaxLength; }
            set
            {
                if (value < 1)
                    threadBodyMaxLength = 1;
                else
                    threadBodyMaxLength = value;
            }
        }

        #endregion

        #region 回帖
        
        private int postBodyMaxLength = 5000;

        /// <summary>
        /// 回帖内容长度
        /// </summary>
        public int PostBodyMaxLength
        {
            get { return postBodyMaxLength; }
            set
            {
                if (value < 1)
                    postBodyMaxLength = 1;
                else
                    postBodyMaxLength = value;
            }
        }

        #endregion

        #region 帖子评分
        private bool enableRating = true;
        /// <summary>
        /// 是否启用帖子评分
        /// </summary>
        public bool EnableRating
        {
            get { return enableRating; }
            set { enableRating = value; }
        }

        #region 评分项
        private int reputationPointsMaxValue = 5;
        /// <summary>
        /// 威望评分最大值
        /// </summary>
        public int ReputationPointsMaxValue
        {
            get { return reputationPointsMaxValue; }
            set { reputationPointsMaxValue = value; }
        }

        private int reputationPointsMinValue = 1;
        /// <summary>
        /// 威望评分最小值
        /// </summary>
        public int ReputationPointsMinValue
        {
            get { return reputationPointsMinValue; }
            set { reputationPointsMinValue = value; }
        }

        private int userReputationPointsPerDay = 20;
        /// <summary>
        /// 用户每日威望评分上限
        /// </summary>
        public int UserReputationPointsPerDay
        {
            get { return userReputationPointsPerDay; }
            set { userReputationPointsPerDay = value; }
        }

        
        

        //private bool enableReputationPoints = true;
        ///// <summary>
        ///// 是否启用威望评分
        ///// </summary>
        //public bool EnableReputationPoints
        //{
        //    get { return enableReputationPoints; }
        //    set { enableReputationPoints = value; }
        //}


        #endregion
        #endregion

        #region 其他设置
        private bool enableUserCreateSection;
        /// <summary>
        /// 是否允许用户创建帖吧
        /// </summary>
        public bool EnableUserCreateSection
        {
            get { return enableUserCreateSection; }
            set { enableUserCreateSection = value; }
        }

        private int userRankOfCreateSection = 5;
        /// <summary>
        /// 用户可以创建帖吧的等级限制
        /// </summary>
        public int UserRankOfCreateSection
        {
            get { return userRankOfCreateSection; }
            set { userRankOfCreateSection = value; }
        }

        private bool onlyFollowerCreateThread = true;
        /// <summary>
        /// 仅允许帖吧关注用户发帖
        /// </summary>
        public bool OnlyFollowerCreateThread
        {
            get { return onlyFollowerCreateThread; }
            set { onlyFollowerCreateThread = value; }
        }

        private bool onlyFollowerCreatePost = false;
        /// <summary>
        /// 仅允许帖吧关注用户回帖
        /// </summary>
        public bool OnlyFollowerCreatePost
        {
            get { return onlyFollowerCreatePost; }
            set { onlyFollowerCreatePost = value; }
        }

        private int sectionManagerMaxCount = 5;

        /// <summary>
        /// 帖吧允许的最大的管理员个数
        /// </summary>
        public int SectionManagerMaxCount
        {
            get { return sectionManagerMaxCount; }
            set { sectionManagerMaxCount = value; }
        }



        #endregion


        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(BarSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
        
    }
}