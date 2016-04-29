//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{

    /// <summary>
    /// 评分业务逻辑类
    /// </summary>
    public class BarRatingService
    {
        private IBarRatingRepository barRatingRepository = null;

        
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BarRatingService()
            : this(new BarRatingRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="barRatingRepository"></param>
        public BarRatingService(IBarRatingRepository barRatingRepository)
        {
            this.barRatingRepository = barRatingRepository;
        }

        #region 维护评分

        /// <summary>
        /// 创建评分
        /// </summary>
        /// <param name="rating">评分</param>
        /// <returns>true-评分成功，false-评分失败（可能今日评分已超过限额）</returns>
        public bool Create(BarRating rating)
        {
            BarThreadService barThreadService = new BarThreadService();
            BarThread thread = barThreadService.Get(rating.ThreadId);
            EventBus<BarRating>.Instance().OnBefore(rating, new CommonEventArgs(EventOperationType.Instance().Create()));
            bool result = false;
            
            
            bool.TryParse(barRatingRepository.Insert(rating).ToString(), out result);

            if (result)
            {
                //给楼主加威望/交易积分
                IUserService userService = DIContainer.Resolve<IUserService>();
                userService.ChangePoints(thread.UserId, 0, rating.ReputationPoints, rating.TradePoints);
                PointService pointService = new PointService();
                pointService.CreateRecord(thread.UserId, "帖子评分", "发布的帖子被其他用户评分", 0, rating.ReputationPoints, rating.TradePoints);

                EventBus<BarRating>.Instance().OnAfter(rating, new CommonEventArgs(EventOperationType.Instance().Create()));
            }
            return result;
        }

        #endregion

        #region 获取评分


        /// <summary>
        /// 获取单个评分实体
        /// </summary>
        /// <param name="ratingId">评分Id</param>
        /// <returns>评分</returns>
        public BarRating Get(long ratingId)
        {
            return barRatingRepository.Get(ratingId);
        }

        /// <summary>
        /// 判断用户某一段时间内是否对某个帖子评过分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="threadId"></param>
        /// <param name="beforeDays">最近多少天内</param>
        /// <returns></returns>
        public bool IsRated(long userId, long threadId, int beforeDays = 30)
        {
            
            
            IEnumerable<long> threadIds = barRatingRepository.GetThreadIdsByUser(userId, beforeDays);

            if (threadIds != null)
            {
                return threadIds.Contains(threadId);
            }

            return false;
        }

        /// <summary>
        /// 获取主题帖的评分记录分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>评分列表</returns>
        public PagingDataSet<BarRating> Gets(long threadId, int pageIndex = 1)
        {
            //缓存周期：对象集合，需要维护即时性
            //排序：发布时间（倒序）
            return barRatingRepository.Gets(threadId, pageIndex);
        }


        #endregion

        /// <summary>
        /// 获取我今天评分的集合
        /// </summary>
        /// <returns>我今天评分的集合</returns>
        public int GetUserTodayRatingSum(long userId)
        {
            return barRatingRepository.GetUserTodayRatingSum(userId);
        }

    }
}