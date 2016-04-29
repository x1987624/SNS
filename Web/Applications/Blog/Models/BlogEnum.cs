//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-10-16" version="0.5">新建</log>
//--------------------------------------------------------------
//</TunynetCopyright>


namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志排序依据
    /// </summary>
    public enum SortBy_BlogThread
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        DateCreated_Desc,
        
        /// <summary>
        /// 评论数
        /// </summary>
        CommentCount,

        /// <summary>
        /// 阶段浏览数
        /// </summary>
        StageHitTimes
    }


    /// <summary>
    /// 日志归档阶段
    /// </summary>
    public enum ArchivePeriod
    {
        /// <summary>
        /// 按年
        /// </summary>
        Year = 1,

        /// <summary>
        /// 按月
        /// </summary>
        Month = 2,

        /// <summary>
        /// 按天
        /// </summary>
        Day = 3
    }
}
