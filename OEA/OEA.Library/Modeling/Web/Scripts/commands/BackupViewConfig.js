Ext.define('BackupViewConfig', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: {
            text: "重置",
            tooltip: "重置为原始配置"
        }
    },
    constructor: function () {
        this.svc = 'BackupViewConfigService';
    },
    execute: function (view) {
        var c = view.getCurrent();
        Oea.invokeSvc({
            svc: this.svc,
            svcParams: {
                Model: c.get("EntityType"),
                ViewName: c.get("ViewName")
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

//command end

Ext.define('RestoreViewConfig', {
    extend: 'BackupViewConfig',
    config: {
        meta: {
            text: "还原",
            tooltip: "还原为最新的配置"
            //            ,hidden: true
        }
    },
    constructor: function () {
        this.svc = 'RestoreViewConfigService';
    }
});

//command end

Ext.define('OpenConfigFile', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: {
            text: "XML",
            tooltip: "打开XML文件进行编辑",
            hidden: window.location.href.indexOf('localhost') < 0
        }
    },
    execute: function (view) {
        var c = view.getCurrent();
        Oea.invokeSvc({
            svc: 'GetBlockConfigFileService',
            svcParams: {
                Model: c.get("EntityType"),
                ViewName: c.get("ViewName")
            },
            callback: function (res) {
                if (!res.Opened) {
                    Ext.Msg.alert("提示", "暂时还没有进行任何配置，没有找到对应的 XML 文件。");
                }
            }
        });
    }
});