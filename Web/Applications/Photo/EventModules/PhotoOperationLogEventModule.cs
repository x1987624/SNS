//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using Tunynet;
using Tunynet.Events;
using Tunynet.Globalization;
using Tunynet.Logging;
using Spacebuilder.Common;

namespace Spacebuilder.Photo.EventModules
{
    /// <summary>
    /// 处理照片操作日志
    /// </summary>
    public class PhotoOperationLogEventModule : IEventMoudle
    {


        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void IEventMoudle.RegisterEventHandler()
        {
            EventBus<Photo>.Instance().After += new CommonEventHandler<Photo, CommonEventArgs>(PhotoOperationLogEventModule_After);
        }


        /// <summary>
        /// 照片操作日志事件处理
        /// </summary>
        private void PhotoOperationLogEventModule_After(Photo senders, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Delete()
               || eventArgs.EventOperationType == EventOperationType.Instance().Approved()
               || eventArgs.EventOperationType == EventOperationType.Instance().Disapproved()
               || eventArgs.EventOperationType == EventOperationType.Instance().SetEssential()
               || eventArgs.EventOperationType == EventOperationType.Instance().SetSticky()
               || eventArgs.EventOperationType == EventOperationType.Instance().CancelEssential()
               || eventArgs.EventOperationType == EventOperationType.Instance().CancelSticky())
            {
                OperationLogEntry entry = new OperationLogEntry(eventArgs.OperatorInfo);
            
                entry.ApplicationId = entry.ApplicationId;
                entry.Source = PhotoConfig.Instance().ApplicationName;
                entry.OperationType = eventArgs.EventOperationType;
                entry.OperationObjectName = string.IsNullOrEmpty(senders.Description) ? "照片" : senders.Description;
                entry.OperationObjectId = senders.PhotoId;
                entry.Description = string.Format(ResourceAccessor.GetString("OperationLog_Pattern_" + eventArgs.EventOperationType, entry.ApplicationId), "照片", entry.OperationObjectName);

                OperationLogService logService = Tunynet.DIContainer.Resolve<OperationLogService>();
                logService.Create(entry);
            }
        }

    }
}