//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.51</verion>
//<createdate>2012-02-10</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-10" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
namespace Tunynet.Tasks
{
    /// <summary>
    /// 任务执行控制器
    /// </summary>
    /// <remarks>需要从DI容器中获取注册的tasks</remarks>
    public interface ITaskScheduler
    {
        /// <summary>
        /// 开始执行任务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止任务
        /// </summary>
        void Stop();

        /// <summary>
        /// 更新任务在调度器中的状态
        /// </summary>
        /// <param name="task">任务详细信息</param>
        void Update(TaskDetail task);

        /// <summary>
        /// 重启所有任务
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// 获取单个任务
        /// </summary>
        /// <param name="Id">任务Id</param>
        /// <returns>任务详细信息</returns>
        TaskDetail GetTask(int Id);

        /// <summary>
        /// 执行单个任务
        /// </summary>
        /// <param name="Id">任务Id</param>
        void Run(int Id);

        /// <summary>
        /// 保存任务状态
        /// </summary>
        /// <remarks>将当前需要需要ResumeContinue为true的任务记录，以便应用程序重启后检查是否需要立即执行</remarks>
        void SaveTaskStatus();
    }
}