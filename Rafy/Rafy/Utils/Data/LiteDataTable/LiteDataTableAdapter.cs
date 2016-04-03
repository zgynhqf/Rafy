/*******************************************************
 * 
 * 作者：Steven
 * 创建日期：2006-04-15
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130524 09:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Rafy.Data
{
    /// <summary>
    /// 数据容器帮助类
    /// </summary>
    /// Title: DataContainerUtil
    /// Author: Steven
    /// Version: 1.0
    /// History:
    ///     2006-04-15 Steven [创建] 
    ///     2007-06-07 jianghaoqun [添加功能 FillDataContainer()]
    ///     2007-06-13 Steven 添加FillDatawindowChild重载
    public static class LiteDataTableAdapter
    {
        /// <summary>
        /// 根据SQL Command返回的IDataReader填充数据容器
        /// <para>
        /// 对于DBNull的值，填充null
        /// </para>
        /// </summary>
        /// <param name="dataTable">数据容器</param>
        /// <param name="reader">SQL语句返回的</param>
        public static void Fill(LiteDataTable dataTable, IDataReader reader)
        {
            if (dataTable.Columns.Count == 0)
            {
                AddColumns(dataTable, reader);
            }

            int columnsCount = dataTable.Columns.Count;

            while (reader.Read())
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < columnsCount; i++)
                {
                    var value = reader[i];
                    if (value != DBNull.Value)
                    {
                        row[i] = value;
                    }
                }
                dataTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// 使用 IDataReader 中的结构为指定的表格添加列。
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="reader"></param>
        public static void AddColumns(LiteDataTable dataTable, IDataReader reader)
        {
            //columnsCount = reader.FieldCount;

            var schema = reader.GetSchemaTable();
            foreach (DataRow dr in schema.Rows)
            {
                dataTable.Columns.Add(new LiteDataColumn(dr["ColumnName"].ToString(), dr["DataType"].ToString()));
            }
        }

        /// <summary>
        /// 把DataTable填充到数据容器中，方便填充数据到DataWindow
        /// </summary>
        /// <param name="dataTable">数据容器</param>
        /// <param name="dt">DataTable</param>
        /// <Author>jianghaoqun 2007-06-07</Author>
        public static void Fill(LiteDataTable dataTable, DataTable dt)
        {
            int columnsCount = dataTable.Columns.Count;
            if (columnsCount == 0)
            {
                columnsCount = dt.Columns.Count;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dataTable.Columns.Add(new LiteDataColumn(dt.Columns[i].ColumnName, dt.Columns[i].DataType));
                }
            }

            foreach (DataRow dr in dt.Rows)
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < columnsCount; i++)
                {
                    var value = dr[i];
                    if (value != DBNull.Value)
                    {
                        row[i] = value;
                    }
                }
                dataTable.Rows.Add(row);
            }
        }
    }
}