//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.51</verion>
//<createdate>2012-02-14</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-14" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using Quartz;
using Tunynet.Logging;

namespace Tunynet.Tasks.Quartz
{
    /// <summary>
    /// Quartz任务实现
    /// </summary>
    public class QuartzTask : IJob
    {

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="context">Quartz任务运行环境</param>
        /// <remarks>外部不需调用，仅用于任务调度组建内部</remarks>
        public void Execute(IJobExecutionContext context)
        {
            int Id = context.JobDetail.JobDataMap.GetInt("Id");
            TaskDetail task = TaskSchedulerFactory.GetScheduler().GetTask(Id);

            if (task == null)
            {
                throw new ArgumentException("Not found task ：" + task.Name);
            }


            TaskService taskService = new TaskService();

            task.IsRunning = true;
            DateTime lastStart = DateTime.UtcNow;

            try
            {
                ITask excuteTask = (ITask)Activator.CreateInstance(Type.GetType(task.ClassType));
                excuteTask.Execute(task);

                task.LastIsSuccess = true;
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger().Error(ex, string.Format("Exception while running job {0} of type {1}", context.JobDetail.Key, context.JobDetail.JobType.ToString()));
                task.LastIsSuccess = false;
            }

            task.IsRunning = false;

            task.LastStart = lastStart;
            if (context.NextFireTimeUtc.HasValue)
                task.NextStart = context.NextFireTimeUtc.Value.UtcDateTime;
            else
                task.NextStart = null;

            task.LastEnd = DateTime.UtcNow;
            taskService.SaveTaskStatus(task);
        }
    }
}
