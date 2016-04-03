/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 一些内置的 WPF 命令
    /// </summary>
    public static class WPFCommandNames
    {
        /// <summary>
        /// 这个命令需要引入 Rafy.Customization 插件才可使用。
        /// 否则值为 null。
        /// </summary>
        public static Type CustomizeUI;

        public static Type ExportToExcel;
        public static Type FireQuery;
        public static Type RefreshDataSourceInRDLC;
        public static Type ShowReportData;

        public static Type PopupAdd;
        public static Type Add;
        public static Type SaveBill;
        public static Type SaveList;
        public static Type Cancel;
        public static Type Filter;
        public static Type Refresh;
        public static Type Delete;
        public static Type Edit;

        public static Type MoveUp;
        public static Type MoveDown;
        public static Type LevelUp;
        public static Type LevelDown;
        public static Type InsertBefore;
        public static Type InsertChild;
        //public static Type InsertFollow;
        public static Type ExpandAll;
        public static Type ExpandOne;
        public static Type ExpandTwo;
        public static Type ExpandThree;
        public static Type ExpandFour;

        public static Type SelectAll;
        public static Type SelectReverse;

        //public static Type LookupSelectAdd;

        /*********************** 代码块解释 *********************************
         * 
         * 可以修改这几个集合中的数据以达到修改整个应用程序的效果。
         * 
        **********************************************************************/
        /// <summary>
        /// 系统中为非查询类型 添加的默认系统命令
        /// 
        /// 这里的命令会在 UIModel.Views.CreateBaseView 方法中进行设置。
        /// </summary>
        public static readonly List<Type> SysCommands = new List<Type>();
        /// <summary>
        /// 系统中为查询类型 添加的默认系统命令
        /// 
        /// 这里的命令会在 UIModel.Views.CreateBaseView 方法中进行设置。
        /// </summary>
        public static readonly List<Type> SysQueryCommands = new List<Type>();
        public static readonly List<Type> CommonCommands = new List<Type>();
        public static readonly List<Type> TreeCommands = new List<Type>();
        public static readonly List<Type> TreeExpandCommands = new List<Type>();
        public static readonly List<Type> RootCommands = new List<Type>();

        public static void InitCommonCommands()
        {
            WPFCommandNames.SysCommands.AddRange(new Type[]{
                ExportToExcel
            });
            //WPFCommandNames.SysQueryCommands.AddRange(new Type[]{
            //    FireQuery
            //});

            WPFCommandNames.CommonCommands.AddRange(new Type[]{
                PopupAdd, Edit, Delete
            });

            WPFCommandNames.TreeCommands.AddRange(new Type[]{
                ExpandAll, ExpandOne, ExpandTwo, ExpandThree, ExpandFour,
                PopupAdd, Edit, Delete, 
                MoveUp, MoveDown, LevelUp, LevelDown, InsertBefore, InsertChild,
            });

            WPFCommandNames.TreeExpandCommands.AddRange(new Type[]{
                ExpandAll, ExpandOne, ExpandTwo, ExpandThree, ExpandFour
            });

            WPFCommandNames.RootCommands.AddRange(new Type[]{
                SaveList, Cancel, Refresh
            });
        }

        internal static void Clear()
        {
            SysCommands.Clear();
            SysQueryCommands.Clear();
            CommonCommands.Clear();
            TreeCommands.Clear();
            TreeExpandCommands.Clear();
            RootCommands.Clear();
        }
    }
}