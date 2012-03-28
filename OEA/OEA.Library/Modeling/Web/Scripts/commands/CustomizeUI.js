Ext.define('Oea.cmd.CustomizeUI', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "界面配置" }
    },
    execute: function (listView, source) {
        var me = this;
        var targetMeta = listView.getMeta();

        Oea.AutoUI.getMeta({
            model: 'OEA.ViewConfigurationModel',
            viewName: 'ViewConfigurationModel模块界面',
            isAggt: true,
            callback: function (meta) {
                var controlResult = Oea.AutoUI.generateAggtControl(meta);

                var win = Oea.Window.show({
                    title: '定制' + targetMeta.label,
                    width: Ext.getBody().getWidth() - 50, height: 400,
                    animateTarget: source,
                    items: controlResult.getControl(),
                    buttons: '刷新,关闭',
                    callback: function (btn) {
                        if (btn == '刷新') {
                            window.location.reload();
                        }
                    }
                });

                var criteria = Ext.create('OEA.ViewConfigurationModelNameCriteria', {
                    EntityType: targetMeta.model,
                    ViewName: targetMeta.viewName
                });

                var v = controlResult.getView();

                //这个参数可以被 BackupViewConfig.js 使用
                v.lastLoadCfg = {
                    model : meta.model,
                    criteria: criteria,
                    callback: function (entities) {
                        if (entities.length > 0) {
                            v.setCurrent(entities[0]);
                        } else {
                            Ext.msg.alert("没有找到对应的配置。");
                            win.close();
                        }
                    }
                };

                //添加一个临时方法供内部的按钮刷新使用
                v.reloadViewConfiguration = function () {
                    Oea.queryEntities(this.lastLoadCfg);
                };

                v.reloadViewConfiguration();
            }
        });
    }
});