Ext.define('Oea.cmd.Edit', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "编辑" }
    },
    execute: function (listView, source) {
        var me = this;
        var entity = listView.getCurrent();
        if (entity != null) {
            if (!me._meta) {
                Oea.AutoUI.getMeta({
                    async: false,
                    ignoreCommands: true,
                    model: listView.getMeta().model,
                    isDetail: true,
                    callback: function (meta) {
                        me._meta = meta;
                    }
                });
            }
            me._showDetailWin(entity, source);
        }
    },
    _showDetailWin: function (entity, source) {
        var me = this;
        var view = Oea.AutoUI.createDetailView(me._meta, entity);
        Oea.Window.show({
            title: '编辑' + me._meta.label,
            items: view.getControl(),
            animateTarget: source,
            callback: function (btn) {
                if (btn == "确定") {
                    var form = view.getControl().getForm();
                    if (!form.isValid()) {
                        Ext.Msg.alert('失败', '表单中的值不可用。');
                        return false;
                    }
                    view.updateEntity();
                }
            }
        });
    }
});