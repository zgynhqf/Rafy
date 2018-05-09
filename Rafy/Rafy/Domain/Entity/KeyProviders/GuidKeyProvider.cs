﻿/*******************************************************
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
    internal class GuidKeyProvider : IKeyProvider
    {
        private object Empty = Guid.Empty;

        public Type KeyType
        {
            get { return typeof(Guid); }
        }

        public object DefaultValue
        {
            get { return Empty; }
        }

        public bool IsAvailable(object id)
        {
            return id != null && (Guid)id != Guid.Empty;
        }

        public object NewLocalValue()
        {
            return Guid.NewGuid();
        }

        public object GetEmptyId()
        {
            return Empty;
        }

        public object ToNullableValue(object value)
        {
            return IsAvailable(value) ? value : default(Guid?);
        }
    }
}