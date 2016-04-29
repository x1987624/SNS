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

namespace Tunynet.Tasks.Quartz
{
    /// <summary>
    /// 用以管理Quartz任务调度相关的操作
    /// </summary>
    public class QuartzTaskScheduler : ITaskScheduler
    {
        private static List<TaskDetail> _tasks;

        /// <summary>
        ///构造器
        /// </summary>
        public QuartzTaskScheduler()
            : this(new TaskDetailRepository())
        {
        }

        /// <summary>
        ///构造器
        /// </summary>
        public QuartzTaskScheduler(RunAtServer runAtServer)
            : this(new TaskDetailRepository())
        {
            if (_tasks != null)
            {
                _tasks = _tasks.Where(n => n.RunAtServer == runAtServer).ToList();
            }
        }

        /// <summary>
        ///构造器
        /// </summary>
        /// <param name="taskDetailRepository">用于测试的Repository</param>
        public QuartzTaskScheduler(ITaskDetailRepository taskDetailRepository)
        {
            if (_tasks == null)
            {
                _tasks = new TaskService(taskDetailRepository).GetAll().ToList();
            }
        }

        #region 常用操作

        /// <summary>
        /// 启动任务
        /// </summary>
        public void Start()
        {
            if (_tasks.Count() == 0)
                return;

            TaskService taskService = new TaskService();
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();

            foreach (var task in _tasks)
            {
                if (!task.Enabled)
                    continue;

                Type type = Type.GetType(task.ClassType);
                if (type == null)
                    continue;

                string triggerName = type.Name + "_trigger";

                IJobDetail job = JobBuilder.Create(typeof(QuartzTask))
                                           .WithIdentity(type.Name)
                                           .Build();

                job.JobDataMap.Add(new KeyValuePair<string, object>("Id", task.Id));


                TriggerBuilder tb = TriggerBuilder.Create()
                                                  .WithIdentity(triggerName)
                                                  .WithCronSchedule(task.TaskRule);

                if (task.StartDate > DateTime.MinValue)
                {
                    tb.StartAt(new DateTimeOffset(task.StartDate));
                }

                if (task.EndDate > task.StartDate)
                {
                    tb.EndAt(task.EndDate);
                }

                ICronTrigger trigger = (ICronTrigger)tb.Build();
                DateTime ft = sched.ScheduleJob(job, trigger).DateTime;
                task.NextStart = TimeZoneInfo.ConvertTime(ft, trigger.TimeZone);
            }

            sched.Start();
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void Stop()
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.Shutdown(true);
        }

        /// <summary>
        /// 更新任务在调度器中的状态
        /// </summary>
        /// <param name="task">任务详细信息</param>
        public void Update(TaskDetail task)
        {
            if (task == null)
                return;

            int index = _tasks.FindIndex(n => n.Id == task.Id);

            if (_tasks[index] == null)
                return;

            task.ClassType = _tasks[index].ClassType;
            task.LastEnd = _tasks[index].LastEnd;
            task.LastStart = _tasks[index].LastStart;
            task.LastIsSuccess = _tasks[index].LastIsSuccess;

            _tasks[index] = task;

            Type type = Type.GetType(task.ClassType);
            if (type == null)
            {
                return;
            }

            Remove(type.Name);

            if (!task.Enabled)
                return;

            string triggerName = type.Name + "_trigger";

            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();

            IJobDetail job = JobBuilder.Create(typeof(QuartzTask))
                                       .WithIdentity(type.Name)
                                       .Build();

            job.JobDataMap.Add(new KeyValuePair<string, object>("Id", task.Id));


            TriggerBuilder tb = TriggerBuilder.Create()
                                              .WithIdentity(triggerName)
                                              .WithCronSchedule(task.TaskRule);

            if (task.StartDate > DateTime.MinValue)
            {
                tb.StartAt(new DateTimeOffset(task.StartDate));
            }

            if (task.EndDate.HasValue && task.EndDate > task.StartDate)
            {
                tb.EndAt(task.EndDate);
            }


            ICronTrigger trigger = (ICronTrigger)tb.Build();

            DateTime ft = sched.ScheduleJob(job, trigger).DateTime;
            task.NextStart = TimeZoneInfo.ConvertTime(ft, trigger.TimeZone);
        }

        /// <summary>
        /// 重启所有任务
        /// </summary>
        public void ResumeAll()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// 获取单个任务
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns>任务详细信息</returns>
        public TaskDetail GetTask(int Id)
        {
            return _tasks.FirstOrDefault(n => n.Id == Id);
        }

        /// <summary>
        /// 运行单个任务
        /// </summary>
        /// <param name="Id">任务Id</param>
        public void Run(int Id)
        {
            TaskDetail task = GetTask(Id);
            Run(task);
        }

        /// <summary>
        /// 运行单个任务
        /// </summary>
        /// <param name="task">要运行的任务</param>
        public void Run(TaskDetail task)
        {
            if (task == null)
                return;

            Type type = Type.GetType(task.ClassType);
            if (type == null)
            {
                LoggerFactory.GetLogger().Error(string.Format("任务： {0} 的taskType为空。", task.Name));
                return;
            }

            ITask t = (ITask)Activator.CreateInstance(type);
            if (t != null && !task.IsRunning)
            {

                try
                {
                    t.Execute();
                }
                catch (Exception ex)
                {
                    LoggerFactory.GetLogger().Error(ex, string.Format("执行任务： {0} 出现异常。", task.Name));
                }
            }
        }

        #endregion

        #region 状态保持

        /// <summary>
        /// 保存任务状态
        /// </summary>
        /// <remarks>将当前需要需要ResumeContinue为true的任务记录，以便应用程序重启后检查是否需要立即执行</remarks>
        public void SaveTaskStatus()
        {
            foreach (var item in _tasks)
            {
                TaskService taskService = new TaskService();
                taskService.SaveTaskStatus(item);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从调度其中移除任务
        /// </summary>
        /// <param name="name">调度器中任务的名称</param>
        private void Remove(string name)
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            IScheduler sched = sf.GetScheduler();
            sched.DeleteJob(new JobKey(name));
        }

        #endregion

    }
}
