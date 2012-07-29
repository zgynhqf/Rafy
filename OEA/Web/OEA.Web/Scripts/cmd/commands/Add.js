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

Ext.define('Oea.cmd.Add', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "添加", group: "edit" }
    },
    canExecute: function (listView) {
        var p = listView.getParent();
        if (p == null) return true;

        //如果父对象是新加的对象，则不能使用
        var c = p.getCurrent();
        return c != null && !c.phantom;
    },
    execute: function (listView) {
        var m = listView.addNew();
        if (m) listView.startEdit(m, 0);
    }
});