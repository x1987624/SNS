//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using System;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城权限验证
    /// </summary>
    public static class AuthorizerExtension
    {
        /// <summary>
        /// 创建，编辑，删除商品，商品上下架，推荐，
        /// </summary>
        /// <remarks>
        /// 仅管理员
        /// </remarks>
        public static bool Gift_Manage(this Authorizer authorizer)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            if (authorizer.IsAdministrator(PointMallConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 积分换商品
        /// </summary>
        /// <remarks>
        /// 登录用户并且积分够的用户
        /// </remarks>
        public static bool Gift_Exchange(this Authorizer authorizer,PointGift gift)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.TradePoints>=gift.Price)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 申请接受，拒绝
        /// </summary>
        /// <remarks>
        /// 仅管理员
        /// </remarks>
        public static bool ExchangeRecord_Manage(this Authorizer authorizer)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            if (authorizer.IsAdministrator(PointMallConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }
    }
}