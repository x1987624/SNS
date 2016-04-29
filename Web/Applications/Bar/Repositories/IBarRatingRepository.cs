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
using System.Text;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Tunynet.Caching;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖子评分仓储接口
    /// </summary>
    public interface IBarRatingRepository : IRepository<BarRating>
    {

        /// <summary>
        /// 获取主题帖的评分记录分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>评分列表</returns>
        PagingDataSet<BarRating> Gets(long threadId, int pageIndex);

        /// <summary>
        /// 根据用户获取所有评过分的帖子列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="beforeDays">最近多少天内</param>
        /// <returns></returns>
        IEnumerable<long> GetThreadIdsByUser(long userId, int beforeDays = 30);

        /// <summary>
        /// 获取今天用户的评分数
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>用户的评分数</returns>
        int GetUserTodayRatingSum(long userId);
    }
}