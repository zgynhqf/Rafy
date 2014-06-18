Rafy.defineCommand('Rafy.customization.cmd.SaveViewConfig', {
    meta: { text: "保存" },
    execute: function (view, source) {
        view.updateEntity();

        view.save({
            withChildren: true,
            callback: function (res) {
                if (res.Success) {
                    //该方法为 CustomizeUI command 添加
                    view.reloadViewConfiguration();
                }
                else {
                    Ext.Msg.alert('保存失败', res.Message);
                }
            }
        });
    }
});