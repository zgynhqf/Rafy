Ext.define('Oea.Window', {
    singleton: true,
    show: function (config) {
        /// <summary>
        /// 弹出一个窗体。
        /// </summary>
        /// <param name="config">
        /// buttons: '确定,取消'，这里定义的按钮列表，也是回调中传出来的按钮名称。
        /// callback: 回调。参数：
        ///     btn：被点击的按钮名称。
        /// 其它：支持所有 window 的配置。
        /// </param>
        /// <returns></returns>
        var me = this;
        Ext.applyIf(config, {
            bodyPadding: 5,
            layout: 'fit',
            autoScroll: true,
            modal: true,
            buttons: '确定,取消'
        });

        //如果没有定义宽度，则宽度设置为屏幕宽度的 80%
        if (!config.width) {
            config.width = document.body.clientWidth * 0.8;
        }

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
                if (handled === false) { return; }
            }
            var win = this.up('window');
            win.close();
        };
    }
});