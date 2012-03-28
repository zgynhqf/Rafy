/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110322
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100322
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OEA.Module.WPF
{
    /// <summary>
    /// 暂无行为，以后添加
    /// </summary>
    public class NavigateRelationView : RelationView
    {
        public NavigateRelationView(NavigateQueryObjectView view)
            : base(OEA.MetaModel.View.SurrounderType.Navigation, view) { }

        //public new NavigateQueryObjectView View
        //{
        //    get
        //    {
        //        return base.View as NavigateQueryObjectView;
        //    }
        //}

        //protected override void OnOwnerCurrentObjectChanged()
        //{
        //    base.OnOwnerCurrentObjectChanged();

        //    //this.View.SetByReferenceEntity(this.PropertyName, selectedItem);
        //}
    }
}