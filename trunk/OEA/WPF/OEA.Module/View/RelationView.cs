/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110328
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;

namespace OEA
{
    /// <summary>
    /// 关系视图
    /// </summary>
    public class RelationView
    {
        public RelationView(string surrounderType, ObjectView view)
        {
            if (string.IsNullOrEmpty(surrounderType)) throw new ArgumentNullException("surrounderType");
            if (view == null) throw new ArgumentNullException("view");

            this.SurrounderType = surrounderType;
            this.View = view;
        }

        /// <summary>
        /// 当前所对应的环绕块类型
        /// </summary>
        public string SurrounderType { get; private set; }

        /// <summary>
        /// 拥有这个关系的视图
        /// </summary>
        public ObjectView Owner { get; internal set; }

        /// <summary>
        /// 关系视图
        /// </summary>
        public ObjectView View { get; private set; }

        internal protected virtual void OnOwnerDataChanging() { }

        internal protected virtual void OnOwnerDataChanged() { }

        internal protected virtual void OnOwnerCurrentObjectChanged() { }

        //internal protected virtual void OnOwnerEvent(string eventName, EventArgs e)
        //{

        //}
    }
}
