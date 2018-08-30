/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160406 11:03
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.MultiTenancy.Core
{
    /// <summary>
    /// 动态为实体注册 TenantId 属性的类型。
    /// </summary>
    public class TenantIdPropertyRegister : ExtensionPropertiesRegister
    {
        protected override void RegisterCore()
        {
            foreach (var entityType in MultiTenancyPlugin.Configuration.EnabledTypes)
            {
                var mp = new Property<long>(
                    entityType,
                    typeof(TenantIdPropertyRegister),
                    TenantAwareEntityExtension.TenantIdProperty,
                    new PropertyMetadata<long>()
                    );

                ManagedPropertyRepository.Instance.RegisterProperty(mp);
            }
        }
    }
}
