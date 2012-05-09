Ext.define('Oea.cmd.LookupSelectAdd', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "选择添加", group: "edit" }
    },

    viewName: null,

    canExecute: function (view) {
        return this.isParentSelected(view);
    },
    execute: function (listView, source) {
        var me = this;
        var cmdMeta = me.getMeta();
        if (cmdMeta.targetClass) {
            Oea.AutoUI.getMeta({
                model: cmdMeta.targetClass,//这个参数需要在服务端使用自定义参数进行设置
                viewName: me.viewName,
                ignoreCommands: true,
                isReadonly: true,
                callback: function (block) {
                    block.gridConfig.selType = 'checkboxmodel';
                    block.gridConfig.selModel = { checkOnly: true };
                    block.storeConfig.pageSize = 15;

                    var view = Oea.AutoUI.createListView(block);

                    Oea.Window.show({
                        title: cmdMeta.text, animateTarget: source, items: view.getControl(),
                        width: 600, height: 400, callback: function (btn) {
                            if (btn == "确定") {
                                me.onSelected(listView, view.getSelection());
                            }
                        }
                    });

                    view.loadData();
                }
            });
        }
    },

    onSelected: function (listView, selection) { Oea.markAbstract(); }
});
