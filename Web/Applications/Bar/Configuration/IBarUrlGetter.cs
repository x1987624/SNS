//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

namespace Spacebuilder.Bar
{
    /// <summary>
    /// �����л�ȡ���ӵĽӿ�
    /// </summary>
    public interface IBarUrlGetter
    {
        /// <summary>
        /// �⻧����id
        /// </summary>
        string TenantTypeId { get; }

        /// <summary>
        /// ��̬ӵ��������
        /// </summary>
        int ActivityOwnerType { get; }

        /// <summary>
        /// �Ƿ�Ϊ˽��״̬
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        bool IsPrivate(long sectionId);

        /// <summary>
        /// ��ϸҳ��
        /// </summary>
        /// <param name="threadId">���ӵ�id</param>
        /// <returns></returns>
        string ThreadDetail(long threadId, bool onlyLandlord = false, SortBy_BarPost sortBy = SortBy_BarPost.DateCreated, int pageIndex = 1, long? anchorPostId = null, bool isAnchorPostList = false, long? childPostIndex = null);

        /// <summary>
        /// ������ϸ��ʾҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>������ϸ��ʾҳ��</returns>
        string SectionDetail(long sectionId, SortBy_BarThread? sortBy = null, bool? isEssential = null, long? categoryId = null);

        /// <summary>
        /// �༭\��������
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <param name="threadId">����id</param>
        /// <returns>�༭\��������</returns>
        string Edit(long sectionId, long? threadId = null);

        /// <summary>
        /// �༭�����ķ���
        /// </summary>
        /// <param name="threadId">���ӵ�id</param>
        /// <param name="postId">������id</param>
        /// <returns>�༭�����ķ���</returns>
        string EditPost(long threadId, long? postId = null);

        /// <summary>
        /// �û���ҳ
        /// </summary>
        /// <param name="userId">�û�id</param>
        /// <param name="sectionId">����id</param>
        /// <returns>�û���ҳ</returns>
        string UserSpaceHome(long userId, long? sectionId = null);

        /// <summary>
        /// �û�������ҳ��
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <param name="userId">�û�id</param>
        /// <returns>�û����ӵ���ϸ</returns>
        string UserThreads(long userId, long? sectionId = null);

        /// <summary>
        /// �û��Ļ���ҳ��
        /// </summary>
        /// <param name="sectionId">���ɵ�id</param>
        /// <param name="userId">�û�id</param>
        /// <returns>�û�������ҳ��</returns>
        string UserPosts(long userId, long? sectionId = null);

        /// <summary>
        /// �������ӣ�ǰ̨ҳ�棩
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns></returns>
        string ManageThreads(long sectionId);

        /// <summary>
        /// ���������ǰ̨ҳ�棩
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>��������</returns>
        string ManagePosts(long sectionId);

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <returns>������</returns>
        string ManageCategories(long sectionId);

        /// <summary>
        /// ��ǩ�µ�����
        /// </summary>
        /// <param name="sectionId">����id</param>
        /// <param name="tagName">��ǩ��</param>
        /// <returns>��ǩ�µ�����</returns>
        string ListByTag(string tagName, long? sectionId = null, SortBy_BarThread? sortBy = null, bool? isEssential = null);

        /// <summary>
        /// ��̨������ҳ
        /// </summary>
        /// <returns>��̨������ҳ</returns>
        string BackstageHome();

        /// <summary>
        /// ��̨��ݲپֲ�ҳ�������
        /// </summary>
        /// <returns></returns>
        string _ManageSubMenu();
    }
}