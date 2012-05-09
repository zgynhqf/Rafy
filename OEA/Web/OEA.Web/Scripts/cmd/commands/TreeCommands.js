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