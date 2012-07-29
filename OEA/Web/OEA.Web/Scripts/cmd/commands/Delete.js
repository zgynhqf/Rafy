/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Ext.define('Oea.cmd.Delete', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "删除", group: "edit" }
    },
    execute: function (listView) {
        listView.removeSelection();
    }
});