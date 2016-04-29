//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-11-07</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-11-07" version="0.5">创建</log>
//----------------------

using Tunynet.Common;
using Spacebuilder.Common;
using Tunynet;
using System;

namespace Spacebuilder.Ask
{
    [Serializable]
    public class AskApplication : ApplicationBase
    {
        protected AskApplication(ApplicationModel model)
            : base(model)
        { }

        /// <summary>
        /// 删除用户在应用中的数据
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="takeOverUserName">用于接管删除用户时不能删除的内容(例如：用户创建的群组)</param>
        /// <param name="isTakeOver">是否接管被删除用户可被接管的内容</param>
        protected override void DeleteUser(long userId, string takeOverUserName, bool isTakeOver)
        {
            AskService askService = DIContainer.Resolve<AskService>();
            askService.DeleteUser(userId, takeOverUserName, isTakeOver);
        }

        protected override bool Install(string presentAreaKey, long ownerId)
        {
            return true;
        }

        protected override bool UnInstall(string presentAreaKey, long ownerId)
        {
            return true;
        }
    }
}