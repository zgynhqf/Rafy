/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110412
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110412
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Rafy.WPF
{
    public interface ITableSaver
    {
        /// <summary>
        /// 保存table到目的Excel文件中，Sheet名为Table的名字。
        /// </summary>
        /// <param name="table">
        /// 需要存储到excel中的表格数据
        /// </param>
        /// <param name="fileName">
        /// 目的文件地址，如果已经存在，就会覆盖。
        /// </param>
        void SaveToFile(DataTable table, string fileName);
    }

    public interface ITableReader
    {
        /// <summary>
        /// 把指定excel文件中指定的Sheet读取出为数据。
        /// </summary>
        /// <param name="fileName">excel文件全路径。</param>
        /// <returns></returns>
        DataTable ReadFromFile(string fileName);
    }
}
