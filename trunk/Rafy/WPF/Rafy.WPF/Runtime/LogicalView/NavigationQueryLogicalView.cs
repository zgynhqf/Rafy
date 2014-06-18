/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 支持导航面板对象中孩子对象属性改变时，发生导航 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Diagnostics;
using System.ComponentModel;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// 导航查询面板视图控制器
    /// 
    /// 收集整个导航聚合对象中的所有导航属性（包括孩子对象的导航属性），
    /// 当某一个导航属性发生改变时，自动触发导航查询。
    /// </summary>
    public class NavigationQueryLogicalView : QueryLogicalView
    {
        internal NavigationQueryLogicalView(WPFEntityViewMeta evm) : base(evm) { }

        #region 公有接口

        /// <summary>
        /// 这个导航面板使用的所有的导航属性。
        /// </summary>
        public IEnumerable<WPFEntityPropertyViewMeta> NavigationProperties
        {
            get
            {
                foreach (WPFEntityPropertyViewMeta item in this.Meta.EntityProperties)
                {
                    if (item.NavigationMeta != null)
                    {
                        yield return item;
                    }
                }
            }
        }

        #endregion

        #region 导航主逻辑

        /// <summary>
        /// 是否关闭导航面板自动根据属性变更而查询的功能。
        /// </summary>
        public bool SuppressAutoQuery { get; set; }

        internal override void OnCurrentEntityPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnCurrentEntityPropertyChanged(e);

            if (!this.SuppressAutoQuery)
            {
                this.RaiseQueryIfNavigationChanged(e.PropertyName);
            }
        }

        /// <summary>
        /// 如果某个导航项改变时，需要发生导航查询。
        /// </summary>
        /// <param name="propertyName"></param>
        private void RaiseQueryIfNavigationChanged(string propertyName)
        {
            //如果任何一个导航属性发生改变，则执行查询。
            foreach (var naviProperty in this.NavigationProperties)
            {
                if (naviProperty.Name == propertyName)
                {
                    this.TryExecuteQuery();
                    return;
                }
            }
        }

        #endregion
    }
}