//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Events
{
    /// <summary>
    /// 事件操作类型
    /// </summary>
    public class EventOperationType
    {
        #region Instance
        private static volatile EventOperationType _instance = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns>返回EventOperationType对象</returns>
        public static EventOperationType Instance()
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new EventOperationType();
                    }
                }
            }
            return _instance;
        }

        private EventOperationType()
        { }
        #endregion



        /// <summary>
        /// 创建
        /// </summary>
        public string Create()
        {
            return "Create";
        }

        /// <summary>
        /// 更新
        /// </summary>
        public string Update()
        {
            return "Update";
        }

        /// <summary>
        /// 删除
        /// </summary>
        public string Delete()
        {
            return "Delete";
        }

        /// <summary>
        /// 通过审核
        /// </summary>
        public string Approved()
        {
            return "Approved";
        }

        /// <summary>
        /// 不通过审核
        /// </summary>
        public string Disapproved()
        {
            return "Disapproved";
        }

        /// <summary>
        /// 设置精华
        /// </summary>
        public string SetEssential()
        {
            return "SetEssential";
        }

        /// <summary>
        /// 取消精华
        /// </summary>
        public string CancelEssential()
        {
            return "CancelEssential";
        }

        /// <summary>
        /// 设置置顶
        /// </summary>
        public string SetSticky()
        {
            return "SetSticky";
        }

        /// <summary>
        /// 取消置顶
        /// </summary>
        public string CancelSticky()
        {
            return "CancelSticky";
        }

        /// <summary>
        /// 设置分类
        /// </summary>
        public string SetCategory()
        {
            return "SetCategory";
        }

        /// <summary>
        /// 受控查看
        /// </summary>        
        public string ControlledView()
        {
            return "ControlledView";
        }


    }
}
