//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-9</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-9" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Tunynet
{
    /// <summary>
    /// 分页数据封装
    /// </summary>
    /// <typeparam name="T">分页数据的实体类型</typeparam>
    [Serializable]
    public class PagingDataSet<T> : ReadOnlyCollection<T>, IPagingDataSet
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entities">用于分页的实体集合</param>
        public PagingDataSet(IEnumerable<T> entities)
            : base(entities.ToList())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entities">用于分页的实体集合</param>
        public PagingDataSet(IList<T> entities)
            : base(entities)
        {
        }

        private int _pageSize = 20;
        private int _pageIndex = 1;
        private long _totalRecords = 0;

        /// <summary>
        /// 每页显示记录数
        /// </summary>
        public int PageSize
        {
            get { return this._pageSize; }
            set { this._pageSize = value; }
        }

        /// <summary>
        /// 当前页数
        /// </summary>
        public int PageIndex
        {
            get { return this._pageIndex; }
            set { this._pageIndex = value; }
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalRecords
        {
            get { return this._totalRecords; }
            set { this._totalRecords = value; }
        }

        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount
        {
            get
            {
                long result = TotalRecords / PageSize;
                if (TotalRecords % PageSize != 0)
                    result++;

                return Convert.ToInt32(result);
            }
        }

        private double queryDuration = 0;
        /// <summary>
        /// 搜索执行时间(秒)
        /// </summary>
        public double QueryDuration
        {
            get { return queryDuration; }
            set { queryDuration = value; }
        }

    }
}
