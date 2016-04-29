//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-10-17</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-10-17" version="0.5">新建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志归档统计项目
    /// </summary>
    [Serializable]
    public class ArchiveItem
    {
        /// <summary>
        /// 归档阶段-年
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 归档阶段-月
        /// </summary>
        public int Month { get; set; }


        /// <summary>
        /// 归档阶段-日
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 该阶段的日志统计数
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 用于构建CacheKey
        /// </summary>
        public override string ToString()
        {
            return Year + "-" + Month + "-" + Day;
        }

    }
}