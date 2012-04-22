/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101125
 * 说明：一般的单个命令的分组算法
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101125
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 一般的单个命令的分组算法
    /// 
    /// 使用这个类时，只需要指定分组后需要用到的生成器类型就可以了。
    /// </summary>
    /// <typeparam name="TGenerator"></typeparam>
    public class GenericItemAlgorithm<TGenerator> : GroupAlgorithm
        where TGenerator : GroupGenerator
    {
        protected override void GroupCommandCore(WPFCommand cmd)
        {
            //为每一个命令都声明一个单独的组
            var group = AddNewGroup(cmd.Name);

            //构造并设置控件生成器
            var generator = Activator.CreateInstance(typeof(TGenerator)) as GroupGenerator;
            generator.CommandGroup = group;
            generator.Context = this.Context;
            group.Generator = generator;

            group.AddCommand(cmd);
        }
    }
}
