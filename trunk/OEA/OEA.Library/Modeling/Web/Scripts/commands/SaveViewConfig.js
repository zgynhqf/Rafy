Ext.define('SaveViewConfig', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "保存" }
    },
    execute: function (view, source) {
        view.updateEntity();

        view.save({
            withUnchanged: true,
            withChildren: true,
            autoLoad: false,
            callback: function (res) {
                if (res.success) {
                    //该方法为 CustomizeUI command 添加
                    view.reloadViewConfiguration();
                }
                else {
                    Ext.Msg.alert('保存失败', res.msg);
                }
            }
        });
    }
});