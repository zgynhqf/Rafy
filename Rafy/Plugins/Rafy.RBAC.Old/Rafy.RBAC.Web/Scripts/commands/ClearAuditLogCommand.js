/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Rafy.defineCommand('Rafy.rbac.cmd.ClearAuditLogCommand', {
    meta: { text: "清空日志" },
    execute: function (listView, source) {
        Ext.Msg.show({
            title: '请确认', msg: '确定清空所有日志？', buttons: Ext.Msg.OKCANCEL,
            width: 300, animateTarget: source,
            fn: function (btn) {
                if (btn == 'ok') {
                    Rafy.invokeService({
                        svc: 'Rafy.RBAC.Old.ClearLogService',
                        callback: function () {
                            listView.reloadData();
                        }
                    });
                }
            }
        });
    }
});