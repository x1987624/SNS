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
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Quartz;
using System.Collections.Generic;
using PetaPoco;
using Tunynet.Caching;

namespace Tunynet.Tasks
{
    /// <summary>
    /// 任务详细信息实体
    /// </summary>
    [TableName("tn_TaskDetails")]
    [PrimaryKey("Id", autoIncrement = true)]
    [CacheSetting(false)]
    [Serializable]
    public class TaskDetail : IEntity
    {
        /// <summary>
        /// 实例化实体
        /// </summary>
        /// <remarks>实例化实体时先根据taskName从数据库中获取，如果取不到则创建新实例</remarks>
        public static TaskDetail New()
        {
            TaskDetail taskDetail = new TaskDetail();
            return taskDetail;
        }

        #region 需要持久化的属性

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 任务的执行时间规则
        /// </summary>
        public string TaskRule { get; set; }

        /// <summary>
        /// 程序重启后立即执行
        /// </summary>
        /// <remarks>在应用程序池重启后,是否检查此任务上次被执行过，如果没有执行则立即执行</remarks>
        public bool RunAtRestart { get; set; }

        /// <summary>
        /// 任务在哪台服务器上运行
        /// </summary>
        public RunAtServer RunAtServer { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        /// <remarks>用于任务实例化</remarks>
        public string ClassType { get; set; }

        /// <summary>
        /// 上次执行开始时间
        /// </summary>
        public DateTime? LastStart { get; set; }

        /// <summary>
        /// 上次执行结束时间
        /// </summary>
        public DateTime? LastEnd { get; set; }

        /// <summary>
        /// 上次任务执行状态
        /// </summary>
        /// <remarks>true-成功/false-失败</remarks>
        public bool? LastIsSuccess { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextStart { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.Id; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        /// <summary>
        /// 获取规则指定部分
        /// </summary>
        /// <param name="rulePart">规则组成部分</param>
        /// <returns></returns>
        public string GetRulePart(RulePart rulePart)
        {
            if (string.IsNullOrEmpty(TaskRule))
            {
                return RulePart.dayofweek == rulePart ? null : "1";
            }

            string part = TaskRule.Split(' ').GetValue((int)rulePart).ToString();
            if (part == "*" || part == "?")
                return RulePart.dayofweek == rulePart ? null : "1";

            if (part.Contains("/"))
            {
                return part.Substring(part.IndexOf("/") + 1);
            }

            return part;
        }
    }
}

