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

Rafy.defineCommand('Rafy.cmd.Insert', {
    extend: 'Rafy.cmd.Add',
    meta: { text: "添加子", group: "edit" },
    execute: function (listView) {
        var m = listView.insertNewChild();
        if (m) listView.startEdit(m, 0);
    }
});
//rafy:commandEnd
Rafy.defineCommand('Rafy.cmd.ExpandAll', {
    meta: { text: "展开", group: "view" },
    execute: function (listView) {
        listView.expandSelection();
    }
});
//rafy:commandEnd
Rafy.defineCommand('Rafy.cmd.CollaseAll', {
    meta: { text: "折叠", group: "view" },
    execute: function (listView) {
        listView.collapseSelection();
    }
});