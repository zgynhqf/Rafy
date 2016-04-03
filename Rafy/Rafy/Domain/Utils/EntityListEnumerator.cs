/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130423 11:04
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体列表的强类型遍历器。
    /// </summary>
    /// <typeparam name="TEntity">要保证 TEntity 是 Entity 的子类，否则会转换出错。</typeparam>
    public class EntityListEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private IEnumerator<Entity> _core;

        public EntityListEnumerator(EntityList list)
        {
            _core = list.GetEnumerator();
        }

        public TEntity Current
        {
            get { return (TEntity)(object)_core.Current; }
        }

        public void Dispose()
        {
            _core.Dispose();
        }

        object IEnumerator.Current
        {
            get { return _core.Current; }
        }

        public bool MoveNext()
        {
            return _core.MoveNext();
        }

        public void Reset()
        {
            _core.Reset();
        }
    }
}