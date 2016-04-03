/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120831 15:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120831 15:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using Rafy.Domain;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 实体唯一上下文
    /// 
    /// 保证了同一个 id 的实体在内存中只有一个运行时对象：
    /// 申请实体上下文块后，块中的所有代码经过实体仓库操作的实体，都会被保存在内存中。
    /// 当再次查询出同样的实体时，则会返回出之前已经查询出来的实体。
    /// 
    /// 注意，目前此功能只能在服务端使用。
    /// </summary>
    internal class EntityContext
    {
        /// <summary>
        /// 当前线程所对应的实体上下文
        /// </summary>
        [ThreadStatic]
        internal static EntityContext Current;

        /// <summary>
        /// 是否已经禁用了 EntityContext 功能。
        /// </summary>
        internal bool Disabled = false;

        /// <summary>
        /// 为最后一次被查询的对象进行缓存，提高查询效率。
        /// </summary>
        private TypedEntityContext _lastContext;

        /// <summary>
        /// 所有实体类型对应的上下文对象。
        /// </summary>
        private Dictionary<Type, TypedEntityContext> _contexts = new Dictionary<Type, TypedEntityContext>();

        /// <summary>
        /// 查找或者直接创建某一特定实体类型的上下文对象。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal TypedEntityContext GetOrCreateTypeContext(Type entityType)
        {
            if (Disabled) return new DisabledTypedEntityContext();

            if (this._lastContext != null && this._lastContext.EntityType == entityType)
            {
                return this._lastContext;
            }

            TypedEntityContext res = null;
            if (!this._contexts.TryGetValue(entityType, out res))
            {
                res = new TypedEntityContext { EntityType = entityType };
                this._contexts.Add(entityType, res);
            }

            this._lastContext = res;

            return res;
        }

        /// <summary>
        /// 查找对应实体的类型上下文
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal TypedEntityContext FindTypeContext(Type entityType)
        {
            if (Disabled) return null;

            if (this._lastContext != null && this._lastContext.EntityType == entityType)
            {
                return this._lastContext;
            }

            TypedEntityContext res = null;
            if (this._contexts.TryGetValue(entityType, out res))
            {
                this._lastContext = res;
            }
            return res;
        }

        /// <summary>
        /// 申明一个实体上下文操作代码块。
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static IDisposable Enter()
        {
            return new EntityContextWrapper();
        }

        /// <summary>
        /// 声明一个禁用了 EntityContext 功能的代码块。
        /// </summary>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static IDisposable Disable()
        {
            return new EntityContextDisableWrapper();
        }
    }
}