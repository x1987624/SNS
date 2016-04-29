//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Tunynet.Caching;
using Tunynet.Repositories;

namespace Spacebuilder.CMS.Metadata
{
    /// <summary>
    /// 模型定义服务
    /// </summary>
    public class MetadataService
    {
        /// <summary>
        /// 附表必需的字段（用于与主表关联）
        /// </summary>
        private static readonly string AddOnTableNecessaryColumnName = "ContentItemId";

        #region Repository
        private Repository<ContentTypeDefinition> contentTypeDefinitionRepository;
        private ContentTypeColumnDefinitionRepository contentTypeColumnDefinitionRepository;
        private Repository<FormControlDefinition> formControlDefinitionRepository;
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        public MetadataService()
        {
            contentTypeDefinitionRepository = new Repository<ContentTypeDefinition>();
            contentTypeColumnDefinitionRepository = new ContentTypeColumnDefinitionRepository();
            formControlDefinitionRepository = new Repository<FormControlDefinition>();
        }

        #region ContentType

        /// <summary>
        /// 获取内容模型
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public ContentTypeDefinition GetContentType(int contentTypeId)
        {
            return contentTypeDefinitionRepository.Get(contentTypeId);
        }

        /// <summary>
        /// 获取内容模型
        /// </summary>
        /// <param name="contentTypeKey"></param>
        /// <returns></returns>
        public ContentTypeDefinition GetContentType(string contentTypeKey)
        {
            return GetContentTypes(null).FirstOrDefault(n => n.ContentTypeKey == contentTypeKey);
        }

        /// <summary>
        /// 获取内容模型列表
        /// </summary>
        /// <param name="IsEnabled">是否启用</param>
        /// <returns></returns>
        public IEnumerable<ContentTypeDefinition> GetContentTypes(bool? IsEnabled)
        {
            if (IsEnabled.HasValue)
                return contentTypeDefinitionRepository.GetAll().Where(x => x.IsEnabled == IsEnabled);
            else
                return contentTypeDefinitionRepository.GetAll();
        }

        ///// <summary>
        ///// 创建内容模型
        ///// </summary>
        ///// <param name="contentType"></param>
        //public static bool CreateContentType(ContentTypeDefinition contentType, out string errorMessage)
        //{
        //    CreateColumnCommand command = new CreateColumnCommand(contentType.TableName, AddOnTableNecessaryColumnName)
        //        .WithType(System.Data.DbType.Int32).NotNull().PrimaryKey();

        //    SchemaBuilder schemaBuilder = new SchemaBuilder();
        //    schemaBuilder.CreateTable(command);
        //    if (!schemaBuilder.Execute(out errorMessage))
        //        return false;

        //    contentTypeDefinitionRepository.Insert(contentType);
        //    return true;
        //}

        ///// <summary>
        ///// 更新内容模型
        ///// </summary>
        ///// <remarks>
        ///// 不允许修改ContentType的TableName
        ///// </remarks>
        ///// <param name="contentType"></param>
        //public static void UpdateContentType(ContentTypeDefinition contentType)
        //{
        //    contentTypeDefinitionRepository.Update(contentType);
        //}

        ///// <summary>
        ///// 删除内容模型
        ///// </summary>
        ///// <remarks>
        ///// 不允许修改ContentType的TableName
        ///// </remarks>
        ///// <param name="contentType"></param>
        //public static bool DeleteContentType(ContentTypeDefinition contentType, out string errorMessage)
        //{
        //    SchemaBuilder schemaBuilder = new SchemaBuilder();
        //    schemaBuilder.DropTable(contentType.TableName);
        //    if (!schemaBuilder.Execute(out errorMessage))
        //        return false;

        //    foreach (var column in contentType.Columns)
        //    {
        //        contentTypeColumnDefinitionRepository.Delete(column);
        //    }
        //    contentTypeDefinitionRepository.Delete(contentType);

        //    return true;
        //}

        #endregion

        #region ContentTypeColumn

        /// <summary>
        /// 获取内容模型的所有字段
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public IEnumerable<ContentTypeColumnDefinition> GetColumnsByContentTypeId(int contentTypeId)
        {
            return contentTypeColumnDefinitionRepository.GetColumnsOfContentType(contentTypeId);
        }

        /// <summary>
        /// 获取内容模型字段
        /// </summary>
        /// <param name="columnId"></param>
        /// <returns></returns>
        public ContentTypeColumnDefinition GetColumn(int columnId)
        {
            return contentTypeColumnDefinitionRepository.Get(columnId);
        }

        ///// <summary>
        ///// 添加内容模型字段
        ///// </summary>
        ///// <param name="column"></param>
        ///// <returns></returns>
        //public bool CreateColumn(ContentTypeColumnDefinition column, out string errorMessage)
        //{
        //    ContentTypeDefinition contentType = column.ContentType;
        //    CreateColumnCommand command = new CreateColumnCommand(contentType.TableName, column.ColumnName)
        //        .WithType((DbType)Enum.Parse(typeof(DbType), column.DataType));

        //    if (column.DataType == UsableDatabaseTypes.String)
        //        command.WithLength(column.Length);
        //    else if (column.DataType == UsableDatabaseTypes.Decimal)
        //    {
        //        string[] precisionSplit = column.Precision.Split(',');
        //        if (precisionSplit.Length != 2)
        //        {
        //            errorMessage = "Decimal must be: Precision,Scale";
        //            return false;
        //        }
        //        else
        //        {
        //            byte precision = 10;
        //            byte.TryParse(precisionSplit[0], out precision);

        //            byte scale = 2;
        //            byte.TryParse(precisionSplit[1], out scale);

        //            command.WithPrecision(precision).WithScale(scale);
        //        }
        //    }

        //    if (column.IsNotNull)
        //        command.NotNull();

        //    if (!string.IsNullOrEmpty(column.DefaultValue))
        //        command.WithDefault(column.DefaultValue);

        //    if (column.IsUnique)
        //        command.Unique();

        //    SchemaBuilder schemaBuilder = new SchemaBuilder();
        //    schemaBuilder.AddColumn(command);

        //    if (column.IsIndex)
        //    {
        //        if (string.IsNullOrEmpty(column.KeyOrIndexColumns))
        //            column.KeyOrIndexColumns = column.ColumnName;

        //        if (string.IsNullOrEmpty(column.KeyOrIndexName))
        //            column.KeyOrIndexName = string.Format("IX_{0}_{1}", contentType.TableName, string.Join("_", column.KeyOrIndexColumns.Split(',')));

        //        schemaBuilder.AddIndex(contentType.TableName, column.KeyOrIndexName, column.KeyOrIndexColumns);
        //    }

        //    if (!schemaBuilder.Execute(out errorMessage))
        //        return false;

        //    contentTypeColumnDefinitionRepository.Insert(column);

        //    return true;
        //}

        ///// <summary>
        ///// 更新内容模型字段
        ///// </summary>
        ///// <remarks>
        ///// IsNotNull,IsUnique不允许修改
        ///// </remarks>
        ///// <param name="column"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public static bool UpdateColumn(ContentTypeColumnDefinition column, ContentTypeColumnDefinition oldColumn, out string errorMessage)
        //{
        //    if (!column.IsBuiltIn)
        //    {
        //        ContentTypeDefinition contentType = column.ContentType;
        //        SchemaBuilder schemaBuilder = new SchemaBuilder();

        //        if (column.ColumnName != oldColumn.ColumnName || column.DataType != oldColumn.DataType || column.Length != oldColumn.Length ||
        //            column.Precision != oldColumn.Precision || column.DefaultValue != oldColumn.DefaultValue)
        //        {
        //            AlterColumnCommand command = new AlterColumnCommand(contentType.TableName, column.ColumnName)
        //                .WithType((DbType)Enum.Parse(typeof(DbType), column.DataType));

        //            if (column.DataType == UsableDatabaseTypes.String)
        //                command.WithLength(column.Length);
        //            else if (column.DataType == UsableDatabaseTypes.Decimal)
        //            {
        //                string[] precisionSplit = column.Precision.Split(',');
        //                if (precisionSplit.Length != 2)
        //                {
        //                    errorMessage = "Decimal must be: Precision,Scale";
        //                    return false;
        //                }
        //                else
        //                {
        //                    byte precision = 10;
        //                    byte.TryParse(precisionSplit[0], out precision);

        //                    byte scale = 2;
        //                    byte.TryParse(precisionSplit[1], out scale);

        //                    command.WithPrecision(precision).WithScale(scale);
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(column.DefaultValue))
        //                command.WithDefault(column.DefaultValue);

        //            schemaBuilder.AlterColumn(command);
        //        }

        //        if (column.IsIndex != oldColumn.IsIndex || (column.IsIndex && (column.KeyOrIndexColumns != oldColumn.KeyOrIndexColumns)))
        //        {
        //            if (!column.IsIndex)
        //            {
        //                schemaBuilder.DropIndex(contentType.TableName, oldColumn.KeyOrIndexName);
        //            }
        //            else
        //            {
        //                if (oldColumn.IsIndex)
        //                {
        //                    schemaBuilder.DropIndex(contentType.TableName, oldColumn.KeyOrIndexName);
        //                }
        //                if (string.IsNullOrEmpty(column.KeyOrIndexColumns))
        //                    column.KeyOrIndexColumns = column.ColumnName;

        //                if (string.IsNullOrEmpty(column.KeyOrIndexName))
        //                    column.KeyOrIndexName = string.Format("IX_{0}_{1}", contentType.TableName, string.Join("_", column.KeyOrIndexColumns.Split(',')));

        //                schemaBuilder.AddIndex(contentType.TableName, column.KeyOrIndexName, column.KeyOrIndexColumns);
        //            }
        //        }

        //        if (!schemaBuilder.Execute(out errorMessage))
        //            return false;
        //    }
        //    contentTypeColumnDefinitionRepository.Update(column);
        //    errorMessage = string.Empty;
        //    return true;
        //}

        ///// <summary>
        ///// 删除内容模型字段
        ///// </summary>
        ///// <param name="column"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public static bool DeleteColumn(ContentTypeColumnDefinition column, out string errorMessage)
        //{
        //    ContentTypeDefinition contentType = column.ContentType;
        //    SchemaBuilder schemaBuilder = new SchemaBuilder();
        //    schemaBuilder.DropColumn(contentType.TableName, column.ColumnName);
        //    if (!schemaBuilder.Execute(out errorMessage))
        //        return false;

        //    contentTypeColumnDefinitionRepository.Delete(column);
        //    return true;
        //}

        #endregion

        #region FormControl

        /// <summary>
        /// 获取所有表单控件
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FormControlDefinition> GetFormControls()
        {
            return formControlDefinitionRepository.GetAll();
        }

        #endregion

    }
}
