//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet;
using System.Linq;
using System.Collections.Generic;
using System;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    public class BarUrlGetter : IBarUrlGetter
    {
        /// <summary>
        /// �⻧����id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().Bar(); }
        }

        /// <summary>
        /// ��̬ӵ��������
        /// </summary>
        public int ActivityOwnerType
        {
            get { return ActivityOwnerTypes.Instance().BarSection(); }
        }

        /// <summary>
        /// �Ƿ�Ϊ˽��״̬
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public bool IsPrivate(long sectionId)
        {
            return false;
        }

        /// <summary>
        /// ������ϸ��ʾҳ��
        /// </summary>
        /// <param name="threadId">����id</param>
        /// <param name="onlyLandlord">ֻ��¥��</param>
        /// <param name="sortBy">����ʽ</param>
        /// <param name="pageIndex">ҳ��</param>
        /// <returns>������ϸ��ʾҳ��</returns>
        public string ThreadDetail(long threadId, bool onlyLandlord = false, SortBy_BarPost sortBy = SortBy_BarPost.DateCreated, int pageIndex = 1, long? anchorPostId = null, bool isAnchorPostList = false, long? childPostIndex = null)
        {
            return SiteUrls.Instance().ThreadDetail(threadId, onlyLandlord, sortBy, pageIndex, anchorPostId, isAnchorPostList, childPostIndex);
        }

        /// <summary>
        /// ������ϸ��ʾҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>������ϸ��ʾҳ��</returns>
        public string SectionDetail(long sectionId, SortBy_BarThread? sortBy = null, bool? isEssential = null, long? categoryId = null)
        {
            return SiteUrls.Instance().SectionDetail(sectionId, sortBy, isEssential, categoryId);
        }

        /// <summary>
        /// ����/�༭����ҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <param name="threadId">����id</param>
        /// <returns>����/�༭����ҳ��</returns>
        public string Edit(long sectionId, long? threadId)
        {
            return SiteUrls.Instance().BarThreadEdit(sectionId, threadId);
        }

        /// <summary>
        /// �༭����ҳ��
        /// </summary>
        /// <param name="threadId">����id</param>
        /// <param name="postId">����id</param>
        /// <returns>�༭����ҳ��</returns>
        public string EditPost(long threadId, long? postId)
        {
            return SiteUrls.Instance().EditPost(threadId, postId);
        }

        /// <summary>
        /// �û�����ҳ��
        /// </summary>
        /// <param name="userId">�û�id</param>
        /// <param name="sectionId">����id</param>
        /// <returns>�û�����ҳ��</returns>
        public string UserThreads(long userId, long? sectionId = null)
        {
            return SiteUrls.Instance().UserThreads(userId, false);
        }

        /// <summary>
        /// �û��Ļ���ҳ��
        /// </summary>
        /// <param name="userId">�û�id</param>
        /// <param name="sectionId">����id</param>
        /// <returns>�û��Ļ���ҳ��</returns>
        public string UserPosts(long userId, long? sectionId = null)
        {
            return SiteUrls.Instance().UserThreads(userId, true);
        }

        /// <summary>
        /// ��������ҳ��
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public string ManageThreads(long sectionId)
        {
            return SiteUrls.Instance().ManageThreadsForSection(sectionId);
        }

        /// <summary>
        /// �������ҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>�������ҳ��</returns>
        public string ManagePosts(long sectionId)
        {
            return SiteUrls.Instance().ManagePostsForSection(sectionId);
        }

        /// <summary>
        /// �����������ҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>�����������ҳ��</returns>
        public string ManageCategories(long sectionId)
        {
            return SiteUrls.Instance().ManageThreadCategoriesForSection(sectionId);
        }

        /// <summary>
        /// ��ǩ�µ�����ҳ��
        /// </summary>
        /// <param name="tagName">��ǩ��</param>
        /// <param name="sectionId">����id</param>
        /// <returns>��ǩ�µ�����ҳ��</returns>
        public string ListByTag(string tagName, long? sectionId = null, SortBy_BarThread? sortBy = null, bool? isEssential = null)
        {
            return SiteUrls.Instance().ListsByTag(tagName, sortBy, isEssential);
        }

        /// <summary>
        /// ��̨������ҳ
        /// </summary>
        /// <returns></returns>
        public string BackstageHome()
        {
            return SiteUrls.Instance().ManageThreads();
        }


        /// <summary>
        /// ��̨����ֲ�ҳ
        /// </summary>
        /// <returns></returns>
        public string _ManageSubMenu()
        {
            return "~/Applications/Bar/Views/ControlPanelBar/_ManageBarRightMenuShortcut.cshtml";
        }

        /// <summary>
        /// �û���ҳ
        /// </summary>
        /// <param name="userId">�û���ҳ</param>
        /// <param name="sectionId">����id</param>
        /// <returns>�û���ҳ</returns>
        public string UserSpaceHome(long userId, long? sectionId = null)
        {
            return SiteUrls.Instance().UserThreads(userId);
        }

    }
}