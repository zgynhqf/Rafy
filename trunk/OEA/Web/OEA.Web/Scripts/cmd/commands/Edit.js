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

Ext.define('Oea.cmd.Edit', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "编辑", group: "edit" }
    },
    //private
    _meta: null,
    //public override
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
    //private
    _showDetailWin: function (entity, source) {
        var me = this;
        var view = Oea.AutoUI.createDetailView(me._meta, entity);
        Oea.Window.show({
            title: '编辑' + me._meta.label, width: 400, animateTarget: source,
            items: view.getControl(),
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