Ext.define('Oea.cmd.Delete', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "删除", group: "edit" }
    },
    execute: function (listView) {
        listView.removeSelection();
    }
});