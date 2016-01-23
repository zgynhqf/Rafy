/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151022
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151022 11:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ComponentModel;

namespace Rafy.Domain.EntityPhantom
{
    /// <summary>
    /// <para>本插件用于为领域实体框架添加‘幽灵’功能（假删除），即在删除数据时，不是真实地在数据库中删除这行记录，而是把该实体的‘幽灵’标识标记为真。   </para>
    /// <para>                                                                                                                                </para>
    /// <para>添加此插件，将会在下面几个方面影响领域实体框架：                                                                                   </para>
    /// <para>* 所有继承自 Entity 的实体都会统一的添加一个 IsPhantom 的属性。这个属性表示这个实体是否为‘幽灵’，即已经删除的数据。                   </para>
    /// <para>* 开发者可以使用 Meta.EnablePhantoms() 来为某个指定的实体类型开启‘幽灵’功能。                                                      </para>
    /// <para>* 开启该功能的实体的 IsPhantom 属性会自动映射到数据库中。                                                                          </para>
    /// <para>* 在保存实体时，如果要删除一个聚合实体，则这个聚合中的所有实体都将会被标记为‘幽灵’状态。                                              </para>
    /// <para>* 在查询实体时，所有的查询，都将会自动过滤掉所有‘幽灵’状态的数据。（手写 SQL 查询的场景不在考虑范围内。）                              </para>
    /// <para>* 使用批量导入数据插件进行数据的批量导入时，批量删除的实体同样都会被标记为‘幽灵’状态。                                                </para>
    /// </summary>
    public class EntityPhantomPlugin : DomainPlugin
    {
        /// <summary>
        /// Initializes the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public override void Initialize(IApp app)
        {
            //设置约定的属性。
            EntityConvention.Property_IsPhantom = EntityPhantomExtension.IsPhantomProperty;

            //数据的删除、查询的拦截器。
            PhantomDataInterceptor.Intercept();
        }
    }
}