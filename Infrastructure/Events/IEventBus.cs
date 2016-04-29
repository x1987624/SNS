//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Events
{

    /// <summary>
    /// 用于事件处理的通用委托
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    /// <typeparam name="A">事件参数的对象类型</typeparam>
    /// <param name="sender">触发事件的对象</param>
    /// <param name="eventArgs">事件参数</param>
    public delegate void CommonEventHandler<S, A>(S sender, A eventArgs);

    /// <summary>
    /// 用于事件处理带历史数据的委托
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    /// <typeparam name="A">事件参数的对象类型</typeparam>
    /// <param name="sender">触发事件的对象</param>
    /// <param name="eventArgs">事件参数</param>
    /// <param name="historyData">触发事件对象的历史数据（例如S从一种状态变更为另一种状态，historyData指变更前的对象）</param>
    public delegate void EventHandlerWithHistory<S, A>(S sender, A eventArgs, S historyData);

    /// <summary>
    /// 一组批量事件处理的委托
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    /// <typeparam name="A">事件参数的对象类型</typeparam>
    /// <param name="senders">触发事件的对象集合</param>
    /// <param name="eventArgs">事件参数</param>
    public delegate void BatchEventHandler<S, A>(IEnumerable<S> senders, A eventArgs);

    /// <summary>
    /// 事件总线接口
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    /// <typeparam name="A">事件参数的对象类型</typeparam>
    public interface IEventBus<S, A>
    {

        #region 事件定义

        /// <summary>
        /// 操作执行前事件
        /// </summary>
        event CommonEventHandler<S, A> Before;

        /// <summary>
        /// 操作执行后事件
        /// </summary>
        event CommonEventHandler<S, A> After;

        /// <summary>
        /// 含历史数据操作执行前事件
        /// </summary>
        event EventHandlerWithHistory<S, A> BeforeWithHistory;

        /// <summary>
        /// 含历史数据操作执行后事件
        /// </summary>
        event EventHandlerWithHistory<S, A> AfterWithHistory;

        /// <summary>
        /// 批量操作执行前事件
        /// </summary>
        event BatchEventHandler<S, A> BatchBefore;

        /// <summary>
        /// 批量操作执行后事件
        /// </summary>
        event BatchEventHandler<S, A> BatchAfter;

        #endregion

        #region 执行事件

        /// <summary>
        /// 触发操作执行前事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        void OnBefore(S sender, A eventArgs);

        /// <summary>
        /// 触发操作执行后事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        void OnAfter(S sender, A eventArgs);

        /// <summary>
        /// 触发含历史数据操作执行前事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="historyData">触发事件对象的历史数据（例如S从一种状态变更为另一种状态，historyData指变更前的对象）</param>
        void OnBeforeWithHistory(S sender, A eventArgs, S historyData);

        /// <summary>
        /// 触发含历史数据操作执行后事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="historyData">触发事件对象的历史数据（例如S从一种状态变更为另一种状态，historyData指变更前的对象）</param>
        void OnAfterWithHistory(S sender, A eventArgs, S historyData);


        /// <summary>
        /// 触发批量操作执行前事件
        /// </summary>
        /// <param name="senders">触发事件的对象集合</param>
        /// <param name="eventArgs">事件参数</param>
        void OnBatchBefore(IEnumerable<S> senders, A eventArgs);

        /// <summary>
        /// 触发批量操作执行后事件
        /// </summary>
        /// <param name="senders">触发事件的对象集合</param>
        /// <param name="eventArgs">事件参数</param>
        void OnBatchAfter(IEnumerable<S> senders, A eventArgs);


        #endregion


    }
}
