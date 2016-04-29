//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Xml.Linq;
using Autofac;
using Spacebuilder.Photo.EventModules;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Common.Configuration;
using Tunynet.Events;
using Tunynet.Globalization;
using System.Collections.Generic;


namespace Spacebuilder.Photo
{
    [Serializable]
    public class PhotoConfig : ApplicationConfig
    {
        private static int applicationId = 1003;
        private XElement tenantAttachmentSettingsElement;
        private XElement tenantCommentSettingsElement;

        /// <summary>
        /// 获取PhotoConfig实例
        /// </summary>
        public static PhotoConfig Instance()
        {
            ApplicationService applicationService = new ApplicationService();

            ApplicationBase app = applicationService.Get(applicationId);
            if (app != null)
                return app.Config as PhotoConfig;
            else
                return null;
        }


        public PhotoConfig(XElement xElement)
            : base(xElement)
        {
            this.tenantAttachmentSettingsElement = xElement.Element("tenantAttachmentSettings");
            this.tenantCommentSettingsElement = xElement.Element("tenantCommentSettings");
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
            get { return "Photo"; }
        }

        /// <summary>
        /// 获取PhotoApplication实例
        /// </summary>
        public override Type ApplicationType
        {
            get { return typeof(PhotoApplication); }
        }

        /// <summary>
        /// 应用初始化
        /// </summary>
        /// <param name="containerBuilder">容器构建器</param>
        public override void Initialize(ContainerBuilder containerBuilder)
        {
            //注册ResourceAccessor的应用资源
            ResourceAccessor.RegisterApplicationResourceManager(ApplicationId, "Spacebuilder.Photo.Resources.Resource", typeof(Spacebuilder.Photo.Resources.Resource).Assembly);

            //评论设置的注册
            TenantCommentSettings.RegisterSettings(tenantCommentSettingsElement);

            //注册附件设置
            TenantAttachmentSettings.RegisterSettings(tenantAttachmentSettingsElement);

            //注册全文检索搜索器
            containerBuilder.Register(c => new PhotoSearcher("相册", "~/App_Data/IndexFiles/Photo", true, 5)).As<ISearcher>().Named<ISearcher>(PhotoSearcher.CODE).SingleInstance();

            //问题应用数据统计
            containerBuilder.Register(c => new PhotoApplicationStatisticDataGetter()).Named<IApplicationStatisticDataGetter>(this.ApplicationKey).SingleInstance();

        }

        /// <summary>
        /// 应用加载
        /// </summary>
        public override void Load()
        {
            base.Load();

            OwnerDataSettings.RegisterStatisticsDataKeys(TenantTypeIds.Instance().User(), OwnerDataKeys.Instance().PhotoCount());
            //注册相册的计数服务
            CountService albumCountService = new CountService(TenantTypeIds.Instance().Album());
            albumCountService.RegisterCounts();

            //注册照片的计数服务
            CountService photoCountService = new CountService(TenantTypeIds.Instance().Photo());
            photoCountService.RegisterCounts();
            photoCountService.RegisterCountPerDay();
            photoCountService.RegisterStageCount(CountTypes.Instance().HitTimes(),7);

            //注册标签云标签链接接口实现
            TagUrlGetterManager.RegisterGetter(TenantTypeIds.Instance().Photo(), new PhotoTagUrlGetter());

            //添加应用管理员角色
            ApplicationAdministratorRoleNames.Add(applicationId, new List<string> { "PhotoAdministrator" });


        }
    }
}