//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-11-07</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-11-07" version="0.5">创建</log>
//----------------------

using System.Configuration;
using System.Web.Mvc;
using Spacebuilder.Ask.Controllers;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答路由设置
    /// </summary>
    public class UrlRoutingRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Ask"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //对于IIS6.0默认配置不支持无扩展名的url
            string extensionForOldIIS = ".html";
            int iisVersion = 0;

            if (!int.TryParse(ConfigurationManager.AppSettings["IISVersion"], out iisVersion))
                iisVersion = 7;
            if (iisVersion >= 7)
                extensionForOldIIS = string.Empty;


            #region Channel

            //问答频道-问答首页
            context.MapRoute(
              "Channel_Ask_Home", // Route name
              "Ask" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "Home", CurrentNavigationId = "10101302" } // Parameter defaults
            );

            //问答频道-问题
            context.MapRoute(
              "Channel_Ask_Questions", // Route name
              "Ask/Questions" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "Questions", CurrentNavigationId = "10101303" } // Parameter defaults
            );
            
            //问答频道-问题详情页
            context.MapRoute(
              "Channel_Ask_QuestionDetail", // Route name
              "Ask/q-{questionId}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "QuestionDetail", CurrentNavigationId = "10101303" } // Parameter defaults
            );

            //问答频道-关注问题的用户
            context.MapRoute(
              "Channel_Ask_QuestionFollowers", // Route name
              "Ask/qf-{questionId}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "QuestionFollowers", CurrentNavigationId = "10101303" } // Parameter defaults
            );

            //问答频道-标签
            context.MapRoute(
              "Channel_Ask_Tags", // Route name
              "Ask/Tags" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "Tags", CurrentNavigationId = "10101304" } // Parameter defaults
            );

            //问答频道-标签详情页
            context.MapRoute(
              "Channel_Ask_TagDetail", // Route name
              "Ask/t-{tagName}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "TagDetail", CurrentNavigationId = "10101304" } // Parameter defaults
            );

            //问答频道-关注问题的用户
            context.MapRoute(
              "Channel_Ask_TagFollowers", // Route name
              "Ask/tf-{tagId}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "TagFollowers", CurrentNavigationId = "10101304" } // Parameter defaults
            );

            //问答频道-用户
            context.MapRoute(
              "Channel_Ask_Rank", // Route name
              "Ask/Rank" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "Rank", CurrentNavigationId = "10101305" } // Parameter defaults
            );

            //问答频道-我的问答
            context.MapRoute(
              "Channel_Ask_User", // Route name
              "Ask/u/{SpaceKey}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "AskUser", CurrentNavigationId = "10101306" } // Parameter defaults
            );

            //问答频道-我的问答
            context.MapRoute(
              "Channel_Ask_My", // Route name
              "Ask/My" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "AskUser", CurrentNavigationId = "10101306" } // Parameter defaults
            );

            //问答频道-提问
            context.MapRoute(
              "Channel_Ask_EditQuestion", // Route name
              "Ask/EditQuestion" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelAsk", action = "EditQuestion", CurrentNavigationId = "10101303" } // Parameter defaults
            );

            context.MapRoute(
                "Channel_Ask_Common", // Route name
                "Ask/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelAsk", action = "Home" } // Parameter defaults
            );

            #endregion

            #region ControlPanel
            context.MapRoute(
                "ControlPanel_Ask_Home", // Route name
                "ControlPanel/Content/Ask/ManageQuestions" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelAsk", action = "ManageQuestions", CurrentNavigationId = "20101301" } // Parameter defaults
            );

            context.MapRoute(
                "ControlPanel_Ask_Common", // Route name
                "ControlPanel/Content/Ask/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelAsk", CurrentNavigationId = "20101301" } // Parameter defaults
            );

            #endregion

            #region 动态列表控件路由

            context.MapRoute(
               string.Format("ActivityDetail_{0}_CreateAskQuestion", TenantTypeIds.Instance().AskQuestion()), // Route name
                "AskActivity/CreateAskQuestion/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "AskActivity", action = "_CreateAskQuestion" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_CreateAskAnswer", TenantTypeIds.Instance().AskAnswer()), // Route name
                "AskActivity/CreateAskAnswer/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "AskActivity", action = "_CreateAskAnswer" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_CommentAskQuestion", TenantTypeIds.Instance().Comment()), // Route name
                "AskActivity/CommentAskQuestion/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "AskActivity", action = "_CommentAskQuestion" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_CommentAskAnswer", TenantTypeIds.Instance().Comment()), // Route name
                "AskActivity/CommentAskAnswer/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "AskActivity", action = "_CommentAskAnswer" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_SupportAskAnswer", TenantTypeIds.Instance().AskAnswer()), // Route name
                "AskActivity/SupportAskAnswer/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "AskActivity", action = "_SupportAskAnswer" } // Parameter defaults
            );

            #endregion

        }
    }
}