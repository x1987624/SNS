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


namespace Tunynet.Tasks
{
    /// <summary>
    /// 任务工厂
    /// </summary>
    /// <remarks>
    /// 用于获取TaskScheduler
    /// </remarks>
    public static class TaskSchedulerFactory
    {
        private static ITaskScheduler _scheduler = null;

        /// <summary>
        /// 获取任务调度器
        /// </summary>
        /// <returns></returns>
        public static ITaskScheduler GetScheduler()
        {
            if (_scheduler == null)
            {
                _scheduler = DIContainer.Resolve<ITaskScheduler>();
            }
            return _scheduler;
        }
    }
}
