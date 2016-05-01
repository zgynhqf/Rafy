/*******************************************************
* 
* 作者：胡庆访
* 创建日期：20160502
* 说明：此文件只包含一个类，具体内容见类型注释。
* 版本号：1.0.0
* 
* 历史记录：
* 创建文件 胡庆访 20160502 01:53
* 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain;

namespace Rafy.FileStorage.Controllers
{
    /// <summary>
    /// 大文件相关的领域操作，都封装这个类中。
    /// </summary>
    public class FileStorageController : DomainController
    {
        public void Save(FileInfo file, Stream fileStream)
        {
            throw new NotSupportedException();
        }

        public Stream Read(FileInfo file)
        {
            throw new NotSupportedException();
        }
    }
}