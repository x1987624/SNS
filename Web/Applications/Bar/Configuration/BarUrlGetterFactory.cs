//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// �����л�ȡ���ӵĽӿ�
    /// </summary>
    public static class BarUrlGetterFactory
    {
        /// <summary>
        /// ��ȡ���ӵķ���
        /// </summary>
        /// <param name="tenantTypeId">�⻧����id</param>
        /// <returns>��ȡ���ӵ�ʵ��</returns>
        public static IBarUrlGetter Get(string tenantTypeId)
        {
            return DIContainer.Resolve<IEnumerable<IBarUrlGetter>>().Where(n => n.TenantTypeId.Equals(tenantTypeId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
    }
}