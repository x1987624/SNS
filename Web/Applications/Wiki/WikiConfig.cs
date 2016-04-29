//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Xml.Linq;
using Autofac;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Common.Configuration;
using Tunynet.Events;
using Tunynet.Globalization;
using System.Collections.Generic;
using Spacebuilder.Wiki.EventModules;


namespace Spacebuilder.Wiki
{
    public class WikiConfig : ApplicationConfig
    {
        private static int applicationId = 1016;
        private XElement tenantAttachmentSettingsElement;

        /// <summary>
        /// 获取WikiConfig实例
        /// </summary>
        public static WikiConfig Instance()
        {
            ApplicationService applicationService = new ApplicationService();

            ApplicationBase app = applicationService.Get(applicationId);
            if (app != null)
                return app.Config as WikiConfig;
            else
                return null;
        }


        public WikiConfig(XElement xElement)
            : base(xElement)
        {
            this.tenantAttachmentSettingsElement = xElement.Element("tenantAttachmentSettings");
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
            get { return "Wiki"; }
        }


        /// <summary>
        /// 获取WikiApplication实例
        /// </summary>
        public override Type ApplicationType
        {
            get { return typeof(WikiApplication); }
        }

        /// <summary>
        /// 应用初始化
        /// </summary>
        /// <param name="containerBuilder">容器构建器</param>
        public override void Initialize(ContainerBuilder containerBuilder)
        {
            //注册ResourceAccessor的应用资源
            ResourceAccessor.RegisterApplicationResourceManager(ApplicationId, "Spacebuilder.Wiki.Resources.Resource", typeof(Spacebuilder.Wiki.Resources.Resource).Assembly);

            //注册附件设置
            TenantAttachmentSettings.RegisterSettings(tenantAttachmentSettingsElement);
            //注册百科正文解析器
            containerBuilder.Register(c => new WikiBodyProcessor()).Named<IBodyProcessor>(TenantTypeIds.Instance().WikiPage()).SingleInstance();
            containerBuilder.Register(c => new DefaultPageIdToTitleDictionary()).As<PageIdToTitleDictionary>().SingleInstance();

            //注册全文检索搜索器
            containerBuilder.Register(c => new WikiSearcher("百科", "~/App_Data/IndexFiles/Wiki", true, 3)).As<ISearcher>().Named<ISearcher>(WikiSearcher.CODE).SingleInstance();


            containerBuilder.Register(c => new WikiApplicationStatisticDataGetter()).Named<IApplicationStatisticDataGetter>(this.ApplicationKey).SingleInstance();
        }

        /// <summary>
        /// 应用加载
        /// </summary>
        public override void Load()
        {
            base.Load();
            //注册词条计数服务
            CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());

            //注册计数服务
            countService.RegisterCounts();

            //需要统计阶段计数时，需注册每日计数服务
            countService.RegisterCountPerDay();

            //注册词条浏览计数服务
            countService.RegisterStageCount(CountTypes.Instance().HitTimes(), 7);

            //注册标签云标签链接接口实现
            TagUrlGetterManager.RegisterGetter(TenantTypeIds.Instance().WikiPage(), new WikiTagUrlGetter());

            //添加应用管理员角色
            ApplicationAdministratorRoleNames.Add(applicationId, new List<string> { "WikiAdministrator" });

        }
    }
}