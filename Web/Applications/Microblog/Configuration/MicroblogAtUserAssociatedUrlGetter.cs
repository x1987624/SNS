﻿//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2013-03-21</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2013-03-21" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using Tunynet.Common;
using Spacebuilder.Common;
using System;
using Tunynet.Globalization;
using Tunynet.Utilities;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// At用户关联项Url获取
    /// </summary>
    public class MicroblogAtUserAssociatedUrlGetter : IAtUserAssociatedUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().Microblog(); }
        }

        public AssociatedInfo GetAssociatedInfo(long associateId, string tenantTypeId = "")
        {
            MicroblogService microblogService = new MicroblogService();
            MicroblogEntity microblog = microblogService.Get(associateId);

            if (microblog != null)
            {
                IMicroblogUrlGetter urlGetter = MicroblogUrlGetterFactory.Get(microblog.TenantTypeId);
                return new AssociatedInfo()
                {
                    DetailUrl = urlGetter.MicroblogDetail(microblog.MicroblogId),
                    Subject = HtmlUtility.TrimHtml(microblog.GetResolvedBody(), 16)
                };
            }
            return null;
        }


        public string GetOwner()
        {
            return "微博";
        }
    }
}