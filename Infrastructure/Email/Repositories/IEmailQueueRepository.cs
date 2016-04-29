//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>20120302</createdate>
//<author>杨明杰</author>
//<email>yangmj@tunynet.com</email>
//<log date="2012-03-02" version="0.5">新建</log>
//<log date="2012-03-10" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;

namespace Tunynet.Email
{
    /// <summary>
    /// Email队列基类
    /// </summary>
    public interface IEmailQueueRepository : IRepository<EmailQueueEntry>
    {
        /// <summary>
        /// 按照优先级从高到低排序，获取达到NextTryTime的前N条邮件
        /// </summary>
        /// <param name="maxNumber">最大记录数</param>
        IEnumerable<EmailQueueEntry> Dequeue(int maxNumber);

        //public PagingDataSet<EmailQueueEntry> GetPagingMailQueueEntries(bool isFailed, string keywords, int pageIndex,int pageSize);
    }
}
