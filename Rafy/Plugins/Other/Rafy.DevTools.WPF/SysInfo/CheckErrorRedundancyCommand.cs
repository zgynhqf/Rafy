/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130108 18:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130108 18:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.DevTools.SysInfo
{
    [Command(Label = "检查编码错误", ToolTip = "检查开发过程中的编码错误", GroupType = CommandGroupType.Business)]
    public class CheckErrorRedundancyCommand : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            var res = CheckErrorRedundancy();
            if (res)
            {
                App.MessageBox.Show("检查完成，未检测到编码错误。");
            }
        }

        /// <summary>
        /// 检测一些没有正确声明的冗余属性。
        /// </summary>
        private static bool CheckErrorRedundancy()
        {
            CommonModel.Entities.EnsureAllLoaded();
            foreach (var item in CommonModel.Entities)
            {
                var container = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(item.EntityType);
                foreach (var mp in container.GetCompiledProperties())
                {
                    var property = mp as IProperty;
                    if (property != null && property.IsRedundant)
                    {
                        var path = property.RedundantPath;
                        foreach (var refProperty in path.RefPathes)
                        {
                            var em = CommonModel.Entities.Find(refProperty.Owner);
                            if (em == null || em.TableMeta == null)
                            {
                                var msg = string.Format("由于不能找到对应的表，所以不支持把类型 {0} 中声明的引用属性 {1} 声明在冗余路径 {2} 中。",
                                    refProperty.Owner, refProperty.Name, path.GetPathExpression()
                                    );
                                App.MessageBox.Show(msg, "代码编写错误".Translate());
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}