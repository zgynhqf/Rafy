/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110621
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Media;
using System.Windows.Data;
using OEA.Library;
using System.Windows.Controls;

namespace OEA.Module.WPF.Controls
{
    public partial class MultiTypesTreeGrid
    {
        private new GridTreeView Tree
        {
            get { return base.Tree as GridTreeView; }
        }

        /// <summary>
        /// 运行时的根类型
        /// </summary>
        private Type RootRuntimeType
        {
            get { return this.RootEntityViewMeta.EntityType; }
        }

        #region 背景色

        /// <summary>
        /// 设置背景色，可以根据根据自定义值转换绑定，也可直接指定
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        private void SetBackground(GridTreeViewRow result, Entity item)
        {
            var evm = result.EntityViewMeta;

            var backgroundConverter = GetBackGroundConverter(evm);
            if (backgroundConverter != null)
            {
                result.SetBinding(TreeViewItem.BackgroundProperty, new Binding
                {
                    BindsDirectlyToSource = true,
                    Mode = BindingMode.OneWay,
                    Converter = backgroundConverter,
                    ConverterParameter = this
                });
            }
            else
            {
                //设置默认背景色
                var color = GetColor(evm);
                result.Background = new SolidColorBrush(color);
            }
        }

        #region TypeBackGroundColorKey

        /// <summary>
        /// 用于 MultiTypesTreeGrid 和实体类间建立背景颜色的约定。
        /// </summary>
        public static readonly string TypeBackGroundColorKey = "TypeBackGroundColorKey";

        private static Color GetColor(EntityViewMeta evm)
        {
            return evm.TryGetCustomParams<Color>(TypeBackGroundColorKey);
        }

        public static void SetColor<TEntity>(byte r, byte g, byte b)
        {
            UIModel.Views.Create(typeof(TEntity))
                .SetCustomParams(MultiTypesTreeGrid.TypeBackGroundColorKey, Color.FromRgb(r, g, b));
        }

        #endregion

        #region TypeBackGroundConverterKey

        public static readonly string TypeBackGroundConverterKey = "TypeBackGroundConverterKey";

        public static IValueConverter GetBackGroundConverter(EntityViewMeta evm)
        {
            return evm.TryGetCustomParams<IValueConverter>(TypeBackGroundConverterKey);
        }

        public static void SetBackGroundConverter<TEntity>(IValueConverter valueConverter)
        {
            UIModel.Views.Create(typeof(TEntity))
                .SetCustomParams(MultiTypesTreeGrid.TypeBackGroundConverterKey, valueConverter);
        }

        #endregion

        #endregion
    }
}
