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
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Itenso.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;


namespace OEA.Module.WPF
{
    /// <summary>
    /// 查询面板
    /// </summary>
    public abstract class QueryObjectView : DetailObjectView
    {
        internal QueryObjectView(EntityViewMeta evm)
            : base(evm) { }

        /// <summary>
        /// 查询后的结果使用的View
        /// </summary>
        public ObjectView ResultView
        {
            get
            {
                return this.TryFindRelation(SurrounderType.Result.GetDescription());
            }
        }

        public new Criteria Current
        {
            get
            {
                return base.Current as Criteria;
            }
            set
            {
                if (!(value is Criteria)) throw new InvalidOperationException("查询对象必须继承 QueryEntity 类.");
                base.Current = value as Criteria;
            }
        }

        /// <summary>
        /// 为这个查询面板构造并使用一个新的查询对象。
        /// </summary>
        /// <returns></returns>
        public void AttachNewQueryObject()
        {
            var criteria = RF.Create(this.EntityType).New() as Criteria;
            if (criteria == null) throw new InvalidProgramException("导航类需要继承自 Criteria 类。");

            this.Data = criteria;
        }

        /// <summary>
        /// 命令这个查询面板执行查询命令
        /// </summary>
        public void ExecuteQuery()
        {
            var queryObject = this.Current;
            queryObject.CheckRules();
            if (queryObject.BrokenRulesCollection.Count <= 0)
            {
                var resultView = this.ResultView;
                if (resultView == null) throw new InvalidOperationException("还没有设置 ResultView");

                //查询出结果集是不是需要到服务器取数据。
                CommandRepository.TryExecuteCommand(new QueryObjectCommand(), this);
            }
        }

        #region PropertyEditors

        private List<IPropertyEditor> _propertyEditors = new List<IPropertyEditor>();

        /// <summary>
        /// 这个View使用的所有的属性Editor
        /// Key：BOType的属性
        /// Value：这个属性使用的编辑器
        /// </summary>
        public IList<IPropertyEditor> PropertyEditors
        {
            get
            {
                return this._propertyEditors.AsReadOnly();
            }
        }

        /// <summary>
        /// 找到指定属性的Editor。
        /// 如果找不到，则把editor加入并返回。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public IPropertyEditor AddPropertyEditor(IPropertyEditor editor)
        {
            var result = this._propertyEditors.FirstOrDefault(e => e.PropertyViewInfo == editor.PropertyViewInfo);
            if (result == null)
            {
                result = editor;
                this._propertyEditors.Add(result);
            }
            return result;
        }

        /// <summary>
        /// 在View中寻找指定属性的Editor
        /// </summary>
        /// <param name="conView"></param>
        /// <param name="propertyName">找这个属性对应的Editor</param>
        /// <returns></returns>
        public IPropertyEditor FindPropertyEditor(string propertyName)
        {
            return this._propertyEditors.FirstOrDefault(e => e.PropertyViewInfo.Name == propertyName);
        }

        #endregion
    }
}