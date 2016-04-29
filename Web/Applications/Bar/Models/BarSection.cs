//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>bianchx</author>
//<email>bianchx@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Common;
using Spacebuilder.Common;
using Tunynet;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖吧
    /// </summary>
    [TableName("spb_BarSections")]
    [PrimaryKey("SectionId", autoIncrement = false)]
    [CacheSetting(true)]
    [Serializable]
    public class BarSection : SerializablePropertiesBase, IAuditable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        //todo:需要检查成员初始化的类型是否正确
        public static BarSection New()
        {
            BarSection barSection = new BarSection()
            {
                Name = string.Empty,
                LogoImage = string.Empty,
                DateCreated = DateTime.UtcNow,
                Description = string.Empty,
                DisplayOrder = 100

            };
            return barSection;
        }

        #region 需持久化属性

        /// <summary>
        ///SectionId
        /// </summary>
        public long SectionId { get; set; }

        /// <summary>
        ///帖吧租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///帖吧拥有者Id（例如：活动Id、群组Id），若是帖吧应用，则OwnerId为0
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///吧主用户Id（若是活动/群组，则对应活动/群组创建者Id）
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///帖吧名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///帖吧描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///Logo存储图片名称
        /// </summary>
        public string LogoImage { get; set; }

        /// <summary>
        ///是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///是否启用RSS
        /// </summary>
        public bool EnableRss { get; set; }

        /// <summary>
        ///主题分类状态 0=禁用；1=启用（不强制）；2=启用（强制）
        /// </summary>
        public ThreadCategoryStatus ThreadCategoryStatus { get; set; }


        /// <summary>
        ///排序序号
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.SectionId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region 扩展属性
        /// <summary>
        /// 判断是否有Logo
        /// </summary>
        [Ignore]
        public bool HasLogoImage
        {
            get
            {
                return !string.IsNullOrEmpty(LogoImage) && !string.IsNullOrEmpty(LogoImage.Trim());
            }
        }


        /// <summary>
        /// 吧管理员列表
        /// </summary>
        [Ignore]
        public IEnumerable<User> SectionManagers
        {
            get
            {
                return new BarSectionService().GetSectionManagers(this.SectionId);
            }
        }


        /// <summary>
        /// 吧主
        /// </summary>
        [Ignore]
        public User User
        {
            get
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                return userService.GetFullUser(this.UserId);
            }
        }
        
        

        /// <summary>
        /// 帖吧分类
        /// </summary>
        [Ignore]
        public Category Category
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(SectionId, 0, TenantTypeIds.Instance().BarSection());
                return categories == null || categories.Count() == 0 ? null : categories.FirstOrDefault();
            }
        }

        /// <summary>
        /// 贴吧下所有帖子的分类
        /// </summary>
        [Ignore]
        public IEnumerable<Category> ThreadCategories
        {
            get { return new CategoryService().GetOwnerCategories(this.SectionId, TenantTypeIds.Instance().BarThread()); }
        }

        #endregion

        #region IAuditable 实现

        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Bar_Section(); }
        }

        /// <summary>
        ///审核状态
        /// </summary>
        //[SqlBehavior(~SqlBehaviorFlags.Update)]
        public AuditStatus AuditStatus { get; set; }

        #endregion

           #region 计数

        /// <summary>
        /// 主题帖数
        /// </summary>
        [Ignore]
        public int ThreadCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                return countService.Get(CountTypes.Instance().ThreadCount(), this.SectionId);
            }
        }

        /// <summary>
        /// 主题帖和回帖总数
        /// </summary>
        [Ignore]
        public int ThreadAndPostCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                return countService.Get(CountTypes.Instance().ThreadAndPostCount(), this.SectionId);
            }
        }

        /// <summary>
        /// 今日主题帖和回帖总数
        /// </summary>
        [Ignore]
        public int ToDayThreadAndPostCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                return countService.GetStageCount(CountTypes.Instance().ThreadAndPostCount(), 1, this.SectionId);
            }
        }

        /// <summary>
        ///被关注数
        /// </summary>
        [Ignore]
        public int FollowedCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                return countService.Get(CountTypes.Instance().FollowedCount(), this.SectionId);
            }
        }

        #endregion
    }
}