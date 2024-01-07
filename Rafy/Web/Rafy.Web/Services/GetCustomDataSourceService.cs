/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120510 18:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120510 18:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.Web
{
    /// <summary>
    /// 本服务用于为 ComboList 编辑器提供自定义的属性数据源
    /// </summary>
    [JsonService]
    [Contract, ContractImpl]
    internal class GetCustomDataSourceService : Service
    {
        /// <summary>
        /// 为这个实体提供数据源
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// 使用这个数据源属性
        /// </summary>
        public string DataSourceProperty { get; set; }

        /// <summary>
        /// 数据源实体列表
        /// </summary>
        [ServiceOutput]
        public IEntityList DataSource { get; set; }

        protected override void Execute()
        {
            if (this.Entity == null) throw new ArgumentNullException("this.Entity");
            if (this.DataSourceProperty == null) throw new ArgumentNullException("this.DataSourceProperty");

            var mp = this.Entity.PropertiesContainer.GetAvailableProperties().Find(this.DataSourceProperty);
            if (mp != null)
            {
                var value = this.Entity.GetProperty(mp) as IEntityList;
                if (value != null)
                {
                    this.DataSource = value;
                }
            }
        }
    }
}
