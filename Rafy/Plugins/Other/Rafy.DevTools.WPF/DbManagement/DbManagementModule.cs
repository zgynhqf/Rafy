﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 11:59
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 11:59
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.Domain.ORM.DbMigration.Presistence;

namespace Rafy.DevTools.DbManagement
{
    /// <summary>
    /// 数据库升级模块。
    /// </summary>
    public class DbManagementModule : UITemplate
    {
        public DbManagementModule()
        {
            this.EntityType = typeof(DbMigrationHistory);
        }

        protected override void OnBlocksDefined(AggtBlocks blocks)
        {
            //添加一个升级数据库的按钮。
            blocks.MainBlock.ViewMeta.AsWPFView().UseCommands(typeof(MigrateDatabaseCommand));

            base.OnBlocksDefined(blocks);
        }
    }
}
