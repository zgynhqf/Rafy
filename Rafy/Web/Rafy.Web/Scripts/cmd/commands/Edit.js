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

Rafy.defineCommand('Rafy.cmd.Edit', {
    meta: { text: "编辑", group: "edit" },
    _meta: null,
    execute: function (listView, source) {
        var me = this;
        var entity = listView.getCurrent();
        if (entity != null) {
            if (!me._meta) {
                Rafy.AutoUI.getMeta({
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
        var view = Rafy.AutoUI.createDetailView(me._meta, entity);
        Rafy.Window.show({
            title: '编辑'.t() + me._meta.label, width: 400, animateTarget: source,
            //autoScroll: false,//不需要使用自动滚动，否则会出现异常的滚动条。//需要滚动条，因为需要拖动窗口大小。
            items: view.getControl(),
            callback: function (btn) {
                if (btn == "确定".t()) {
                    var form = view.getControl().getForm();
                    if (!form.isValid()) {
                        Ext.Msg.alert('失败'.t(), '表单中的值不可用。'.t());
                        return false;
                    }
                    view.updateEntity();
                }
            }
        });
    }
});