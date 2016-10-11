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
    internal class LongKeyProvider : IKeyProvider
    {
        /// <summary>
        /// 先装箱完成
        /// </summary>
        private object Zero = 0L;

        public Type KeyType
        {
            get { return typeof(long); }
        }

        public object DefaultValue
        {
            get { return Zero; }
        }

        public bool IsAvailable(object id)
        {
            return id != null && Convert.ToInt64(id) > 0;
        }

        public object NewLocalValue()
        {
            return RafyEnvironment.NewLocalId();
        }

        public object GetEmptyIdForRefIdProperty()
        {
            return Zero;
        }

        public object ToNullableValue(object value)
        {
            return IsAvailable(value) ? value : default(long?);
        }
    }
}
