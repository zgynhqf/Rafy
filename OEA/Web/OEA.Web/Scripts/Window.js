Ext.define('Oea.Window', {
    singleton: true,
    show: function (config) {
        var me = this;
        Ext.applyIf(config, {
            width: 400,
            bodyPadding: 5,
            layout: 'fit',
            autoScroll: true,
            modal: true,
            buttons: '确定,取消'
        });

        if (Ext.isString(config.buttons)) {
            var btnCfg = [];
            var list = config.buttons.split(',');
            Oea.each(list, function (btn) {
                btnCfg.push({
                    text: btn, width: 100,
                    handler: me._createButtonHandler(btn, config.callback)
                });
            });

            config.buttons = btnCfg;
        }

        var win = Ext.create('Ext.window.Window', config).show();

        win.center();

        return win;
    },
    _createButtonHandler: function (btn, callback) {
        return function () {
            if (callback) {
                var handled = callback(btn);
                if (Ext.isDefined(handled) && !handled) {
                    return;
                }
            }
            var win = this.up('window');
            win.close();
        };
    }
});