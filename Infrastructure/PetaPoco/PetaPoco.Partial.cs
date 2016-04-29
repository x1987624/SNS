//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2013-10-9</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2013-10-9" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using PetaPoco;
using System.Data;
using System.Diagnostics;
using Tunynet;
using PetaPoco.Internal;
using System.Text.RegularExpressions;

namespace PetaPoco
{
    /// <summary>
    /// 对PetaPoco.Database进行封装，以便于使用
    /// </summary>
    public partial class Database
    {

        /// <summary>
        /// 获取PetaPoco.Database的实例
        /// </summary>
        public static Database CreateInstance(string connectionStringName = null)
        {
            Database instance = new Database(connectionStringName);
            instance.EnableAutoSelect = true;
            instance.EnableNamedParams = true;
            return instance;
        }

        //mazq,2011-10-12
        /// <summary>
        /// 批量执行sql
        /// </summary>
        /// <param name="sqls"></param>
        /// <returns></returns>
        public int Execute(IEnumerable<Sql> sqls)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    //mazq,2011-10-19,处理多线程情况
                    lock (syncObj)
                    {
                        var retv = 0;
                        foreach (var sql in sqls)
                        {
                            using (var cmd = CreateCommand(_sharedConnection, sql.SQL, sql.Arguments))
                            {
                                retv += cmd.ExecuteNonQuery();
                                OnExecutedCommand(cmd);
                            }
                        }
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <param name="sql">PetaPoco.Sql</param>
        public IEnumerable<object> FetchFirstColumn(PetaPoco.Sql sql)
        {
            return FetchFirstColumn(sql.SQL, sql.Arguments);
        }

        //mazq,2011-10-8
        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        public IEnumerable<object> FetchFirstColumn(string sql, params object[] args)
        {
            OpenSharedConnection();
            List<object> primaryKeys = new List<object>();
            try
            {
                ////mazq,2011-10-19,处理多线程情况
                lock (syncObj)
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        using (IDataReader r = cmd.ExecuteReader())
                        {
                            OnExecutedCommand(cmd);
                            while (r.Read())
                            {
                                primaryKeys.Add(r[0]);
                            }
                            r.Close();
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
            return primaryKeys;
        }


        /// <summary>
        /// 获取可分页的主键集合
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="maxRecords">最大返回记录数</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        /// <param name="sql">PetaPoco.Sql</param>
        /// <returns>可分页的实体Id集合</returns>
        public PagingEntityIdCollection FetchPagingPrimaryKeys<TEntity>(long maxRecords, int pageSize, int pageIndex, PetaPoco.Sql sql) where TEntity : IEntity
        {
            string sqlString = sql.SQL;
            object[] args = sql.Arguments;

            string sqlCount, sqlPage;
            BuildPagingPrimaryKeyQueries<TEntity>(maxRecords, (pageIndex - 1) * pageSize, pageSize, sqlString, ref args, out sqlCount, out sqlPage);

            // Setup the paged result
            long totalRecords = ExecuteScalar<long>(sqlCount, args);
            List<object> primaryKeyList = FetchFirstColumn(sqlPage, args).ToList();

            return new PagingEntityIdCollection(primaryKeyList, totalRecords);
        }

        /// <summary>
        /// 获取可分页的主键集合
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="maxRecords">最大返回记录数</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        /// <param name="primaryKey">主键</param>
        /// <param name="sql">PetaPoco.Sql <remarks>要求必须是完整的sql语句</remarks></param>
        /// <returns>可分页的实体Id集合</returns>
        public PagingEntityIdCollection FetchPagingPrimaryKeys(long maxRecords, int pageSize, int pageIndex, string primaryKey, PetaPoco.Sql sql)
        {
            string sqlString = sql.SQL;
            object[] args = sql.Arguments;

            string sqlCount, sqlPage;
            BuildPagingPrimaryKeyQueries(maxRecords, (pageIndex - 1) * pageSize, pageSize, primaryKey, sqlString, ref args, out sqlCount, out sqlPage);

            // Setup the paged result
            long totalRecords = ExecuteScalar<long>(sqlCount, args);
            List<object> primaryKeyList = FetchFirstColumn(sqlPage, args).ToList();

            return new PagingEntityIdCollection(primaryKeyList, totalRecords);
        }

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <param name="topNumber">前多少条数据</param>
        /// <param name="sql">PetaPoco.Sql</param>
        public IEnumerable<object> FetchTopPrimaryKeys<TEntity>(int topNumber, PetaPoco.Sql sql) where TEntity : IEntity
        {
            string sqlString = sql.SQL;
            object[] args = sql.Arguments;
            string sqlTop = BuildTopSql<TEntity>(topNumber, sqlString);

            return FetchFirstColumn(sqlTop, args);
        }

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="topNumber">前多少条数据</param>
        /// <param name="sql">PetaPoco.Sql<remarks>要求必须是完整的sql语句</remarks></param>
        public IEnumerable<T> FetchTop<T>(int topNumber, PetaPoco.Sql sql)
        {
            string sqlString = sql.SQL;
            object[] args = sql.Arguments;
            string sqlTop = BuildTopSql(topNumber, sqlString);
            return Fetch<T>(sqlTop, args);
        }

        //mazq,2011-10-9,增加依据主键集合获取实体集合的方法
        public IEnumerable<T> FetchByPrimaryKeys<T>(IEnumerable<object> primaryKeys)
        {
            if (primaryKeys == null || primaryKeys.Count() == 0)
                return new List<T>();

            string primaryKeyName = _dbType.EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey);

            StringBuilder sqlBuilder = new StringBuilder("WHERE ");
            int argIndex = 0;
            foreach (var primaryKey in primaryKeys)
            {
                sqlBuilder.AppendFormat("{0} = @{1} or ", primaryKeyName, argIndex);
                argIndex++;
            }
            sqlBuilder.Remove(sqlBuilder.Length - 4, 3);

            return Fetch<T>(sqlBuilder.ToString(), primaryKeys.ToArray());
        }

        //mazq,2011-10-8
        /// <summary>
        /// 创建分页的SQL语句
        /// </summary>
        protected void BuildPagingPrimaryKeyQueries<T>(long maxRecords, long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            var pd = PocoData.ForType(typeof(T));

            //libsh,2012-12-05
            string primaryKey = string.Empty;
            if (sql.Contains(pd.TableInfo.TableName))
                primaryKey = pd.TableInfo.TableName + "." + pd.TableInfo.PrimaryKey;
            else
                primaryKey = pd.TableInfo.PrimaryKey;

            if (EnableAutoSelect)
                sql = AutoSelectHelper.AddSelectClause<T>(_dbType, sql, primaryKey);

            BuildPagingPrimaryKeyQueries(maxRecords, skip, take, primaryKey/*libsh,2012-12-05*/, sql, ref args, out sqlCount, out sqlPage);
        }

        //mazq,2012-04-01
        /// <summary>
        /// 创建分页的SQL语句
        /// </summary>
        protected void BuildPagingPrimaryKeyQueries(long maxRecords, long skip, long take, string primaryKey, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            // Split the SQL
            string sqlSelectRemoved, sqlOrderBy;
            if (!SplitSqlForPagingOptimized(maxRecords, sql, primaryKey, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");

            sqlPage = _dbType.BuildPageQuery(skip, take, new PagingHelper.SQLParts { sql = sql, sqlCount = sqlCount, sqlSelectRemoved = sqlSelectRemoved, sqlOrderBy = sqlOrderBy }, ref args, primaryKey);
        }

        /// <summary>
        /// 切割sql数据
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <param name="sql"></param>
        /// <param name="primaryKey"></param>
        /// <param name="sqlCount"></param>
        /// <param name="sqlSelectRemoved"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        protected bool SplitSqlForPagingOptimized(long maxRecords, string sql, string primaryKey, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = PagingHelper.rxColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            Group g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            if (PagingHelper.rxDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else if (maxRecords > 0)
            {
                if (_providerName.StartsWith("MySql"))
                    sqlCount = "select count(*) from (" + sql + " limit " + maxRecords + " ) as TempCountTable";
                else
                    sqlCount = "select count(*) from (" + sql.Substring(0, g.Index) + " top " + maxRecords + " " + primaryKey + " " + sql.Substring(g.Index + g.Length) + " ) as TempCountTable";
            }
            else
                sqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);


            // Look for the last "ORDER BY <whatever>" clause not part of a ROW_NUMBER expression
            m = PagingHelper.rxOrderBy.Match(sqlCount);
            if (!m.Success)
            {
                sqlOrderBy = null;
            }
            else
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }
            sqlOrderBy = null;

            return true;
        }


        //mazq,2011-10-26
        /// <summary>
        /// 构建获取前topNumber记录的SQL
        /// </summary>
        protected string BuildTopSql<T>(int topNumber, string sql)
        {
            var pd = PocoData.ForType(typeof(T));
            //libsh,2012-9-30 修改不支持多部分组成的语句
            string tempPK = pd.TableInfo.TableName + "." + pd.TableInfo.PrimaryKey;
            if (EnableAutoSelect)
                sql = AutoSelectHelper.AddSelectClause<T>(_dbType, sql, tempPK);

            return BuildTopSql(topNumber, sql);
        }

        //mazq,2012-04-01
        /// <summary>
        /// 构建获取前topNumber记录的SQL
        /// </summary>
        protected string BuildTopSql(int topNumber, string sql)
        {
            string sqlTop = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = PagingHelper.rxColumns.Match(sql);
            if (!m.Success)
                return null;

            Group g = m.Groups[1];

            //mazq,仅支持SQL Server  对于其他数据库需要重写
            //libsh,2011-11-05,添加Mysql支持
            if (_providerName.StartsWith("MySql"))
                sqlTop = sql + " limit " + topNumber;
            else
                sqlTop = sql.Substring(0, g.Index) + " top " + topNumber + " " + g.Value + " " + sql.Substring(g.Index + g.Length);

            return sqlTop;
        }
    }
}
