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
namespace Tunynet.Tasks
{
    /// <summary>
    /// 用于注册任务的接口
    /// </summary>
    /// <remarks>所有任务都需要实现此接口</remarks>
    public interface ITask
    {
        /// <summary>
        /// 执行任务的方法
        /// </summary>
        /// <param name=" taskDetail">任务配置状态信息</param>
        void Execute(TaskDetail taskDetail = null);
    }
}
