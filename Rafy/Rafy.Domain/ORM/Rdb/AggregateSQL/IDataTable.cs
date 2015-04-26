/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100402
 * 说明：此文件包括表格数据的类型。表、子表等。
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100402
 * 
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 一个存储表格数据的对象
    /// 
    /// 注意：
    /// 以此为参数的方法只能在服务端执行
    /// </summary>
    internal interface IDataTable : IEnumerable<DataRow>
    {
        /// <summary>
        /// 行数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取指定的行。
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        DataRow this[int rowIndex] { get; }
    }
}