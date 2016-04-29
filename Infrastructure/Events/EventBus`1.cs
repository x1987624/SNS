//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Web;
using System.Threading.Tasks;
using Tunynet.Logging;

namespace Tunynet.Events
{
    /// <summary>
    /// 事件总线（用于定义事件、触发事件）
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam> 
    /// <typeparam name="T">通用事件参数</typeparam>
    public class EventBus<S, T> : IEventBus<S, T> where T : CommonEventArgs
    {

        #region 获取实例
        private static volatile EventBus<S, T> _instance = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns>返回EventBus实例</returns>
        public static EventBus<S, T> Instance()
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new EventBus<S, T>();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        protected EventBus()
        { }
        #endregion


        private static EventHandlerList Handlers = new EventHandlerList();


        #region EventHandlder Keys

        private object EventHandlerKey_Before = new object();
        private object EventHandlerKey_After = new object();

        private object EventHandlerKey_BeforeWithHistory = new object();
        private object EventHandlerKey_AfterWithHistory = new object();

        private object EventHandlerKey_BatchBefore = new object();
        private object EventHandlerKey_BatchAfter = new object();

        #endregion


        #region 事件定义

        /// <summary>
        /// 操作执行前事件
        /// </summary>
        public event CommonEventHandler<S, T> Before
        {
            add { Handlers.AddHandler(EventHandlerKey_Before, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_Before, value); }
        }

        /// <summary>
        /// 操作执行后事件
        /// </summary>
        public event CommonEventHandler<S, T> After
        {
            add { Handlers.AddHandler(EventHandlerKey_After, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_After, value); }
        }

        /// <summary>
        /// 含历史数据操作执行前事件
        /// </summary>
        public event EventHandlerWithHistory<S, T> BeforeWithHistory
        {
            add { Handlers.AddHandler(EventHandlerKey_BeforeWithHistory, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_BeforeWithHistory, value); }
        }

        /// <summary>
        /// 含历史数据操作执行后事件
        /// </summary>
        public event EventHandlerWithHistory<S, T> AfterWithHistory
        {
            add { Handlers.AddHandler(EventHandlerKey_AfterWithHistory, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_AfterWithHistory, value); }
        }

        /// <summary>
        /// 批量操作执行前事件
        /// </summary>
        public event BatchEventHandler<S, T> BatchBefore
        {
            add { Handlers.AddHandler(EventHandlerKey_BatchBefore, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_BatchBefore, value); }
        }

        /// <summary>
        /// 批量操作执行后事件
        /// </summary>
        public event BatchEventHandler<S, T> BatchAfter
        {
            add { Handlers.AddHandler(EventHandlerKey_BatchAfter, value); }
            remove { Handlers.RemoveHandler(EventHandlerKey_BatchAfter, value); }
        }

        #endregion


        #region 执行事件

        /// <summary>
        /// 触发操作执行前事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        public void OnBefore(S sender, T eventArgs)
        {
            CommonEventHandler<S, T> handler = Handlers[EventHandlerKey_Before] as CommonEventHandler<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();
                foreach (CommonEventHandler<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(sender, eventArgs, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发操作执行前事件时发生异常");
                    }
                }
            }
        }

        /// <summary>
        /// 触发操作执行后事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        public void OnAfter(S sender, T eventArgs)
        {
            CommonEventHandler<S, T> handler = Handlers[EventHandlerKey_After] as CommonEventHandler<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();
                foreach (CommonEventHandler<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(sender, eventArgs, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发操作执行后事件时发生异常");
                    }
                }

                ////并行执行事件处理程序
                //System.Threading.Tasks.Parallel.ForEach(delegateArray,
                //d =>
                //{
                //    try
                //    {
                //        ((CommonEventHandler<S, T>)d).Invoke(sender, eventArgs);
                //    }
                //    catch (Exception e)
                //    {
                //        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发操作执行后事件时发生异常");
                //    }
                //});
            }
        }

        /// <summary>
        /// 触发含历史数据操作执行前事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="historyData">触发事件对象的历史数据（例如S从一种状态变更为另一种状态，historyData指变更前的对象）</param>
        public void OnBeforeWithHistory(S sender, T eventArgs, S historyData)
        {
            EventHandlerWithHistory<S, T> handler = Handlers[EventHandlerKey_BeforeWithHistory] as EventHandlerWithHistory<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();

                foreach (EventHandlerWithHistory<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(sender, eventArgs, historyData, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发含历史数据操作执行前事件时发生异常");
                    }
                }
            }
        }

        /// <summary>
        /// 触发含历史数据操作执行后事件
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="historyData">触发事件对象的历史数据（例如S从一种状态变更为另一种状态，historyData指变更前的对象）</param>
        public void OnAfterWithHistory(S sender, T eventArgs, S historyData)
        {
            EventHandlerWithHistory<S, T> handler = Handlers[EventHandlerKey_AfterWithHistory] as EventHandlerWithHistory<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();

                foreach (EventHandlerWithHistory<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(sender, eventArgs, historyData, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发含历史数据操作执行后事件时发生异常");
                    }
                }
            }
        }


        /// <summary>
        /// 触发批量操作执行前事件
        /// </summary>
        /// <param name="senders">触发事件的对象集合</param>
        /// <param name="eventArgs">事件参数</param>
        public void OnBatchBefore(IEnumerable<S> senders, T eventArgs)
        {
            BatchEventHandler<S, T> handler = Handlers[EventHandlerKey_BatchBefore] as BatchEventHandler<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();

                foreach (BatchEventHandler<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(senders, eventArgs, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发批量操作执行前事件时发生异常");
                    }
                }
            }
        }

        /// <summary>
        /// 触发批量操作执行后事件
        /// </summary>
        /// <param name="senders">触发事件的对象集合</param>
        /// <param name="eventArgs">事件参数</param>
        public void OnBatchAfter(IEnumerable<S> senders, T eventArgs)
        {
            BatchEventHandler<S, T> handler = Handlers[EventHandlerKey_BatchAfter] as BatchEventHandler<S, T>;
            if (handler != null)
            {
                //获取事件中的多路委托列表
                Delegate[] delegateArray = handler.GetInvocationList();

                foreach (BatchEventHandler<S, T> dele in delegateArray)
                {
                    try
                    {
                        dele.BeginInvoke(senders, eventArgs, null, null);
                    }
                    catch (Exception e)
                    {
                        LoggerFactory.GetLogger().Log(LogLevel.Error, e, "执行触发批量操作执行后事件时发生异常");
                    }
                }
            }
        }

        #endregion



    }
}
