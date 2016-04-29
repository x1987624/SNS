using System;
using Autofac;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Logging;
using Tunynet.Logging.Log4Net;
using Tunynet.Search;
using Tunynet.Tasks;
using Tunynet.Tasks.Quartz;
using Tunynet.Email;
using System.Configuration;
using Tunynet.FileStore;

namespace Spacebuilder.Distribute.WcfWeb
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var containerBuilder = new ContainerBuilder();

            //注册运行环境
            containerBuilder.Register(c => new DefaultRunningEnvironment()).As<IRunningEnvironment>().SingleInstance();

            //注册系统日志
            containerBuilder.Register(c => new Log4NetLoggerFactoryAdapter()).As<ILoggerFactoryAdapter>().SingleInstance();

            //注册缓存
            containerBuilder.Register(c => new DefaultCacheService(new MemcachedCache(), 1.0F)).As<ICacheService>().SingleInstance();

            //注册IStoreProvider
            string fileServerRootPath = ConfigurationManager.AppSettings["DistributedDeploy:FileServerRootPath"];
            string fileServerRootUrl = ConfigurationManager.AppSettings["DistributedDeploy:FileServerRootUrl"];
            string fileServerUsername = ConfigurationManager.AppSettings["DistributedDeploy:FileServerUsername"];
            string fileServerPassword = ConfigurationManager.AppSettings["DistributedDeploy:FileServerPassword"];

            containerBuilder.Register(c => new DefaultStoreProvider(fileServerRootPath, fileServerRootUrl, fileServerUsername, fileServerPassword)).Named<IStoreProvider>("CommonStorageProvider").SingleInstance();
            containerBuilder.Register(c => new DefaultStoreProvider(fileServerRootPath, fileServerRootUrl, fileServerUsername, fileServerPassword)).As<IStoreProvider>().SingleInstance();

            //注册任务调度器
            containerBuilder.Register(c => new QuartzTaskScheduler(RunAtServer.Master)).As<ITaskScheduler>().SingleInstance();

            //通知提醒查询器
            containerBuilder.Register(c => new NoticeReminderAccessor()).As<IReminderInfoAccessor>().SingleInstance();

            IContainer container = containerBuilder.Build();
            DIContainer.RegisterContainer(container);

            //启动主控端定时任务
            TaskSchedulerFactory.GetScheduler().Start();

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

            //保存定时任务状态
            TaskSchedulerFactory.GetScheduler().SaveTaskStatus();

            //关闭全文检索索引
            foreach (SearchEngine searchEngine in SearchEngineService.searchEngines.Values)
            {
                searchEngine.Close();
            }
        }
    }
}