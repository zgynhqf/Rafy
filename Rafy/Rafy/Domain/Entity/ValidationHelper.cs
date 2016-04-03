/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130429 11:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 实体验证的帮助方法。
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// 通知上层应用，需要重新验证某个指定的属性。
        /// 一般在某个属性变更时调用此方法来通知另一属性需要进行验证。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="properties">The properties.</param>
        public static void Revalidate(Entity entity, params IProperty[] properties)
        {
            //目前直接用属性变更事件来通知上层的 Binding 重新
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    Revalidate(entity, property);
                }
            }
        }

        /// <summary>
        /// 通知上层应用，需要重新验证某个指定的属性。
        /// 一般在某个属性变更时调用此方法来通知另一属性需要进行验证。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="property">The property.</param>
        public static void Revalidate(Entity entity, IProperty property)
        {
            entity.NotifyRevalidate(property);
        }

        internal static ValidationRulesManager GetTypeRules(Type entityType)
        {
            //类型的规则，都放在 Repository 上。
            var host = RepositoryFactoryHost.Factory.FindByEntity(entityType) as ITypeValidationsHost;
            return GetTypeRules(host, entityType);
        }

        internal static ValidationRulesManager GetTypeRules(ITypeValidationsHost host, Type entityType)
        {
            if (host == null)
            {
                //如果该类型没有 Repository，则使用默认的 TypeValidationsHost 来存储。
                host = TypeValidationsHost.FindOrCreate(entityType);
            }

            if (!host.TypeRulesAdded)
            {
                host.Rules = new ValidationRulesManager();
                host.TypeRulesAdded = true;

                //在第一次创建时，添加类型的业务规则
                //注意，这个方法可能会调用到 Rules 属性获取刚才设置在 _typeRules 上的 ValidationRulesManager。
                InitializeValidations(entityType);

                //如果没有一个规则，则把这个属性删除。
                if (host.Rules.PropertyRules.Count == 0 && host.Rules.TypeRules.GetList(false).Count == 0)
                {
                    host.Rules = null;
                }
            }
            return host.Rules;
        }

        //internal ValidationRulesManager GetTypeRules(ITypeValidationsHost host, Type entityType)
        //{
        //    //类型的规则，都放在 Repository 上。
        //    if (host == null)
        //    {
        //        //如果该类型没有 Repository，则使用默认的 TypeValidationsHost 来存储。
        //        host = TypeValidationsHost.FindOrCreate(entityType);
        //    }
        //    if (!host.TypeRulesAdded)
        //    {
        //        host.Rules = new ValidationRulesManager();
        //        host.TypeRulesAdded = true;

        //        //在第一次创建时，添加类型的业务规则
        //        //注意，这个方法可能会调用到 Rules 属性获取刚才设置在 _typeRules 上的 ValidationRulesManager。
        //        AddValidations();

        //        //如果没有一个规则，则把这个属性删除。
        //        if (host.Rules.PropertyRules.Count == 0 && host.Rules.TypeRules.GetList(false).Count == 0)
        //        {
        //            host.Rules = null;
        //        }
        //    }
        //    return host.Rules;
        //}

        internal static void InitializeValidations(Type entityType)
        {
            var declarer = new ValidationDeclarer(entityType);

            //为所有不可空的引用属性加上 Required 验证规则。
            var container = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(entityType);
            var properties = container.GetAvailableProperties();
            foreach (var p in properties)
            {
                if (p is IRefIdProperty)
                {
                    if (!(p as IRefIdProperty).Nullable)
                    {
                        declarer.AddRule(p, new RequiredRule());
                    }
                }
            }

            //同时，使用 EntityConfig 类来配置所有的验证规则。
            var configurations = RafyEnvironment.FindConfigurations(entityType);
            foreach (var config in configurations)
            {
                //entityType 可能是子类型，所以需要传入对应的 declarer。
                config.AddValidations(declarer);
            }
        }
    }
}