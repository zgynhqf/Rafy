/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：一些实体类通用的条件类。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 对实体的一组属性中的每一个都进行精确匹配的查询条件
    /// </summary>
    [Serializable]
    public class PropertiesMatchCriteria : Criteria, IEnumerable
    {
        private Dictionary<string, object> _v;

        private PropertiesMatchAction _a;

        #region 构造函数

        /// <summary>
        /// 构造对实体的一组属性中的每一个都进行精确匹配的查询条件。
        /// </summary>
        public PropertiesMatchCriteria()
        {
            _v = new Dictionary<string, object>();
            _a = PropertiesMatchAction.None;
        }

        /// <summary>
        /// 构造对实体的一组属性中的每一个都进行精确匹配的查询条件，并指定属性匹配时的行为。
        /// </summary>
        /// <param name="actions"></param>
        public PropertiesMatchCriteria(PropertiesMatchAction actions)
        {
            _v = new Dictionary<string, object>();
            _a = actions;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected PropertiesMatchCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        /// <summary>
        /// 键值对
        /// </summary>
        public Dictionary<string, object> Values
        {
            get { return _v; }
        }

        /// <summary>
        /// 属性对比时的行为。
        /// </summary>
        public PropertiesMatchAction Actions
        {
            get { return _a; }
            set { _a = value; }
        }

        #region 集合初始化器

        /// <summary>
        /// 添加一个托管属性条件
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void Add(IManagedProperty property, object value)
        {
            _v[property.Name] = value;
        }

        /// <summary>
        /// 通过属性名称来添加条件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void Add(string property, object value)
        {
            _v[property] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _v.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// 属性对比时的行为。
    /// </summary>
    public enum PropertiesMatchAction
    {
        /// <summary>
        /// 空行为。
        /// </summary>
        None = 0,
        /// <summary>
        /// 如果值为空，则忽略空值的对比。
        /// <remarks>使用 AddConstrainIf 方法进行对比。</remarks>
        /// </summary>
        IgnoreNull = 1,
        /// <summary>
        /// 字符串的对比，使用包含操作进行模糊比较。
        /// </summary>
        StringAsContains = 2,
    }
}
