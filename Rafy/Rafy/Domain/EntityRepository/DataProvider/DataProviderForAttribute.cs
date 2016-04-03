/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140508
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140508 10:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 标记在某个数据提供器上，标识其为指定的仓库服务。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataProviderForAttribute : Attribute
    {
        /// <summary>
        /// 构造器。
        /// </summary>
        /// <param name="repositoryType">本数据层可以为指定的这个仓库及其子类服务。</param>
        public DataProviderForAttribute(Type repositoryType)
        {
            if (repositoryType == null) throw new ArgumentNullException("repositoryType");
            this.RepositoryType = repositoryType;
        }

        public Type RepositoryType { get; private set; }
    }
}
