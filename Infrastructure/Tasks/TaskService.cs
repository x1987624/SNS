//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.51</verion>
//<createdate>2012-2-14</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-2-14" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Tunynet.Caching;

namespace Tunynet.Tasks
{

    /// <summary>
    /// 任务业务逻辑
    /// </summary>
    public class TaskService
    {
        //TaskDetail Repository
        private ITaskDetailRepository taskDetailRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskService()
            : this(new TaskDetailRepository())
        {
        }

        /// <summary>
        /// 可设置repository的构造函数（主要用于测试用例）
        /// </summary>
        /// <param name="taskDetailRepository">TaskDetailRepository</param>
        public TaskService(ITaskDetailRepository taskDetailRepository)
        {
            this.taskDetailRepository = taskDetailRepository;
        }

        /// <summary>
        /// 依据TaskName获取任务
        /// </summary>
        /// <param name="Id">任务Id</param>
        public TaskDetail Get(int Id)
        {
            return taskDetailRepository.Get(Id);
        }

        /// <summary>
        /// 获取所用任务
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TaskDetail> GetAll()
        {
            return taskDetailRepository.GetAll();
        }

        /// <summary>
        /// 更新任务相关信息
        /// </summary>
        /// <param name="entity">任务详细信息实体</param>
        public void Update(TaskDetail entity)
        {
            taskDetailRepository.Update(entity);
            TaskSchedulerFactory.GetScheduler().Update(entity);
        }

        /// <summary>
        /// 应用程序关闭时保存任务当前状态
        /// </summary>
        /// <param name="entity">任务详细信息实体</param>
        public void SaveTaskStatus(TaskDetail entity)
        {
            taskDetailRepository.SaveTaskStatus(entity);
        }
    }
}