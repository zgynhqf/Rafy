using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    public static class ObjectViewExtension
    {
        /// <summary>
        /// 获取一个view的“活动”对象集。
        /// 
        /// 如果它是一个列表，则返回选中的所有对象。
        /// 否则，返回当前使用的对象CurrentObject。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static IList GetActiveObjects(this IObjectView view)
        {
            if (view is IListObjectView)
            {
                return (view as IListObjectView).SelectedObjects;
            }

            ArrayList list = new ArrayList();
            list.Add(view.Current);
            return list;
        }
    }
}