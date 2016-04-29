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
using Tunynet.Caching;

namespace Tunynet.Email
{
    /// <summary>
    /// 业务逻辑类
    /// </summary>
    public class EmailQueueRepository : Repository<EmailQueueEntry>, IEmailQueueRepository
    {
        /// <summary>
        /// 按照优先级从高到低排序，获取达到NextTryTime的前N条邮件
        /// </summary>
        /// <param name="maxNumber">最大记录数</param>
        public IEnumerable<EmailQueueEntry> Dequeue(int maxNumber)
        {
            //排序方式：按优先级倒排序（数字大的优先级高）
            //注意，不考虑缓存，原因：生命周期短，更新较多
            if (maxNumber < 1)
                return null;

            //利用通用方法获取Top集合
            //todo:zhengw,by libsh:走查下以下代码

            var sql = PetaPoco.Sql.Builder.Select("Id").From("tn_EmailQueue").Where("NextTryTime < @0 ", DateTime.UtcNow).Where("IsFailed = 0").OrderBy("Priority desc");
            var entryIds_objectId = CreateDAO().FetchTopPrimaryKeys<EmailQueueEntry>(maxNumber, sql);
            return PopulateEntitiesByEntityIds<int>(entryIds_objectId.Cast<int>());
        }
    }
}