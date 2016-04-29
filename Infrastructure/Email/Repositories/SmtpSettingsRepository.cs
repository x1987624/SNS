//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2013-1-9</createdate>
//<author>liucp</author>
//<email>liucp@tunynet.com</email>
//<log date="2013-1-9" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Repositories;
using Tunynet.Utilities;
using Tunynet.Email;

namespace Tunynet.Email
{
    /// <summary>
    /// Smtp设置的数据访问类
    /// </summary>
    public class SmtpSettingsRepository : Repository<SmtpSettings>, ISmtpSettingsRepository
    {
    }
}
