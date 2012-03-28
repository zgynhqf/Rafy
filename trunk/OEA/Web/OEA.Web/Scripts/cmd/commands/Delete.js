Ext.define('Oea.cmd.Delete', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "删除" }
    },
    execute: function (listView) {
        listView.removeSelection();
    }
});