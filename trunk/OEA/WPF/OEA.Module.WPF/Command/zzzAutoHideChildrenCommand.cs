//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OEA.MetaModel.Attributes;
//using OEA.MetaModel;
//using OEA.MetaModel.View;
//using OEA.Module.WPF;

//namespace OEA.WPF.Command
//{
//    [Command(CommandNames.AutoHideChildren, ApplyToEntities.None, AutoExecuting = true)]
//    public class AutoHideChildrenCommand : ListViewCommand
//    {
//        public override void Execute(ListObjectView view)
//        {
//            if (view != null) { HideChildrenViews(view); }

//            view.DataChanged += (o, e) => { HideChildrenViews(view); };
//        }

//        /// <summary>
//        /// 隐藏所有子页签
//        /// </summary>
//        /// <param name="view"></param>
//        private void HideChildrenViews(ListObjectView view)
//        {
//            foreach (var item in view.ChildrenViews) { item.IsVisible = false; }
//        }
//    }
//}