//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Ask.EventModules
{
    /// <summary>
    /// 处理问答全文检索索引的EventMoudle
    /// </summary>
    public class AskIndexEventModule : IEventMoudle
    {
        private AskSearcher askSearcher = null;

        /// <summary>
        /// 注册EventHandler
        /// </summary>
        public void RegisterEventHandler()
        {
            EventBus<AskQuestion>.Instance().After += new CommonEventHandler<AskQuestion, CommonEventArgs>(AskQuestion_After);
            EventBus<AskAnswer>.Instance().After += new CommonEventHandler<AskAnswer, CommonEventArgs>(AskAnswer_After);
            EventBus<string, TagEventArgs>.Instance().BatchAfter += new BatchEventHandler<string, TagEventArgs>(AddTagsToQuestion_BatchAfter);
        }

        #region 问答增量索引

        /// <summary>
        /// 问题增量索引
        /// </summary>
        /// <param name="question"></param>
        /// <param name="eventArgs"></param>
        private void AskQuestion_After(AskQuestion question, CommonEventArgs eventArgs)
        {
            if (question == null)
            {
                return;
            }
            if (askSearcher == null)
            {
                askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            }
            //问题添加、删除、更新、设置精华、取消精华操作时更改索引
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                askSearcher.Insert(question);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                askSearcher.Delete(question.QuestionId);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Update() || eventArgs.EventOperationType == EventOperationType.Instance().Approved() || eventArgs.EventOperationType == EventOperationType.Instance().Disapproved())
            {
                askSearcher.Update(question);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().SetEssential())
            {
                askSearcher.Update(question);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().CancelEssential())
            {
                askSearcher.Update(question);
            }
        }

        /// <summary>
        /// 为问答添加标签时触发
        /// </summary>
        private void AddTagsToQuestion_BatchAfter(IEnumerable<string> senders, TagEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().AskQuestion())
            {
                long questionId = eventArgs.ItemId;
                if (askSearcher == null)
                {
                    askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
                }
                askSearcher.Update(new AskService().GetQuestion(questionId));
            }
        }

        /// <summary>
        /// 回答增量索引
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="eventArgs"></param>
        private void AskAnswer_After(AskAnswer answer, CommonEventArgs eventArgs)
        {
            if (answer == null)
            {
                return;
            }
            if (askSearcher == null)
            {
                askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            }
            //创建回答、更新回答、删除回答时更新问题索引
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create() || eventArgs.EventOperationType == EventOperationType.Instance().Update() || eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                askSearcher.Update(answer.Question);

            }

        }

        #endregion
    }
}