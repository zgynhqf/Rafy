Rafy.defineCommand('Rafy.customization.cmd.CustomizeUI', {
    meta: { text: "界面配置" },
    execute: function (listView, source) {
        var me = this;
        var targetMeta = listView.getMeta();

        Rafy.AutoUI.getMeta({
            model: 'Rafy.Customization.ViewConfigurationModel',
            viewName: 'ViewConfigurationModel模块界面',
            isAggt: true,
            callback: function (meta) {
                var ui = Rafy.AutoUI.generateAggtControl(meta);

                var cfgView = ui.getView();

                var win = Rafy.Window.show({
                    title: '定制'.t() + targetMeta.label,
                    width: 800, height: 400,
                    //width: Ext.getBody().getWidth() - 50, height: 400,
                    animateTarget: source,
                    items: ui.getControl(),
                    callback: function (btn) {
                        if (btn == '确定'.t()) {
                            me._saveAndRefreshModule(cfgView);
                        }
                    }
                });

                me._addReloadViewConfiguration(cfgView, meta, targetMeta, win);

                cfgView.reloadViewConfiguration();
            }
        });
    },
    _addReloadViewConfiguration: function (cfgView, meta, targetMeta, win) {
        /// <summary>
        /// 为 cfgView 添加 reloadViewConfiguration 方法。
        /// </summary>
        /// <param name="cfgView"></param>
        /// <param name="meta"></param>
        /// <param name="win"></param>
        var me = this, v = cfgView;

        //以下是查询的两种方式，都需要支持，这里随机调用，以方便测试。
        if (me._next = !me._next) {
            v.lastLoadCfg = {
                method: 'GetByEntity',
                params: [targetMeta.model, targetMeta.viewName]
            };
        }
        else {
            v.lastLoadCfg = {
                criteria: Ext.create('Rafy.Customization.ViewConfigurationModelNameCriteria', {
                    EntityType: targetMeta.model,
                    ViewName: targetMeta.viewName
                })
            };
        }
        Ext.apply(v.lastLoadCfg, {
            model: meta.model,
            callback: function (entities) {
                if (entities.length > 0) {
                    v.setCurrent(entities[0]);
                } else {
                    Ext.Msg.alert("没有找到对应的配置。");
                    win.close();
                }
            }
        });

        //添加一个临时方法供内部的按钮刷新使用，可以被 BackupViewConfig.js 使用。
        v.reloadViewConfiguration = function () {
            Rafy.data.EntityRepository.query(this.lastLoadCfg);
        };
    },
    _saveAndRefreshModule: function (cfgView) {
        //如果在点击刷新时，实体还没有保存，则需要先对实体进行保存后，才能进行刷新操作。
        var cfgEntity = cfgView.getCurrent();
        cfgView.updateEntity();
        if (cfgEntity.isDirty()) {
            var success = true;
            cfgView.save({
                async: false,
                withChildren: true,
                callback: function (res) {
                    success = res.Success;
                    if (!success) {
                        Ext.Msg.alert(res.Message);
                    }
                }
            });
            if (!success) return;
        }

        //提示并刷新模块界面。
        Ext.Msg.show({
            msg: '是否刷新当前模块界面？',
            buttons: Ext.Msg.YESNO,
            icon: Ext.Msg.QUESTION,
            fn: function (btn) {
                if (btn == 'yes') {
                    //使用工作区来重新打开当前模块。
                    var ws = Rafy.App.getWorkspace();
                    if (ws != null) {
                        var module = Rafy.App.getCurrentModule();
                        ws.removeModule(module);
                        Rafy.App.showModule(module);
                    }
                    else {
                        window.location.reload();
                    }
                }
            }
        });
    }
});