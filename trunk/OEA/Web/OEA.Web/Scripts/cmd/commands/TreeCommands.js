Ext.define('Oea.cmd.Insert', {
    extend: 'Oea.cmd.Add',
    config: {
        meta: { text: "添加子" }
    },
    execute: function (listView) {
        var m = listView.insertNewChild();
        if (m) listView.startEdit(m, 0);
    }
});
//command end
Ext.define('Oea.cmd.ExpandAll', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "展开" }
    },
    execute: function (listView) {
        listView.expandSelection();
    }
});
//command end
Ext.define('Oea.cmd.CollaseAll', {
    extend: 'Oea.cmd.Add',
    config: {
        meta: { text: "折叠" }
    },
    execute: function (listView) {
        listView.collapseSelection();
    }
});