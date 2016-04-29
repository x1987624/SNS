//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-11</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-11" version="0.5">创建</log>
//<log date="2011-10-27" version="0.6" author="libsh">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 封装用于分页的实体Id
    /// </summary>
    [Serializable]
    public class PagingEntityIdCollection
    {
        private List<object> entityIds = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entityIds">实体Id集合</param>
        public PagingEntityIdCollection(IEnumerable<object> entityIds)
        {
            this.entityIds = entityIds.ToList();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entityIds">实体Id集合</param>
        /// <param name="totalRecords">总记录数</param>
        public PagingEntityIdCollection(IEnumerable<object> entityIds, long totalRecords)
        {
            this.entityIds = entityIds.ToList();
            this.totalRecords = totalRecords;
        }

        /// <summary>
        /// 获取的entityId数量
        /// </summary>
        public int Count
        {
            get
            {
                if (entityIds == null)
                    return 0;
                else
                    return entityIds.Count;
            }
        }

        private long totalRecords = -1;
        /// <summary>
        /// 符合查询条件的总记录数
        /// </summary>
        public long TotalRecords
        {
            get
            {
                if (totalRecords > 0)
                    return totalRecords;
                else
                    return this.Count;
            }
        }

        private bool isContainsMultiplePages = false;
        /// <summary>
        /// 是否包含前N页数据
        /// </summary>
        public bool IsContainsMultiplePages
        {
            get { return isContainsMultiplePages; }
            set { isContainsMultiplePages = value; }
        }

        /// <summary>
        /// 获取pageIndex所在页数的EntityId集合
        /// </summary>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageIndex">从1开始的当前页码</param>
        /// <returns>指定页码的实体Id集合</returns>
        public IEnumerable<object> GetPagingEntityIds(int pageSize, int pageIndex)
        {
            if (entityIds == null)
                return new List<object>();

            //如果容纳的不是前N页数据，则前pageSize条记录
            if (!IsContainsMultiplePages)
                return entityIds.GetRange(0, this.Count > pageSize ? pageSize : this.Count);

            if (pageIndex < 1)
                pageIndex = 1;

            int pageLowerBound = pageSize * (pageIndex - 1);
            int pageUpperBound = pageSize * pageIndex;

            int count = entityIds.Count;
            if (pageLowerBound < count)
            {
                if (pageUpperBound < count)
                    return entityIds.GetRange(pageLowerBound, pageSize);
                else
                    return entityIds.GetRange(pageLowerBound, count - pageLowerBound);
            }
            else
            {
                return new List<object>();
            }
        }

        /// <summary>
        /// 获取前topNumber条EntityId集合
        /// </summary>
        /// <param name="topNumber">前多少条数据</param>
        /// <returns></returns>
        public IEnumerable<object> GetTopEntityIds(int topNumber)
        {
            if (entityIds == null)
                return new List<object>();

            int count = entityIds.Count;
            return entityIds.GetRange(0, Count > topNumber ? topNumber : Count);
        }

        ///// <summary>
        ///// 获取整个EntityId集合的副本
        ///// </summary>
        ///// <returns></returns>
        //public List<object> GetAll()
        //{
        //    return primaryKeyList.GetRange(0, this.Count);
        //}

    }
}
