Ext.define('CountLocalBookCommand', {
    extend: 'Rafy.cmd.Command',
    config: {
        meta: { text: "统计" }
    },
    execute: function (listView) {
        var store = listView.getData();
        Ext.Msg.alert("统计", "书量：" + store.getCount());
    }
});