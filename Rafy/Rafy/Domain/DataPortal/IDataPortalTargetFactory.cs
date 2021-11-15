using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 数据门户目标对象的构造工厂
    /// </summary>
    public interface IDataPortalTargetFactory
    {
        /// <summary>
        /// 工厂的名称。需要在注册表中唯一。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 通过 targetInfo 来构造或查找对应的调用目标对象。
        /// </summary>
        /// <param name="targetInfo"></param>
        /// <returns></returns>
        IDataPortalTarget GetTarget(string targetInfo);
    }
}
