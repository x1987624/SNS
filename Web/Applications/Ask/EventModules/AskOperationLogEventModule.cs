//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Spacebuilder.Ask.Resources;
using Spacebuilder.Common;
using Tunynet.Common;
using Tunynet.Events;
using Tunynet.Globalization;
using Tunynet.Utilities;
using Tunynet.Logging;
using System;
using Tunynet;

namespace Spacebuilder.Ask.EventModules
{
    /// <summary>
    /// 处理问答操作日志
    /// </summary>
    public class AskOperationLogEventModule : IEventMoudle
    {


        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void IEventMoudle.RegisterEventHandler()
        {
            //提问
            EventBus<AskQuestion>.Instance().After += new CommonEventHandler<AskQuestion, CommonEventArgs>(AskQuestionOperationLogEventModule_After);

            //回答
            EventBus<AskAnswer>.Instance().After += new CommonEventHandler<AskAnswer, CommonEventArgs>(AskAnswerOperationLogEventModule_After);
        }


        /// <summary>
        /// 问题操作日志事件处理
        /// </summary>
        private void AskQuestionOperationLogEventModule_After(AskQuestion senders, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Delete()
               || eventArgs.EventOperationType == EventOperationType.Instance().Approved()
               || eventArgs.EventOperationType == EventOperationType.Instance().Disapproved()
               || eventArgs.EventOperationType == EventOperationType.Instance().SetEssential()
               || eventArgs.EventOperationType == EventOperationType.Instance().SetSticky()
               || eventArgs.EventOperationType == EventOperationType.Instance().CancelEssential()
               || eventArgs.EventOperationType == EventOperationType.Instance().CancelSticky())
            {
            //if (eventArgs.EventOperationType == EventOperationType.Instance().SetEssential()
            //  || eventArgs.EventOperationType == EventOperationType.Instance().CancelEssential())
            //{
                OperationLogEntry entry = new OperationLogEntry(eventArgs.OperatorInfo);
                entry.ApplicationId = eventArgs.ApplicationId;
                entry.Source = AskConfig.Instance().ApplicationName;
                entry.OperationType = eventArgs.EventOperationType;
                entry.OperationObjectName = senders.Subject;
                entry.OperationObjectId = senders.QuestionId;
                entry.Description = string.Format(ResourceAccessor.GetString("OperationLog_Pattern_" + eventArgs.EventOperationType, entry.ApplicationId), "问题", entry.OperationObjectName);

                OperationLogService logService = Tunynet.DIContainer.Resolve<OperationLogService>();
                logService.Create(entry);
            }
        }

        /// <summary>
        /// 回答操作日志事件处理
        /// </summary>
        private void AskAnswerOperationLogEventModule_After(AskAnswer senders, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {

                OperationLogEntry entry = new OperationLogEntry(eventArgs.OperatorInfo);

                entry.ApplicationId = entry.ApplicationId;
                entry.Source = AskConfig.Instance().ApplicationName;
                entry.OperationType = eventArgs.EventOperationType;
                entry.OperationObjectName = StringUtility.Trim(senders.Body, 20);
                entry.OperationObjectId = senders.QuestionId;
                entry.Description = string.Format(ResourceAccessor.GetString("OperationLog_Pattern_" + eventArgs.EventOperationType), "回答", entry.OperationObjectName);

                OperationLogService logService = Tunynet.DIContainer.Resolve<OperationLogService>();
                logService.Create(entry);
            }
        }
    }
}