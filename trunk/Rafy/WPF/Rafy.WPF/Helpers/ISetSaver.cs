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
    public interface ISetSaver : ITableSaver
    {
        /// <summary>
        /// 保存一个DataSet中的多个表到目的Excel文件中，每个Sheet名为对应Table的名字。
        /// </summary>
        /// <param name="dataSet">
        /// 包含多个表的数据集
        /// </param>
        /// <param name="fileName">
        /// 目的文件地址，如果已经存在，就会覆盖。
        /// </param>
        void SaveToFile(DataSet dataSet, string fileName);
    }

    public interface ISetReader : ITableReader
    {
        /// <summary>
        /// 把指定excel文件中的所有Sheet全部读取出为数据集。
        /// </summary>
        /// <param name="fileName">excel文件全路径。</param>
        /// <returns></returns>
        DataSet ReadSetFromFile(string fileName);
    }
}
