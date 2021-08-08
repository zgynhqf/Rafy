/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210808
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210808 19:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 实体配置的仓库。
    /// </summary>
    public interface IEntityConfigRepository
    {
        /// <summary>
        /// 获取某个实体视图的所有配置类实例
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IEnumerable<EntityConfig> FindConfigurations(Type entityType);

        /// <summary>
        /// 清空缓存。
        /// 在新的插件加载到系统中时，会调用此方法。
        /// </summary>
        void ClearCache();
    }
}
