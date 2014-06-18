/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130903
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130903 11:01
 * 
*******************************************************/

//说明：调用服务对用户信息进行保存的命令。
Rafy.defineCommand('Rafy.rbac.cmd.SaveUser', {
    meta: { text: "保存" },
    execute: function (listView, source) {
        Rafy.invokeService({
            svc: 'Rafy.RBAC.SaveUserService',
            svcInput: {
                EntityList: listView.getData()
            },
            callback: function (res) {
                if (res.Success) {
                    listView.loadData();
                }
                else {
                    Ext.Msg.alert('提示', res.Message);
                }
            }
        });
    }
});