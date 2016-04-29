//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-5-18</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-5-18" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// Id生成器（用于替代数据库非自增主键）
    /// </summary>
    public abstract class IdGenerator
    {
        #region Instance

        private static volatile IdGenerator _defaultInstance = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 获取EmailBuilder实例
        /// </summary>
        /// <returns></returns>
        private static IdGenerator Instance()
        {
            if (_defaultInstance == null)
            {
                lock (lockObject)
                {
                    if (_defaultInstance == null)
                    {
                        _defaultInstance = DIContainer.Resolve<IdGenerator>();
                        if (_defaultInstance == null)
                            throw new ExceptionFacade("未在DIContainer注册IdGenerator的具体实现类");
                    }
                }
            }
            return _defaultInstance;
        }

        #endregion
        
        /// <summary>
        /// 获取下一个Id
        /// </summary>
        /// <returns>
        /// 返回生成下一个Id
        /// </returns>
        public static long Next()
        {
            long nextId;
            lock (lockObject)
            {
                nextId = Instance().NextLong();
            }
            return nextId;
        }

        /// <summary>
        /// 获取下一个long类型的Id
        /// </summary>
        /// <returns>
        /// 返回生成下一个Id
        /// </returns>
        protected abstract long NextLong();

    }
}
