//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate></createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="libsh" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Microblog
{

    public interface IMicroblogRepository : IRepository<MicroblogEntity>
    {

        /// <summary>
        /// 根据用户获取可排序的微博集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        PagingDataSet<long> GetPagingIds(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "");

        /// <summary>
        /// 获取微博分页数据
        /// </summary>
        ///<param name="pageIndex">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        PagingDataSet<MicroblogEntity> GetPagings(int pageIndex, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated);

        /// <summary>
        /// 根据用户获取可排序的微博集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        PagingDataSet<MicroblogEntity> GetPagings(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "");

        /// <summary>
        /// 根据拥有者获取微博分页集合
        /// </summary>
        /// <param name="ownerId">用户Id</param>
        /// <param name="userId">微博作者Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        PagingDataSet<MicroblogEntity> GetPagings(long ownerId, long? userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "");

        /// <summary>
        /// 根据用户获取指定条数的微博
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        IEnumerable<MicroblogEntity> GetTops(long userId, MediaType? mediaType, bool? isOriginal, int topNumber, string tenantTypeId = "", SortBy_Microblog sortBy = SortBy_Microblog.DateCreated);

        /// <summary>
        /// 根据拥有者获取指定条数的微博
        /// </summary>
        ///<param name="ownerId">微博拥有者Id</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        IEnumerable<MicroblogEntity> GetTops(long ownerId, int topNumber, string tenantTypeId, MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated);

        /// <summary>
        /// 获取指定条数的微博
        /// </summary>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        IEnumerable<MicroblogEntity> GetTops(int topNumber, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated);

        /// <summary>
        /// 管理员后台获取列表方法
        /// </summary>
        /// <param name="query">查询条件</param>
        ///<param name="pageSize">每页显示内容数</param>
        ///<param name="pageIndex">页码</param>
        /// <returns></returns>
        PagingDataSet<MicroblogEntity> GetMicroblogs(MicroblogQuery query, int pageSize, int pageIndex);

        /// <summary>
        /// 获取某个用户的所有微博（用于删除用户）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        IEnumerable<MicroblogEntity> GetAllMicroblogsOfUser(long userId);

        /// <summary>
        /// 获取最新微博微博数
        /// </summary>
        ///<param name="lastMicroblogId">用户当前浏览的最新一条微博Id</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        int GetNewerCount(long lastMicroblogId, string tenantTypeId);

        /// <summary>
        /// 获取最新微博
        /// </summary>
        ///<param name="lastMicroblogId">用户当前浏览的最新一条微博Id</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        IEnumerable<MicroblogEntity> GetNewerMicroblogs(long lastMicroblogId, string tenantTypeId);

        /// <summary>
        /// 获取解析后的内容
        /// </summary>
        /// <param name="microblogId">微博Id</param>
        /// <returns></returns>
        string GetResolvedBody(long microblogId);

        /// <summary>
        /// 删除用户记录（删除用户时使用）
        /// </summary>
        /// <param name="userId">被删除用户</param>
        /// <param name="takeOver">接管用户</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        void DeleteUser(long userId, User takeOver, bool takeOverAll);
        /// <summary>
        /// 获取微博管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId = null);
        /// <summary>
        /// 获取群组统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null);
    }

}
