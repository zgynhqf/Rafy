/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130416 13:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using Rafy.Domain.Caching;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Domain.ORM;
using Rafy.Domain.DataPortal;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    //实体仓库的保存操作
    partial class EntityRepository
    {
        #region Save 公有接口

        /// <summary>
        /// 把这个组件中的所有改动保存到仓库中。
        /// 
        /// <remarks>
        /// * 当本地保存时，方法返回的就是传入的实体。
        /// * 当客户端保存时，方法返回的是服务端保存后并向客户端回传的实体。
        ///     此时，会对传入的实体或列表进行融合 Id 的操作。
        ///     也就是说，在服务端生成的所有 Id 都会设置到参数实体中。
        ///     而服务端设置其它的属性则会被忽略，如果想要使用其它的属性，则可以从返回值中获取。
        ///     
        /// 在客户端调用本方法保存实体的同时，服务端会把服务端保存完毕后的实体数据传输回客户端，这样才能保证客户端的实体能获取服务端生成的 Id 数据。
        /// 如果希望不进行如何大数据量的传输，则尽量不要在客户端直接调用 Save 来进行实体的保存。（例如可以通过 Service 来定义数据的传输。）
        /// </remarks>
        /// </summary>
        /// <param name="component">需要保存的组件，可以是一个实体，也可以是一个实体列表。</param>
        /// <returns>
        /// 返回在仓库中保存后的实体。
        /// </returns>
        public IDomainComponent Save(IDomainComponent component)
        {
            IDomainComponent result = component;

            if (component.IsDirty)
            {
                this.OnSaving(component);

                if (component is Entity)
                {
                    var entity = component as Entity;

                    var entityServer = this.SaveToPortal(component) as Entity;

                    //如果返回的对象与传入的对象不是同一个对象，表示已经在客户端通过了 WCF 来进行传输，
                    //这时需要把客户端对象的 Id 值与服务器对象的 Id 值统一。
                    if (entity != entityServer)
                    {
                        if (HasNewEntity(entity))
                        {
                            MergeIdRecur(entity, entityServer as Entity);
                        }

                        result = entityServer;
                    }
                }
                else if (component is EntityList)
                {
                    var list = component as EntityList;

                    var listServer = this.SaveToPortal(component) as EntityList;

                    if (list != listServer)
                    {
                        //保存实体列表时，需要把所有新加的实体的 Id 都设置好。
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (HasNewEntity(list[i]))
                            {
                                MergeIdRecur(list[i], listServer[i]);
                            }
                        }

                        result = listServer;
                    }
                }
                else
                {
                    throw new NotSupportedException("只支持对 Entity、EntityList 进行保存。");
                }

                //保存结束，传入的组件需要标记为已保存。
                component.MarkSaved();
            }

            //this.OnSaved(new SavedArgs
            //{
            //    Component = component,
            //    ReturnedComponent = result
            //});

            return result;
        }

        /// <summary>
        /// 通过门户来保存指定的实体类/列表。
        /// 
        /// 所有使用 Save 方法保存的实体，都会通过这个方法来选择是分布式保存、还是直接保存。
        /// 此方法是仓库接口门户层的最后一个方法，之后将会在服务端（如果是分布式）发布 Submit 数据提交操作。
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private IDomainComponent SaveToPortal(IDomainComponent component)
        {
            return DataPortalApi.Update(component, this.DataPortalLocation) as IDomainComponent;
        }

        /// <summary>
        /// 在使用 Save 方法保存实体数据时，进入数据门户前，Rafy 会调用此方法。
        /// <remarks>
        /// 子类可重写此方法实现一些仓库保存前的检查。
        /// 例如，一些仓库只允许在客户端进行调用时，可以在方法中判断，如果当前处在服务端，则抛出异常的逻辑。
        /// </remarks>
        /// </summary>
        /// <param name="component"></param>
        protected virtual void OnSaving(IDomainComponent component) { }

        /// <summary>
        /// 迭归检测一个组合实体中是否有新添加的实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static bool HasNewEntity(Entity entity)
        {
            if (entity.IsNew) return true;

            var enumerator = entity.GetLoadedChildren();
            while (enumerator.MoveNext())
            {
                var field = enumerator.Current.Value;
                var list = field as EntityList;
                if (list != null)
                {
                    foreach (var child in list)
                    {
                        if (HasNewEntity(child)) return true;
                    }
                }
                else
                {
                    var child = field as Entity;
                    if (HasNewEntity(child)) return true;
                }
            }

            return false;
        }

        #endregion
    }
}