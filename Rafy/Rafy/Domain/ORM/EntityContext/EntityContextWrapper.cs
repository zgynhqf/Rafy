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
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 一个实现 IDisposable 模式，辅助声明实体上下文代码块的类型。
    /// </summary>
    internal class EntityContextWrapper : IDisposable
    {
        [ThreadStatic]
        private static int currentRef = 0;

        public EntityContextWrapper()
        {
            if (currentRef == 0)
            {
                EntityContext.Current = new EntityContext();
            }

            currentRef++;
        }

        public void Dispose()
        {
            currentRef--;
            if (currentRef == 0)
            {
                EntityContext.Current = null;
            }
        }
    }

    /// <summary>
    /// 一个实现 IDisposable 模式，辅助禁用实体上下文代码块的类型。
    /// </summary>
    internal class EntityContextDisableWrapper : IDisposable
    {
        public EntityContextDisableWrapper()
        {
            var current = EntityContext.Current;
            if (current != null) current.Disabled = true;
        }

        public void Dispose()
        {
            var current = EntityContext.Current;
            if (current != null) current.Disabled = false;
        }
    }
}
