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

Ext.define('Rafy.Window', {
    singleton: true,
    show: function (opt) {
        /// <summary>
        /// 弹出一个窗体。
        /// </summary>
        /// <param name="opt">
        /// buttons: ['确定'.t(),'取消'.t()]，这里定义的按钮列表，也是回调中传出来的按钮名称。
        /// callback: 回调。参数：
        ///     btn：被点击的按钮名称。
        /// 其它：支持所有 window 的配置。
        /// </param>
        /// <returns></returns>

        var me = this;
        Ext.applyIf(opt, {
            bodyPadding: 5,
            layout: 'fit',
            autoScroll: true,
            modal: true
        });

        //如果没有定义宽度，则宽度设置为屏幕宽度的 80%
        if (!opt.width) {
            opt.width = document.body.clientWidth * 0.8;
        }

        //转换按钮的配置。
        me._convertButtons(opt);

        var win = Ext.create('Ext.window.Window', opt).show();

        win.center();

        return win;
    },
    _convertButtons: function (opt) {
        var me = this;

        if (!opt.buttons) {
            opt.buttons = ['确定'.t(), '取消'.t()];
        }

        if (Ext.isString(opt.buttons)) {
            opt.buttons = opt.buttons.split(',');
        }

        var btnCfg = [];
        Rafy.each(opt.buttons, function (btn) {
            //只转换字符串；对象则不进行转换。
            if (Ext.isString(btn)) {
                btnCfg.push({
                    text: btn, width: 100,
                    handler: me._createButtonHandler(btn, opt.callback)
                });
            }
            else {
                btnCfg.push(btn);
            }
        });

        opt.buttons = btnCfg;
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