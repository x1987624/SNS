//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-8-17</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-8-17" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;

namespace Spacebuilder.Microblog
{
    public static class CountTypesExtension
    {
        /// <summary>
        /// 微博数
        /// </summary>
        public static string MicroblogCount(this CountTypes countTypes)
        {
            return "1";
        }

        /// <summary>
        /// 微博数
        /// </summary>
        public static string MicroblogTopicCount(this CountTypes countTypes)
        {
            return "2";
        }
    }
}
