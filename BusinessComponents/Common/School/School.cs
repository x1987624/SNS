
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Common.Repositories;
using Tunynet.Utilities;

namespace Tunynet.Common
{
    /// <summary>
    /// ѧУʵ����
    /// </summary>
    [TableName("tn_Schools")]
    [PrimaryKey("Id", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "AreaCode", ExpirationPolicy = EntityCacheExpirationPolicies.Stable)]
    [Serializable]
    public class School : IEntity
    {
        public static School New()
        {
            School school = new School()
            {
                Name = string.Empty,
                PinyinName = string.Empty,
                ShortPinyinName = string.Empty

            };
            return school;
        }
        /// <summary>
        /// ��ʶ��
        /// </summary>
        public long Id { get; protected set; }

        /// <summary>
        /// ԺУ����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���Ƶ�ƴ�������硰�����hanyu��
        /// </summary>
        public string PinyinName { get; set; }

        /// <summary>
        /// ���Ƶļ�дƴ�������硰����ļ�дƴ����hy��
        /// </summary>
        public string ShortPinyinName { get; set; }

        /// <summary>
        /// ѧУ����
        /// </summary>
        public SchoolType SchoolType { get; set; }

        /// <summary>
        /// ���ڵ�������
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public long DisplayOrder { get; set; }


        #region IEntity ��Ա

        object IEntity.EntityId { get { return this.Id; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
    }
}
