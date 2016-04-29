//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using Tunynet.Common;
using Fasterflect;

namespace Spacebuilder.Common
{
    /// <summary>
    /// �������ʺŻ�ȡ��
    /// </summary>
    public static class ThirdAccountGetterFactory
    {

        private static readonly object lockObject = new object();
        private static bool isInitialized;
        private static ConcurrentDictionary<string, ThirdAccountGetter> thirdAccountGetters = null;

        /// <summary>
        /// ��ʼ���������ʺŻ�ȡ��
        /// </summary>
        public static void InitializeAll()
        {
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        thirdAccountGetters = new ConcurrentDictionary<string, ThirdAccountGetter>();
                        var accountBindingService = DIContainer.Resolve<AccountBindingService>();
                        foreach (var accountType in accountBindingService.GetAccountTypes())
                        {
                            Type thirdAccountGetterClassType = Type.GetType(accountType.ThirdAccountGetterClassType);
                            if (thirdAccountGetterClassType != null)
                            {
                                ConstructorInvoker thirdAccountGetterConstructor = thirdAccountGetterClassType.DelegateForCreateInstance();
                                ThirdAccountGetter thirdAccountGetter = thirdAccountGetterConstructor() as ThirdAccountGetter;
                                if (thirdAccountGetter != null)
                                    thirdAccountGetters[accountType.AccountTypeKey] = thirdAccountGetter;
                            }
                        }
                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// ��ȡĳһ��ThirdAccountGetter
        /// </summary>
        /// <param name="accountTypeKey">accountTypeKey</param>
        /// <returns>����ThirdAccountGetter</returns>
        public static ThirdAccountGetter GetThirdAccountGetter(string accountTypeKey)
        {
            if (thirdAccountGetters != null && thirdAccountGetters.ContainsKey(accountTypeKey))
                return thirdAccountGetters[accountTypeKey];
            return null;
        }

    }
}