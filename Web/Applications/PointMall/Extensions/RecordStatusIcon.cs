using System.Web.Mvc;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 输出兑换申请状态图标的方法
    /// </summary>
    public static class HtmlHelperRecordStatusIconExtensions
    {
        /// <summary>
        /// 输出兑换申请状态图标的方法
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="approveStatus">兑换申请状态</param>
        public static MvcHtmlString ApproveStatusIcon(this HtmlHelper htmlHelper, ApproveStatus approveStatus)
        {
            TagBuilder span = new TagBuilder("span");
            switch (approveStatus)
            {
                case ApproveStatus.Pending:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-question tn-icon-inline");
                    span.MergeAttribute("title", "待处理");
                    break;
                case ApproveStatus.Approved:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-pass tn-icon-inline");
                    span.MergeAttribute("title", "已通过");
                    break;
                case ApproveStatus.Rejected:
                    span.MergeAttribute("class", "tn-icon tn-icon-exclamation tn-icon-inline");
                    span.MergeAttribute("title", "已拒绝");
                    break;
                default:
                    span.MergeAttribute("class", "tn-icon-colorful tn-icon-colorful-pass tn-icon-inline");
                    span.MergeAttribute("title", "待处理");
                    break;
            }

            return new MvcHtmlString(span.ToString());
        }
    }
}
