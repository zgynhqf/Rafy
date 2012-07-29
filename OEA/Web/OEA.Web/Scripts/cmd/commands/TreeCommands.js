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

Ext.define('Oea.cmd.Insert', {
    extend: 'Oea.cmd.Add',
    config: {
        meta: { text: "添加子", group: "edit" }
    },
    execute: function (listView) {
        var m = listView.insertNewChild();
        if (m) listView.startEdit(m, 0);
    }
});
//block end
Ext.define('Oea.cmd.ExpandAll', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "展开", group: "view" }
    },
    execute: function (listView) {
        listView.expandSelection();
    }
});
//block end
Ext.define('Oea.cmd.CollaseAll', {
    extend: 'Oea.cmd.Add',
    config: {
        meta: { text: "折叠", group: "view" }
    },
    execute: function (listView) {
        listView.collapseSelection();
    }
});