/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160406 10:40
 * 
*******************************************************/

using Rafy.MetaModel;

namespace Rafy.MultiTenancy
{
    public static class EntityMetaExtension
    {
        #region EntityMeta.IsMultiTenancyEnabled

        /// <summary>
        /// '是否启用租户隔离'属性在扩展属性中的键。
        /// </summary>
        private const string IsMultiTenancyEnabledKey = "EntityMetaExtension.IsMultiTenancyEnabled";

        /// <summary>
        /// 设置对象的'是否启用租户隔离'属性。
        /// </summary>
        /// <param name="ext">扩展属性的对象。</param>
        /// <param name="value">设置的属性值。</param>
        /// <returns>扩展属性的对象。</returns>
        public static EntityMeta SetIsMultiTenancyEnabled(this EntityMeta ext, bool value)
        {
            ext.SetExtendedProperty(IsMultiTenancyEnabledKey, value);
            return ext;
        }

        /// <summary>
        /// 获取对象的'是否启用租户隔离'属性。
        /// </summary>
        /// <param name="ext">扩展属性的对象。</param>
        /// <returns>被扩展的属性值，或者该属性的默认值。</returns>
        public static bool GetIsMultiTenancyEnabled(this EntityMeta ext)
        {
            return ext.GetPropertyOrDefault(IsMultiTenancyEnabledKey, default(bool));
        }

        #endregion
    }
}