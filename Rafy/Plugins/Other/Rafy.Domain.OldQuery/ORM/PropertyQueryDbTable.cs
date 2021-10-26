///*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20131214
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20131214 21:47
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Hxy.Data;

//namespace Rafy.Domain.ORM
//{
//    class PropertyQueryDbTable
//    {
//        /// <summary>
//        /// 使用 IPropertyQuery 条件进行查询。
//        /// 分页默认实现为使用内存进行分页。
//        /// </summary>
//        /// <param name="dba">The dba.</param>
//        /// <param name="args">The arguments.</param>
//        public virtual void QueryList(IDbAccesser dba, IPropertySelectArgs args)
//        {
//            var dbQuery = args.PropertyQuery as PropertyQuery;
//            dbQuery.ErrorIfNotCompleted();

//            var parameters = new FormattedSqlParameters();
//            string sql = GenerateSelectSQL(dbQuery, parameters, false);

//            var reader = dba.QueryDataReader(sql, parameters);

//            this.FastFillByColumnIndex(reader, args.List, args);
//        }

//        internal int Count(IDbAccesser dba, IPropertyQuery query)
//        {
//            var dbQuery = query as PropertyQuery;
//            dbQuery.ErrorIfNotCompleted();

//            var parameters = new FormattedSqlParameters();
//            string sql = GenerateSelectSQL(dbQuery, parameters, true);

//            var value = dba.QueryValue(sql, parameters);
//            return ConvertCount(value);
//        }

//        /// <summary>
//        /// 子类可实现此方法实现对应 GenerateSelectSQL 的数据加载逻辑。
//        /// 注意！！！
//        /// 此方法中会释放 Reader。外层不能再用 Using。
//        /// </summary>
//        /// <param name="reader">The reader.</param>
//        /// <param name="list">The list.</param>
//        /// <param name="args">The arguments.</param>
//        private void FastFillByColumnIndex(IDataReader reader, IList<Entity> list, IPropertySelectArgs args)
//        {
//            //如果正在分页，而且不支持数据库层面的分页，则直接使用内存分页。
//            var dbQuery = args.PropertyQuery as PropertyQuery;
//            var memoryPagingInfo = dbQuery.PagingInfo;
//            if (this.GetPagingLocation(memoryPagingInfo) == PagingLocation.Database) { memoryPagingInfo = null; }

//            var fromIndex = list.Count;

//            if (dbQuery == null || !dbQuery.HasInnerJoin)
//            {
//                MemoryPaging(reader, r =>
//                {
//                    int i = 0;
//                    var entity = this.CreateByIndex(reader, ref i);
//                    list.Add(entity);
//                }, args.FetchingFirst, memoryPagingInfo);

//                OnDbLoaded(list, fromIndex);
//            }
//            else
//            {
//                int refItemsCount = 0;
//                List<RefTableProperty> refItems = null;

//                if (dbQuery.RefItems != null)
//                {
//                    refItems = dbQuery.RefItems.Where(i => i.JoinRefType == JoinRefType.QueryData).ToList();
//                    refItemsCount = refItems.Count;
//                }

//                var entitiesPerRow = new List<Entity>(refItemsCount + 1);//每一行最终读取的实体列表
//                var allRefEntities = new Dictionary<DbTable, List<Entity>>(refItemsCount + 1);

//                MemoryPaging(reader, r =>
//                {
//                    entitiesPerRow.Clear();

//                    int i = 0;
//                    var entity = this.CreateByIndex(reader, ref i);
//                    entitiesPerRow.Add(entity);

//                    //有 Join 时，把关系对象也加载进来。
//                    for (int j = 0; j < refItemsCount; j++)
//                    {
//                        var refItem = refItems[j];
//                        var refTable = refItem.RefTable;

//                        //如果创建的对象是关联中主表对应的实体类型，则表示找到数据。此时设置关联属性。
//                        for (int z = 0, c = entitiesPerRow.Count; z < c; z++)
//                        {
//                            var created = entitiesPerRow[z];
//                            if (refItem.PropertyOwner.IsInstanceOfType(created))
//                            {
//                                var refEntity = refTable.CreateByIndex(reader, ref i);
//                                created.SetRefEntity(refItem.RefProperty.RefEntityProperty, refEntity);
//                                entitiesPerRow.Add(refEntity);

//                                //添加到 allRefEntities 中
//                                List<Entity> refList = null;
//                                if (!allRefEntities.TryGetValue(refTable, out refList))
//                                {
//                                    refList = new List<Entity>();
//                                    allRefEntities.Add(refTable, refList);
//                                }
//                                refList.Add(refEntity);

//                                break;
//                            }
//                        }
//                    }

//                    list.Add(entity);
//                }, args.FetchingFirst, memoryPagingInfo);

//                //不同的实体，使用各自的仓库来通知加载完成。
//                OnDbLoaded(list, fromIndex);
//                foreach (var kv in allRefEntities)
//                {
//                    kv.Key.OnDbLoaded(kv.Value);
//                }
//            }
//        }

//        /// <summary>
//        /// 子类可实现此方法实现自己的 SQL 生成逻辑。
//        /// </summary>
//        /// <param name="dbQuery">The database query.</param>
//        /// <param name="parameters">The parameters.</param>
//        /// <param name="isCounting">if set to <c>true</c> [is counting].</param>
//        /// <returns></returns>
//        /// <exception cref="System.NotSupportedException">View 目前不支持使用 Join。</exception>
//        protected virtual string GenerateSelectSQL(PropertyQuery dbQuery, FormattedSqlParameters parameters, bool isCounting)
//        {
//            var tableMeta = this._meta.TableMeta;

//            if (dbQuery.IsEmpty)
//            {
//                var selectSql = GetRawSelectSql(isCounting, tableMeta);
//                return selectSql;
//            }

//            //生成：SELECT，INNER JOIN
//            var sql = new StringWriter();
//            sql.Write("SELECT ");

//            if (isCounting)
//            {
//                sql.Write("COUNT(0) ");
//            }
//            else
//            {
//                this.AppendSelectColumns(sql, false);

//                //引用表的所有字段也输出到 Select 中。
//                if (dbQuery.RefItems != null)
//                {
//                    foreach (var refItem in dbQuery.RefItems)
//                    {
//                        if (refItem.JoinRefType == JoinRefType.QueryData)
//                        {
//                            sql.Write(',');
//                            refItem.RefTable.AppendSelectColumns(sql, true);
//                        }
//                    }
//                }
//            }

//            sql.WriteLine();
//            sql.Write("FROM ");
//            sql.AppendQuoteName(this);
//            sql.WriteLine();

//            //有 Join，要把所有表的数据都带上，此时重新生成 Select。
//            if (dbQuery.HasInnerJoin)
//            {
//                if (tableMeta.IsMappingView) { throw new NotSupportedException("View 目前不支持使用 Join。"); }

//                dbQuery.AppendSqlJoin(sql, this);
//            }

//            //WHERE
//            dbQuery.AppendSqlWhere(sql, this, parameters);

//            //ORDER BY
//            if (!isCounting)
//            {
//                dbQuery.AppendSqlOrder(sql, this);
//            }

//            return sql.ToString();
//        }

//        #region 查询条件生成

//        internal void WritePropertyConstraintSql(PropertyConstraint constraint, TextWriter sql, FormattedSqlParameters parameters)
//        {
//            var expOperator = constraint.Operator;
//            var expValue = constraint.Value;

//            switch (expOperator)
//            {
//                case PropertyCompareOperator.Like:
//                case PropertyCompareOperator.Contains:
//                case PropertyCompareOperator.StartWith:
//                case PropertyCompareOperator.EndWith:
//                    //如果是空字符串的模糊对比操作，直接认为是真。
//                    var strValue = expValue as string;
//                    if (string.IsNullOrEmpty(strValue))
//                    {
//                        sql.Write("1 = 1");
//                        return;
//                    }
//                    break;
//                case PropertyCompareOperator.In:
//                case PropertyCompareOperator.NotIn:
//                    //对于 In、NotIn 操作，如果传入的是空列表时，需要特殊处理：
//                    //In(Empty) 表示 false，NotIn(Empty) 表示 true。
//                    bool hasValue = false;
//                    foreach (var item in expValue as IEnumerable)
//                    {
//                        hasValue = true;
//                        break;
//                    }
//                    if (!hasValue)
//                    {
//                        if (expOperator == PropertyCompareOperator.In)
//                        {
//                            sql.Write("0 = 1");
//                        }
//                        else
//                        {
//                            sql.Write("1 = 1");
//                        }

//                        return;
//                    }
//                    break;
//                default:
//                    break;
//            }

//            string column = this.Translate(constraint.Property);
//            sql.AppendQuoteName(this).Write('.');
//            sql.AppendQuote(this, column).Write(' ');

//            WritePropertyConstraintSqlValue(constraint, sql, parameters);
//        }

//        protected virtual void WritePropertyConstraintSqlValue(PropertyConstraint constraint, TextWriter sql, FormattedSqlParameters parameters)
//        {
//            var expOperator = constraint.Operator;
//            object expValue = constraint.Value;
//            //根据不同的操作符，来生成不同的 sql。
//            switch (expOperator)
//            {
//                case PropertyCompareOperator.Equal:
//                    if (expValue == null || expValue == DBNull.Value)
//                    {
//                        sql.Write("IS NULL");
//                    }
//                    else
//                    {
//                        sql.Write("= ");
//                        parameters.WriteParameter(sql, expValue);
//                    }
//                    break;
//                case PropertyCompareOperator.NotEqual:
//                    if (expValue == null || expValue == DBNull.Value)
//                    {
//                        sql.Write("IS NOT NULL");
//                    }
//                    else
//                    {
//                        sql.Write("!= ");
//                        parameters.WriteParameter(sql, expValue);
//                    }
//                    break;
//                case PropertyCompareOperator.Greater:
//                    sql.Write("> ");
//                    parameters.WriteParameter(sql, expValue);
//                    break;
//                case PropertyCompareOperator.GreaterEqual:
//                    sql.Write(">= ");
//                    parameters.WriteParameter(sql, expValue);
//                    break;
//                case PropertyCompareOperator.Less:
//                    sql.Write("< ");
//                    parameters.WriteParameter(sql, expValue);
//                    break;
//                case PropertyCompareOperator.LessEqual:
//                    sql.Write("<= ");
//                    parameters.WriteParameter(sql, expValue);
//                    break;
//                case PropertyCompareOperator.Like:
//                    sql.Write("LIKE ");
//                    parameters.WriteParameter(sql, expValue);
//                    break;
//                case PropertyCompareOperator.Contains:
//                    sql.Write("LIKE ");
//                    parameters.WriteParameter(sql, "%" + expValue + "%");
//                    break;
//                case PropertyCompareOperator.StartWith:
//                    sql.Write("LIKE ");
//                    parameters.WriteParameter(sql, expValue + "%");
//                    break;
//                case PropertyCompareOperator.EndWith:
//                    sql.Write("LIKE ");
//                    parameters.WriteParameter(sql, "%" + expValue);
//                    break;
//                case PropertyCompareOperator.In:
//                case PropertyCompareOperator.NotIn:
//                    var list = (expValue as IEnumerable).Cast<object>().ToArray();
//                    var op = expOperator == PropertyCompareOperator.In ? "IN" : "NOT IN";
//                    sql.Write(op);
//                    sql.Write(" (");
//                    for (int i = 0, c = list.Length; i < c; i++)
//                    {
//                        if (i != 0) sql.Write(',');
//                        parameters.WriteParameter(sql, list[i]);
//                    }
//                    sql.Write(')');
//                    break;
//                default:
//                    throw new NotSupportedException();
//            }
//        }

//        #endregion
//    }

//    class PropertyQueryOracleTable : PropertyQueryDbTable
//    {
//        protected override void WritePropertyConstraintSqlValue(PropertyConstraint constraint, TextWriter sql, FormattedSqlParameters parameters)
//        {
//            switch (constraint.Operator)
//            {
//                case PropertyCompareOperator.Equal:
//                case PropertyCompareOperator.NotEqual:
//                    //在 Oracle 中，空字符串的对比，需要转换为对 Null 值的对比。
//                    if (constraint.Property.PropertyType == typeof(string))
//                    {
//                        var strValue = constraint.Value as string;
//                        if (string.IsNullOrEmpty(strValue))
//                        {
//                            constraint.Value = null;
//                        }
//                    }
//                    break;
//                default:
//                    break;
//            }

//            base.WritePropertyConstraintSqlValue(constraint, sql, parameters);
//        }
//    }
//}
