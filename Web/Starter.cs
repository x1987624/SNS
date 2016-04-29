﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Spacebuilder.UI;
using SpaceBuilder.Common;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Common.Configuration;
using Tunynet.Events;
using Tunynet.FileStore;
using Tunynet.Globalization;
using Tunynet.Logging;
using Tunynet.Logging.Log4Net;
using Tunynet.Search;
using Tunynet.Tasks;
using Tunynet.Tasks.Quartz;
using Tunynet.UI;
using Tunynet.Utilities;
using Tunynet.Email;
using System;
using Fasterflect;
using System.IO;
using System.Web;
using Tunynet.Common.Repositories;
using Tunynet.Repositories;
using Web;
using Spacebuilder.Web;
using System.Web.Optimization;
using System.Net.Http.Formatting;
using Spacebuilder.MobileClient.Common;
using System.Web.Http;
using System.Xml.Linq;

namespace Spacebuilder.Environments
{
    /// <summary>
    /// 启动应用程序并预热
    /// </summary>
    public class Starter
    {
        private static bool distributedDeploy = Utility.IsDistributedDeploy();

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            //初始化ResourceAccessor
            ResourceAccessor.Initialize("Spacebuilder.Web.Resources.Resource", typeof(Spacebuilder.Web.Resources.Resource).Assembly);

            InitializeDIContainer();
            InitializeMVC();
            InitializeApplication();
        }

        /// <summary>
        /// 初始化DI容器
        /// </summary>
        private static void InitializeDIContainer()
        {
            var containerBuilder = new ContainerBuilder();
            # region 运行环境及全局设置

            //获取web引用的所有Tunynet开头的程序集
            AssemblyName[] assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(n => n.Name.StartsWith("Tunynet")).ToArray();
            List<Assembly> assemblyList = assemblyNames.Select(n => Assembly.Load(n)).ToList();
            //获取web\bin下的所有Spacebuilder开头的的程序集
            IEnumerable<string> files = Directory.EnumerateFiles(HttpRuntime.BinDirectory, "Spacebuilder.*.dll");
            assemblyList.AddRange(files.Select(n => Assembly.Load(AssemblyName.GetAssemblyName(n))));
            Assembly[] assemblies = assemblyList.ToArray();

            //批量注入所有的Service
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Service")).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //批量注入所有的Repository
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Repository")).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //批量注入所有的EventMoudle
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => typeof(IEventMoudle).IsAssignableFrom(t)).As<IEventMoudle>().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //批量注入所有的UrlGetter
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("UrlGetter")).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //批量注入所有的DataGetter
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("DataGetter")).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //批量注入所有的Handler
            containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Handler")).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //解决SearchHistoryService的单实例问题
            containerBuilder.Register(c => new SearchHistoryService()).AsSelf().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            ////批量注入所有的ISearcher
            //containerBuilder.RegisterAssemblyTypes(assemblies).Where(t => typeof(ISearcher).IsAssignableFrom(t)).AsSelf().AsImplementedInterfaces().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //用户身份认证
            containerBuilder.Register(c => new FormsAuthenticationService()).As<IAuthenticationService>().PropertiesAutowired().InstancePerHttpRequest();

            //权限认证
            containerBuilder.Register(c => new Authorizer()).AsSelf().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).SingleInstance();

            //注册运行环境
            containerBuilder.Register(c => new DefaultRunningEnvironment()).As<IRunningEnvironment>().SingleInstance();

            //注册系统日志
            containerBuilder.Register(c => new Log4NetLoggerFactoryAdapter()).As<ILoggerFactoryAdapter>().SingleInstance();


            //注册默认的IStoreProvider
            if (Utility.IsFileDistributedDeploy())
            {
                string fileServerRootPath = ConfigurationManager.AppSettings["DistributedDeploy:FileServerRootPath"];
                string fileServerRootUrl = ConfigurationManager.AppSettings["DistributedDeploy:FileServerRootUrl"];
                string fileServerUsername = ConfigurationManager.AppSettings["DistributedDeploy:FileServerUsername"];
                string fileServerPassword = ConfigurationManager.AppSettings["DistributedDeploy:FileServerPassword"];

                containerBuilder.Register(c => new DefaultStoreProvider(fileServerRootPath, fileServerRootUrl, fileServerUsername, fileServerPassword)).As<IStoreProvider>().Named<IStoreProvider>("CommonStorageProvider").SingleInstance();
            }
            else
            {
                containerBuilder.Register(c => new DefaultStoreProvider(@"~/Uploads")).As<IStoreProvider>().Named<IStoreProvider>("CommonStorageProvider").SingleInstance();
            }

            //注册PageResourceManager
         
            string resourceSite = null;
            if (ConfigurationManager.AppSettings["PageResource:Site"] != null)
                resourceSite = ConfigurationManager.AppSettings["PageResource:Site"];

            containerBuilder.Register(c => new PageResourceManager("xyan 社区") { ResourceSite = resourceSite }).As<IPageResourceManager>().InstancePerHttpRequest();

            //注册Html信任标签配置
            containerBuilder.Register(c => new SafeTrustedHtml()).As<TrustedHtml>().SingleInstance();

            //注册任务调度器
            if (distributedDeploy)
            {
                containerBuilder.Register(c => new QuartzTaskScheduler(RunAtServer.Slave)).As<ITaskScheduler>().SingleInstance();
            }
            else
            {
                containerBuilder.Register(c => new QuartzTaskScheduler()).As<ITaskScheduler>().SingleInstance();
            }

            # endregion 运行环境及全局设置

            #region 范型注入

            containerBuilder.RegisterGeneric(typeof(SettingManager<>)).As(typeof(ISettingsManager<>)).SingleInstance().PropertiesAutowired();
            containerBuilder.RegisterGeneric(typeof(SettingsRepository<>)).As(typeof(ISettingsRepository<>)).SingleInstance().PropertiesAutowired();
            containerBuilder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).SingleInstance().PropertiesAutowired();

            #endregion

            # region 业务逻辑实现

            //用户UserId生成器
            containerBuilder.Register(c => new DefaultIdGenerator()).As<IdGenerator>().SingleInstance();

            //查询UserID与UserName的查询器
            containerBuilder.Register(c => new DefaultUserIdToUserNameDictionary()).As<UserIdToUserNameDictionary>().SingleInstance().PropertiesAutowired();

            //查询UserID与NickName的查询器
            containerBuilder.Register(c => new DefaultUserIdToNickNameDictionary()).As<UserIdToNickNameDictionary>().SingleInstance().PropertiesAutowired();

            //验证码管理器
            containerBuilder.Register(c => new DefaultCaptchaManager()).As<ICaptchaManager>().SingleInstance();

            //注册隐私指定对象验证器
            containerBuilder.Register(c => new UserPrivacySpecifyObjectValidator()).Named<IPrivacySpecifyObjectValidator>(SpecifyObjectTypeIds.Instance().User().ToString()).SingleInstance();
            containerBuilder.Register(c => new UserGroupPrivacySpecifyObjectValidator()).Named<IPrivacySpecifyObjectValidator>(SpecifyObjectTypeIds.Instance().UserGroup().ToString()).SingleInstance();

            //注册动态接收人获取器
            containerBuilder.Register(c => new UserActivityReceiverGetter()).Named<IActivityReceiverGetter>(ActivityOwnerTypes.Instance().User().ToString()).SingleInstance();

            //评论内容解析
            containerBuilder.Register(c => new CommentBodyProcessor()).As<ICommentBodyProcessor>().SingleInstance();

            //注册短网址
            containerBuilder.Register(c => new GoogleUrlShortner()).As<IUrlShortner>().SingleInstance();

            //注册缓存
            if (distributedDeploy)
            {
                containerBuilder.Register(c => new DefaultCacheService(new MemcachedCache(), new RuntimeMemoryCache(), 1.0F, true)).As<ICacheService>().SingleInstance();
            }
            else
            {
                containerBuilder.Register(c => new DefaultCacheService(new RuntimeMemoryCache(), 1.0F)).As<ICacheService>().SingleInstance();
            }

            //操作日志
            containerBuilder.Register(c => new OperatorInfoGetter()).As<IOperatorInfoGetter>().SingleInstance();
            containerBuilder.Register(c => new OperationLogService()).As<OperationLogService>().SingleInstance();

            # endregion 业务逻辑实现

            #region 站外提醒

            //通知提醒查询器
            containerBuilder.Register(c => new NoticeReminderAccessor()).As<IReminderInfoAccessor>().SingleInstance();

            //私信提醒查询器
            containerBuilder.Register(c => new MessageReminderAccessor()).As<IReminderInfoAccessor>().SingleInstance();

            //请求提醒查询器
            containerBuilder.Register(c => new InvitationReminderAccessor()).As<IReminderInfoAccessor>().SingleInstance();

            //提醒发送器
            containerBuilder.Register(c => new EmailReminderSender()).As<IReminderSender>().SingleInstance();

            #endregion 站外提醒

            # region 邮箱设置

            //126邮箱获取好友的全局设置
            containerBuilder.Register(c => new Email126ContactAccessor()).As<IEmailContactAccessor>().SingleInstance();

            //163邮箱获取好友的全局设置
            containerBuilder.Register(c => new Email163ContactAccessor()).As<IEmailContactAccessor>().SingleInstance();

            containerBuilder.Register(c => new EmailSettingsManager()).As<IEmailSettingsManager>().SingleInstance();

            #endregion 邮箱设置

            # region 全文检索

            //用户搜索搜索器
            containerBuilder.Register(c => new UserSearcher("找人", "~/App_Data/IndexFiles/User", true, 1)).As<ISearcher>().Named<ISearcher>(UserSearcher.CODE).SingleInstance();

            //关注用户搜索搜索器
            containerBuilder.Register(c => new FollowUserSearcher("关注用户", "~/App_Data/IndexFiles/FollowUser", false, 90)).As<ISearcher>().Named<ISearcher>(FollowUserSearcher.CODE).SingleInstance();

            //标签搜索搜索器
            containerBuilder.Register(c => new TagSearcher("标签", "~/App_Data/IndexFiles/Tag", true, 91)).As<ISearcher>().Named<ISearcher>(TagSearcher.CODE).SingleInstance();

            # endregion 全文检索

            //注册各应用模块的组件
            ApplicationConfig.InitializeAll(containerBuilder);

            MobileClientApplicationConfig.InitializeAll(containerBuilder);

            containerBuilder.RegisterControllers(assemblies).PropertiesAutowired();
            containerBuilder.RegisterSource(new ViewRegistrationSource());
            containerBuilder.RegisterModelBinders(assemblies);
            containerBuilder.RegisterModelBinderProvider();
            containerBuilder.RegisterFilterProvider();
            containerBuilder.RegisterModule(new AutofacWebTypesModule());

            IContainer container = containerBuilder.Build();

            //将Autofac容器中的实例注册到mvc自带DI容器中（这样才获取到每请求缓存的实例）
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            DIContainer.RegisterContainer(container);

            //将webapi的返回值设为Json格式
            var jsonFormatter = new JsonMediaTypeFormatter();
            GlobalConfiguration.Configuration.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
        }

        /// <summary>
        /// 初始化MVC环境
        /// </summary>
        private static void InitializeMVC()
        {
            //扩展控制器
            ControllerBuilder.Current.SetControllerFactory(typeof(TunynetControllerFactory));

            //自定义模型绑定
            ModelBinders.Binders.DefaultBinder = new CustomModelBinder();

            //增加对Cookie的模型绑定
            ValueProviderFactories.Factories.Add(new CookieValueProviderFactory());

            //使MVC支持皮肤机制的视图引擎
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ThemedViewEngine());

            //注册区域
            AreaRegistration.RegisterAllAreas();

            //注册全局过滤器
            GlobalFilters.Filters.Add(new ExceptionHandlerAttribute());
            GlobalFilters.Filters.Add(new TrackOnlineUserAttribute());
            GlobalFilters.Filters.Add(new HttpCompressAttribute());
            GlobalFilters.Filters.Add(new PauseSiteCheckAttribute());
            GlobalFilters.Filters.Add(new SecurityFilter());
            GlobalFilters.Filters.Add(new OutputResultFilter());
            GlobalFilters.Filters.Add(new IsBannedAttribute());
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// 初始化应用程序，加载基础数据
        /// </summary>
        private static void InitializeApplication()
        {
            //加载应用
            ApplicationService applicationService = DIContainer.Resolve<ApplicationService>();
            foreach (var application in applicationService.GetAll())
            {
                if (application.Config == null)
                    continue;
                application.Config.Load();
            }

            //注册事件处理程序
            IEnumerable<IEventMoudle> eventMoudles = DIContainer.Resolve<IEnumerable<IEventMoudle>>();
            foreach (var eventMoudle in eventMoudles)
            {
                eventMoudle.RegisterEventHandler();
            }

            //注册皮肤选择器
            ThemeService.RegisterThemeResolver("Channel", new ChannelThemeResolver());
            ThemeService.RegisterThemeResolver("UserSpace", new UserSpaceThemeResolver());
            ThemeService.RegisterThemeResolver("ControlPanel", new ControlPanelThemeResolver());

            //初始化敏感词过滤
            SensitiveWordService service = DIContainer.Resolve<SensitiveWordService>();
            IEnumerable<SensitiveWord> words = service.Gets();
            if (words != null)
            {
                WordFilter.Add(WordFilterTypeIds.Instance().SensitiveWord(), words.ToDictionary(n => n.Word, n => n.Replacement));
            }

            //注册标题图的配置
            TenantLogoSettings.RegisterSettings(LogoConfigManager.Instance().GetAllLogoConfigs());

            //初始化第三方帐号获取器
            ThirdAccountGetterFactory.InitializeAll();

            //注册标签URL获取管理器
            TagUrlGetterManager.RegisterGetter(TenantTypeIds.Instance().User(), new UserTagUrlGetter());

            //注册请求类型
            InvitationType.Register(new InvitationType { Key = InvitationTypeKeys.Instance().InviteFollow(), Name = "求关注", Description = "" });

            //注册搜索相关的计数服务
            CountService countService = new CountService(TenantTypeIds.Instance().Search());
            countService.RegisterCounts();          //注册计数服务
            countService.RegisterCountPerDay();     //需要统计阶段计数时，需注册每日计数服务
            countService.RegisterStageCount(CountTypes.Instance().SearchCount(), 7);    //阶段计数为：最近7天搜索计数

            //注册用户相关的计数服务
            CountService userCountService = new CountService(TenantTypeIds.Instance().User());
            userCountService.RegisterCounts();          //注册计数服务
            userCountService.RegisterCountPerDay();     //需要统计阶段计数时，需注册每日计数服务
            userCountService.RegisterStageCount(CountTypes.Instance().ReputationPointsCounts(), 7);    //阶段计数为：最近7天威望计数
            userCountService.RegisterStageCount(CountTypes.Instance().HitTimes(), 7);    //阶段计数为：最近7天浏览计数

            //注册标签相关的计数服务
            CountService perDayCountService = new CountService(TenantTypeIds.Instance().Tag());
            perDayCountService.RegisterCounts();          //注册计数服务
            perDayCountService.RegisterCountPerDay();     //需要统计阶段计数时，需注册每日计数服务
            perDayCountService.RegisterStageCount(CountTypes.Instance().ItemCounts(), 1, 7);    //阶段计数为：最近1,7天讨论次数计数

            //注册附件相关的计数服务
            CountService attachmentCountService = new CountService(TenantTypeIds.Instance().Attachment());
            attachmentCountService.RegisterCounts();//注册计数服务

            //注册站点公告相关的计数服务
            CountService announcementCountService = new CountService(TenantTypeIds.Instance().Announcement());
            announcementCountService.RegisterCounts();//注册计数服务
            announcementCountService.RegisterCountPerDay();//需要统计阶段计数时，需注册每日计数服务
            announcementCountService.RegisterStageCount(CountTypes.Instance().HitTimes(), 7);//阶段计数为：最近7天浏览计数

            //注册邮箱相关计数
            CountService emailCountService = new CountService(TenantTypeIds.Instance().Email());
            emailCountService.RegisterCounts();//注册计数服务
            emailCountService.RegisterCountPerDay();     //需要统计阶段计数时，需注册每日计数服务
            emailCountService.RegisterStageCount(CountTypes.Instance().UseCount(), 1);    //阶段计数为：最近1,7天讨论次数计数

            new EmailService().ReLoadSmtpSettings();

            //启动定时任务
            TaskSchedulerFactory.GetScheduler().Start();
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public static void Stop()
        {
            //保存定时任务状态
            try
            {
                new QuartzTaskScheduler().SaveTaskStatus();
            }
            catch { }

            //关闭全文检索索引
            if (!distributedDeploy)
            {
                IEnumerable<ISearcher> searchers = SearcherFactory.GetSearchersOfBaseLucene();
                foreach (var searcher in searchers)
                {
                    if (searcher.SearchEngine is SearchEngine)
                    {
                        ((SearchEngine)searcher.SearchEngine).Close();
                    }
                }
            }
        }
    }
}
