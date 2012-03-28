Ext.define('CountServerBookCommand', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "统计服务器所有书量" }
    },
    execute: function () {
        Oea.invokeSvc({
            svc: 'CountServerBookService',
            callback: function (res) {
                alert("所有的书：" + res.BookCount);
            }
        });
    }
});