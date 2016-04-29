//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-14</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-14" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Tunynet.Logging;
using Tunynet.Repositories;
using PetaPoco;

namespace Tunynet.Tasks
{
    /// <summary>
    /// TaskDetailRepository
    /// </summary>
    public class TaskDetailRepository : Repository<TaskDetail>, ITaskDetailRepository
    {
        /// <summary>
        /// 保存任务状态
        /// </summary>
        /// <param name="taskDetail">任务实体</param>
        public void SaveTaskStatus(TaskDetail taskDetail)
        {
            Sql sql = Sql.Builder;

            sql.Append(@"update tn_TaskDetails 
                       set LastStart = @0, LastEnd = @1,NextStart = @2,IsRunning = @3
                       where Id = @4",
                       taskDetail.LastStart, taskDetail.LastEnd, taskDetail.NextStart, taskDetail.IsRunning, taskDetail.Id);

            CreateDAO().Execute(sql);
        }

    }
}
