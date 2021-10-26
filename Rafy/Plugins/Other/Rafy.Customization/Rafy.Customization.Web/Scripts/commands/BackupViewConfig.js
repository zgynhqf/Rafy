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

Rafy.defineCommand('Rafy.customization.cmd.BackupViewConfig', {
    meta: { text: "重置", tooltip: "重置为原始配置" },
    constructor: function () {
        this.callParent(arguments);

        this.svc = 'Rafy.BackupViewConfigService';
    },
    execute: function (view) {
        var c = view.getCurrent();
        Rafy.invokeService({
            svc: this.svc,
            svcInput: {
                Model: c.getEntityType(),
                ViewName: c.getViewName()
            },
            callback: function (res) {
                if (res.Success) {
                    //该方法为 CustomizeUI command 添加
                    view.reloadViewConfiguration();

                    //                    //动态变化还原最新配置的可见性
                    //                    var cmdCtl = view.getCmdControl('RestoreViewConfig');
                    //                    if (cmdCtl.hidden) { cmdCtl.show(); }
                    //                    else { cmdCtl.hide(); }
                }
            }
        });
    }
});

//rafy:commandEnd

Rafy.defineCommand('Rafy.customization.cmd.RestoreViewConfig', {
    extend: 'Rafy.customization.cmd.BackupViewConfig',
    meta: { text: "还原", tooltip: "还原为最新的配置"/*,hidden: true */ },
    constructor: function () {
        this.callParent(arguments);

        this.svc = 'Rafy.RestoreViewConfigService';
    }
});

//rafy:commandEnd

Rafy.defineCommand('Rafy.customization.cmd.OpenConfigFile', {
    meta: { text: "XML", tooltip: "打开XML文件进行编辑", hidden: window.location.href.indexOf('localhost') < 0 },
    execute: function (view) {
        var c = view.getCurrent();
        Rafy.invokeService({
            svc: 'Rafy.GetBlockConfigFileService',
            svcInput: {
                Model: c.getEntityType(),
                ViewName: c.getViewName()
            },
            callback: function (res) {
                if (!res.Opened) {
                    Ext.Msg.alert("提示", "暂时还没有进行任何配置，没有找到对应的 XML 文件。");
                }
            }
        });
    }
});