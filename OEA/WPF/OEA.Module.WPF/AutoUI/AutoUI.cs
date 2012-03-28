/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100408
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100408
 * 生成控件的门面方法 胡庆访 20100408
 * 
*******************************************************/

using System;
using Microsoft.Practices.Unity;
using OEA.MetaModel.View;


namespace OEA.Module.WPF
{
    /// <summary>
    /// 自动生成UI界面类
    /// </summary>
    public class AutoUI
    {
        static AutoUI()
        {
            var container = OEAEnvironment.UnityContainer;

            BlockUIFactory = container.Resolve<BlockUIFactory>();
            container.RegisterInstance(BlockUIFactory);

            ViewFactory = container.Resolve<ObjectViewFactory>();
            container.RegisterInstance(ViewFactory);

            AggtUIFactory = container.Resolve<AggtControlGenerator>();
            container.RegisterInstance(AggtUIFactory);
        }

        /// <summary>
        /// 单块UI生成器
        /// </summary>
        public static readonly BlockUIFactory BlockUIFactory;

        /// <summary>
        /// ObjectViewFactory
        /// </summary>
        public static readonly ObjectViewFactory ViewFactory;

        /// <summary>
        /// 通过元数据生成界面的界面生成器
        /// </summary>
        public static readonly AggtControlGenerator AggtUIFactory;

        /// <summary>
        /// 为某实体类生成聚合控件
        /// </summary>
        /// <param name="entityType">
        /// 实体类型
        /// </param>
        /// <returns></returns>
        public static ControlResult GenerateAggtControl(Type entityType)
        {
            return GenerateAggtControl(entityType, null);
        }

        /// <summary>
        /// 为某实体类生成聚合控件
        /// </summary>
        /// <param name="entityType">
        /// 实体类型
        /// </param>
        /// 
        /// <param name="blocksModifier">
        /// 元数据修改器，可以传入null。
        /// </param>
        /// <returns></returns>
        public static ControlResult GenerateAggtControl(Type entityType, Action<AggtBlocks, object> blocksModifier)
        {
            var blocks = UIModel.AggtBlocks.GetDefaultBlocks(entityType);

            if (blocksModifier != null) blocksModifier(blocks, _forExtension);

            var result = AggtUIFactory.GenerateControl(blocks);

            return result;
        }

        private static readonly object _forExtension = new object();
    }
}