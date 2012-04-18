using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 一些内置的 web 命令
    /// </summary>
    public static class WPFCommandNames
    {
        public static Type CustomizeUI;
        public static Type FireQuery;
        public static Type PopupAdd;
        public static Type Add;
        public static Type SaveBill;
        public static Type SaveList;
        public static Type Cancel;
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

        //public static Type LookupSelectAdd;

        /*********************** 代码块解释 *********************************
         * 
         * 可以修改这几个集合中的数据以达到修改整个应用程序的效果。
         * 
        **********************************************************************/
        public static readonly List<Type> CommonCommands = new List<Type>();
        public static readonly List<Type> TreeCommands = new List<Type>();
        public static readonly List<Type> TreeExpandCommands = new List<Type>();
        public static readonly List<Type> RootCommands = new List<Type>();

        public static void InitCommonCommands()
        {
            WPFCommandNames.CommonCommands.AddRange(new Type[]{
                PopupAdd, Edit, Delete,
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
    }
}