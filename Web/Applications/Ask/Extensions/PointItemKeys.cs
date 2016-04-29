//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 积分项
    /// </summary>
    public static class PointItemKeysExtension
    {
        /// <summary>
        /// 回答问题
        /// </summary>
        public static string Ask_CreateAnswer(this PointItemKeys pointItemKeys)
        {
            return "Ask_CreateAnswer";
        }

        /// <summary>
        /// 删除回答
        /// </summary>
        public static string Ask_DeleteAnswer(this PointItemKeys pointItemKeys)
        {
            return "Ask_DeleteAnswer";
        }

        /// <summary>
        /// 采纳回答
        /// </summary>
        public static string Ask_AcceptedAnswer(this PointItemKeys pointItemKeys)
        {
            return "Ask_AcceptedAnswer";
        }

        /// <summary>
        /// 回答被采纳
        /// </summary>
        public static string Ask_AnswerWereAccepted(this PointItemKeys pointItemKeys)
        {
            return "Ask_AnswerWereAccepted";
        }

         /// <summary>
        /// 创建问题
        /// </summary>
        public static string Ask_CreateQuestion(this PointItemKeys pointItemKeys)
        {
            return "Ask_CreateQuestion";
        }

         /// <summary>
        /// 删除问题
        /// </summary>
        public static string Ask_DeleteQuestion(this PointItemKeys pointItemKeys)
        {
            return "Ask_DeleteQuestion";
        }

        
        /// <summary>
        /// 被顶/赞同
        /// </summary>
        /// <returns></returns>
        public static string Ask_BeSupported(this PointItemKeys pointItemKeys)
        {
            return "Ask_BeSupported";
        }

        /// <summary>
        /// 被踩/反对
        /// </summary>
        /// <returns></returns>
        public static string Ask_BeOpposed(this PointItemKeys pointItemKeys)
        {
            return "Ask_BeOpposed";
        }
    }
}
