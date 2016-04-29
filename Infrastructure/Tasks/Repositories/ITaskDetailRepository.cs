//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-09-13</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-09-13" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
using Tunynet.Repositories;

namespace Tunynet.Tasks
{
    /// <summary>
    /// ITaskDetailRepository
    /// </summary>
    public interface ITaskDetailRepository : IRepository<TaskDetail>
    {
        /// <summary>
        /// 保存任务状态
        /// </summary>
        /// <param name="taskDetail">任务实体</param>
        void SaveTaskStatus(TaskDetail taskDetail);

    }
}
