//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-14</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-14" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using PetaPoco;
using Tunynet.Caching;

namespace Tunynet.Tasks
{
    /// <summary>
    /// 任务规则组成部分
    /// </summary>
    public enum RulePart
    {
        /// <summary>
        /// 秒域
        /// </summary>
        seconds = 0,
        /// <summary>
        /// 分钟域
        /// </summary>
        minutes = 1,
        /// <summary>
        /// 小时域
        /// </summary>
        hours = 2,
        /// <summary>
        /// 日期域
        /// </summary>
        day = 3,
        /// <summary>
        /// 规则月部分 
        /// </summary>
        mouth = 4,
        /// <summary>
        /// 星期域
        /// </summary>
        dayofweek = 5
    }

    /// <summary>
    /// 任务频率
    /// </summary>
    public enum TaskFrequency
    {
        /// <summary>
        /// 每周
        /// </summary>
        Weekly = 0,

        /// <summary>
        /// 每月
        /// </summary>
        PerMonth = 1,

        /// <summary>
        /// 每天
        /// </summary>
        EveryDay = 2,
    }

    /// <summary>
    /// 任务在哪台服务器上运行
    /// </summary>
    public enum RunAtServer
    {
        /// <summary>
        /// 分布式环境下的集群服务器主控端
        /// </summary>
        Master = 0,

        /// <summary>
        /// 分布式环境下的集群服务器计算节点
        /// </summary>
        Slave = 1,

        /// <summary>
        /// 分布式环境下的搜索服务器
        /// </summary>
        Search = 2,
    }
}

