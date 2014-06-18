Ext.define('CountServerBookCommand', {
    extend: 'Rafy.cmd.Command',
    config: {
        meta: { text: "统计服务器所有书量" }
    },
    execute: function (listView) {
        Rafy.invokeSvc({
            svc: 'CountServerBookService',
            svcParams: {
                Books: listView.serializeData({
                    withUnchanged: true
                })
            },
            callback: function (res) {
                alert("所有的书：" + res.BookCount);
            }
        });
    }
});