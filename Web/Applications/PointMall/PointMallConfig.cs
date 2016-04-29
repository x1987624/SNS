//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Xml.Linq;
using Autofac;
using Spacebuilder.PointMall.EventModules;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Common.Configuration;
using Tunynet.Events;
using Tunynet.Globalization;
using System.Collections.Generic;


namespace Spacebuilder.PointMall
{
    public class PointMallConfig : ApplicationConfig
    {
        private static int applicationId = 2001;
        private XElement tenantAttachmentSettingsElement;
        private XElement priceSettingElement;

        /// <summary>
        /// 获取PhotoConfig实例
        /// </summary>
        public static PointMallConfig Instance()
        {
            ApplicationService applicationService = new ApplicationService();

            ApplicationBase app = applicationService.Get(applicationId);
            if (app != null)
                return app.Config as PointMallConfig;
            else
                return null;
        }


        public PointMallConfig(XElement xElement)
            : base(xElement)
        {
            this.tenantAttachmentSettingsElement = xElement.Element("tenantAttachmentSettings");
            this.priceSettingElement = xElement.Element("priceSetting");
        }

        /// <summary>
        /// ApplicationId
        /// </summary>
        public override int ApplicationId
        {
            get { return applicationId; }
        }

        /// <summary>
        /// ApplicationKey
        /// </summary>
        public override string ApplicationKey
        {
            get { return "PointMall"; }
        }

        /// <summary>
        /// 获取PhotoApplication实例
        /// </summary>
        public override Type ApplicationType
        {
            get { return typeof(PointMallApplication); }
        }

        /// <summary>
        /// 应用初始化
        /// </summary>
        /// <param name="containerBuilder">容器构建器</param>
        public override void Initialize(ContainerBuilder containerBuilder)
        {
            //注册ResourceAccessor的应用资源
            ResourceAccessor.RegisterApplicationResourceManager(ApplicationId, "Spacebuilder.PointMall.Resources.Resource", typeof(Spacebuilder.PointMall.Resources.Resource).Assembly);

            //注册附件设置
            TenantAttachmentSettings.RegisterSettings(tenantAttachmentSettingsElement);

            //问题应用数据统计
            containerBuilder.Register(c => new PointMallApplicationStatisticDataGetter()).Named<IApplicationStatisticDataGetter>(this.ApplicationKey).SingleInstance();
        }

        /// <summary>
        /// 应用加载
        /// </summary>
        public override void Load()
        {
            base.Load();

            //注册价格设置
            PriceSetting.RegisterSettings(this.priceSettingElement);
            
            //注册商品的计数服务
            CountService countService = new CountService(TenantTypeIds.Instance().PointGift());
            countService.RegisterCounts();

            //注册申请计数服务
            List<string> tenantTypeIds = new List<string>() { TenantTypeIds.Instance().User(), TenantTypeIds.Instance().Group() };
            OwnerDataSettings.RegisterStatisticsDataKeys(tenantTypeIds, OwnerDataKeys.Instance().PostCount());

            //添加应用管理员角色
            ApplicationAdministratorRoleNames.Add(applicationId, new List<string> { "PointMallAdministrator" });

        }
    }
}