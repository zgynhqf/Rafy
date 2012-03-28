Ext.define('Oea.cmd.Add', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "添加" }
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