Ext.define('Oea.rbac.ClearAuditLogCommand', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "清空日志" }
    },
    execute: function (listView, source) {
        Ext.Msg.show({
            title: '请确认', msg: '确定清空所有日志？', buttons: Ext.Msg.OKCANCEL,
            width: 300, animateTarget: source,
            fn: function (btn) {
                if (btn == 'ok') {
                    Oea.invokeSvc({
                        svc: 'OEA.RBAC.ClearLogService',
                        callback: function () {
                            listView.reloadData();
                        }
                    });
                }
            }
        });
    }
});