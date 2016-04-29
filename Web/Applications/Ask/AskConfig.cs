//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-11-07</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-11-07" version="0.5">创建</log>
//----------------------

using System;
using System.Xml.Linq;
using Autofac;
using Spacebuilder.Ask.EventModules;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Common.Configuration;
using Tunynet.Events;
using Tunynet.Globalization;
using System.Collections.Generic;


namespace Spacebuilder.Ask
{
    [Serializable]
    public class AskConfig : ApplicationConfig
    {
        private static int applicationId = 1013;
        private XElement tenantAttachmentSettingsElement;
        private XElement tenantCommentSettingsElement;

        /// <summary>
        /// 获取AskConfig实例
        /// </summary>
        public static AskConfig Instance()
        {
            ApplicationService applicationService = new ApplicationService();

            ApplicationBase app = applicationService.Get(applicationId);
            if (app != null)
                return app.Config as AskConfig;
            else
                return null;
        }


        public AskConfig(XElement xElement)
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
            get { return "Ask"; }
        }

        /// <summary>
        /// 问题使用关键字
        /// </summary>
        public string AskQuestion
        {
            get { return "AskQuestion"; }
        }

        /// <summary>
        /// 回答使用关键字
        /// </summary>
        public string AskAnswer
        {
            get { return "AskAnswer"; }
        }

        /// <summary>
        /// 获取AskApplication实例
        /// </summary>
        public override Type ApplicationType
        {
            get { return typeof(AskApplication); }
        }

        /// <summary>
        /// 应用初始化
        /// </summary>
        /// <param name="containerBuilder">容器构建器</param>
        public override void Initialize(ContainerBuilder containerBuilder)
        {
            //注册ResourceAccessor的应用资源
            ResourceAccessor.RegisterApplicationResourceManager(ApplicationId, "Spacebuilder.Ask.Resources.Resource", typeof(Spacebuilder.Ask.Resources.Resource).Assembly);

            //评论设置的注册
            TenantCommentSettings.RegisterSettings(tenantCommentSettingsElement);

            //注册附件设置
            TenantAttachmentSettings.RegisterSettings(tenantAttachmentSettingsElement);

            //注册问答正文解析器
            containerBuilder.Register(c => new AskBodyProcessor()).Named<IBodyProcessor>(TenantTypeIds.Instance().Ask()).SingleInstance();
                     
            //注册全文检索搜索器
            containerBuilder.Register(c => new AskSearcher("问答", "~/App_Data/IndexFiles/Ask", true, 8)).As<ISearcher>().Named<ISearcher>(AskSearcher.CODE).SingleInstance();

            //问题应用数据统计
            containerBuilder.Register(c => new AskQuestionApplicationStatisticDataGetter()).Named<IApplicationStatisticDataGetter>(this.ApplicationKey).SingleInstance();

            //注册问题解析器
            containerBuilder.Register(c => new AskBodyProcessor()).Named<IBodyProcessor>(TenantTypeIds.Instance().AskQuestion()).SingleInstance();

            //注册回答解析器
            containerBuilder.Register(c => new AskBodyProcessor()).Named<IBodyProcessor>(TenantTypeIds.Instance().AskAnswer()).SingleInstance();

            //注册动态接收人获取器
            containerBuilder.Register(c => new SubscribeQuestionActivityReceiverGetter()).Named<IActivityReceiverGetter>(ActivityOwnerTypes.Instance().AskQuestion().ToString()).SingleInstance();
            containerBuilder.Register(c => new SubscribeTagActivityReceiverGetter()).Named<IActivityReceiverGetter>(ActivityOwnerTypes.Instance().AskTag().ToString()).SingleInstance();
        }

        /// <summary>
        /// 应用加载
        /// </summary>
        public override void Load()
        {
            base.Load();

            //注册问题的计数服务
            CountService questionCountService = new CountService(TenantTypeIds.Instance().AskQuestion());
            questionCountService.RegisterCounts();

            //注册回答的计数服务
            CountService answerCountService = new CountService(TenantTypeIds.Instance().AskAnswer());
            answerCountService.RegisterCounts();

            //注册标签的计数服务
            CountService tagCountService = new CountService(TenantTypeIds.Instance().Tag());
            tagCountService.RegisterCounts();

            //注册用户提问数计数服务（用于内容计数）
            OwnerDataSettings.RegisterStatisticsDataKeys(TenantTypeIds.Instance().User(), OwnerDataKeys.Instance().QuestionCount(), OwnerDataKeys.Instance().AnswerCount());

            //注册标签云标签链接接口实现
            TagUrlGetterManager.RegisterGetter(TenantTypeIds.Instance().AskQuestion(), new AskTagUrlGetter());

            //添加应用管理员角色
            ApplicationAdministratorRoleNames.Add(applicationId, new List<string> { "AskAdministrator" });

        }
    }
}