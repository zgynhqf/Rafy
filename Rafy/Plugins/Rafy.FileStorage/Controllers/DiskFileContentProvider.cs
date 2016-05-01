/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 01:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.FileStorage.Controllers
{
    /// <summary>
    /// 使用硬盘文件系统来存储文件的提供程序。
    /// </summary>
    public class DiskFileContentProvider : IFileInfoContentProvider
    {
        /// <summary>
        /// 文件需要存储的文件夹相对路径。
        /// 默认值为 RafyEnvironment.MapAbsolutePath("Files/")。
        /// </summary>
        public string BaseDirectory { get; set; } = RafyEnvironment.MapAbsolutePath("Files\\");

        /// <summary>
        /// 文件存储文件夹的时间分组格式。
        /// 如果此属性不为空，则表示本规则需要根据时间的某个格式来进行分组编号。
        /// 例如：yyyyMM 表示每一个月中的所有文件使用一个文件夹来存储；yyyy 表示一年一个文件夹；yyyyMMdd 表示一天一个文件夹。
        /// 默认值为："yyyyMMdd"。
        /// </summary>
        public string FileTimeGroupFormat { get; set; } = "yyyyMMdd";

        /// <summary>
        /// 为指定的文件写入指定的内容。
        /// </summary>
        /// <param name="fileInfo">要保存的文件对象</param>
        /// <param name="content">文件对象对应的文件的内容</param>
        public virtual void SaveContent(FileInfo fileInfo, byte[] content)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            if (content == null) throw new ArgumentNullException("content");

            fileInfo.Length = content.Length;

            //如果还没有指定存储地址，则生成该地址。
            if (string.IsNullOrEmpty(fileInfo.Storage))
            {
                var dir = Path.Combine(this.BaseDirectory, DateTime.Today.ToString(this.FileTimeGroupFormat));
                Directory.CreateDirectory(dir);
                fileInfo.Storage = Path.Combine(dir, this.GenerateFileName(fileInfo));
            }

            File.WriteAllBytes(fileInfo.Storage, content);
        }

        /// <summary>
        /// 读取指定文件的内容。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public virtual byte[] ReadContent(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            if (string.IsNullOrEmpty(fileInfo.Storage)) throw new ArgumentNullException("fileInfo.Storage");

            if (File.Exists(fileInfo.Storage))
            {
                return File.ReadAllBytes(fileInfo.Storage);
            }
            return new byte[0];
        }

        /// <summary>
        /// 删除指定文件的存储内容。
        /// </summary>
        /// <param name="fileInfo"></param>
        public virtual void DeleteContent(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            if (string.IsNullOrEmpty(fileInfo.Storage)) throw new ArgumentNullException("fileInfo.Storage");

            File.Delete(fileInfo.Storage);
        }

        /// <summary>
        /// 生成用于存储指定文件的文件名。
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        protected virtual string GenerateFileName(FileInfo fileInfo)
        {
            return Guid.NewGuid().ToString();
        }
    }
}