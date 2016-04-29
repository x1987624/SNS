//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------


using Tunynet;
using System;
using Tunynet.Caching;
namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class AskSettings:IEntity
    {
        
        private int maxReward = 0;
        /// <summary>
        /// 问题的最大悬赏分值
        /// </summary>
        public int MaxReward
        {
            get { return maxReward; }
            set { maxReward = value; }
        }

        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(AskSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
        
    }
}