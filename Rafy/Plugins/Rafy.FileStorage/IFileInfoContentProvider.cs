/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140806
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140806 10:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.FileStorage
{
    /// <summary>
    /// FileInfo 的内容提供程序
    /// </summary>
    public interface IFileInfoContentProvider
    {
        /// <summary>
        /// 为指定的文件写入指定的内容。
        /// </summary>
        /// <param name="fileInfo">要保存的文件对象</param>
        /// <param name="content">文件对象对应的文件的内容</param>
        void SaveContent(FileInfo fileInfo, byte[] content);

        /// <summary>
        /// 读取指定文件的内容。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        byte[] ReadContent(FileInfo fileInfo);

        /// <summary>
        /// 删除指定文件的存储内容。
        /// </summary>
        /// <param name="fileInfo"></param>
        void DeleteContent(FileInfo fileInfo);
    }
}