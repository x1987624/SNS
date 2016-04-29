using System.Web.Mvc;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 输出问题状态图标的方法
    /// </summary>
    public static class HtmlHelperQuestionStatusIconExtensions
    {
        /// <summary>
        /// 输出问题状态图标的方法
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="questionStatus">问题状态</param>
        public static MvcHtmlString QuestionStatusIcon(this HtmlHelper htmlHelper, QuestionStatus questionStatus)
        {
            TagBuilder span = new TagBuilder("span");
            switch (questionStatus)
            {
                case QuestionStatus.Unresolved:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-question tn-icon-inline");
                    span.MergeAttribute("title", "未解决");
                    break;
                case QuestionStatus.Resolved:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-pass tn-icon-inline");
                    span.MergeAttribute("title", "已解决");
                    break;
                case QuestionStatus.Canceled:
                    span.MergeAttribute("class", "tn-icon tn-icon-exclamation tn-icon-inline");
                    span.MergeAttribute("title", "已取消");
                    break;
                default:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-pass tn-icon-inline");
                    span.MergeAttribute("title", "已解决");
                    break;
            }

            return new MvcHtmlString(span.ToString());
        }
    }
}
