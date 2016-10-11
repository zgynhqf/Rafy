/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140515
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140515 23:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    internal class StringKeyProvider : IKeyProvider
    {
        public Type KeyType
        {
            get { return typeof(string); }
        }

        public object DefaultValue
        {
            get { return string.Empty; }
        }

        public bool IsAvailable(object id)
        {
            return id != null && !string.IsNullOrEmpty((string)id);
        }

        public object NewLocalValue()
        {
            return Guid.NewGuid().ToString();
        }

        public object GetEmptyIdForRefIdProperty()
        {
            return null;
        }

        public object ToNullableValue(object value)
        {
            return value;
        }
    }
}
