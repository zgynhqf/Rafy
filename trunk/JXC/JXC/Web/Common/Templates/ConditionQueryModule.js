//abstract
Ext.define('Jxc.ConditionQueryModule', {
    extend: 'Oea.UITemplate',
    onUIGenerated: function (ui) {
        var me = this;

        var listView = ui.getView();

        //默认发起一次查询。
        var queryView = listView.getConditionView();
        if (queryView != null) queryView.tryExecuteQuery();

        listView.setIsReadonly(true);

        //列表双击时，弹出查看窗口
        var g = listView.getControl();
        g.on('render', function () {
            var e = g.getEl();
            e.on('dblclick', function () {
                var cmd = listView.findCmd(Jxc.ShowBill);
                if (cmd != null) { cmd.tryExecute(listView, this); }
            });
        });

        this.callParent(arguments);
    }
});