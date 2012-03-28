Ext.define('Oea.cmd.Refresh', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "刷新" }
    },
    execute: function (listView) {
        listView.loadData();
    }
});