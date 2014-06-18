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

Rafy.defineCommand('Rafy.cmd.LookupSelectAdd', {
    meta: { text: "选择添加", group: "edit" },

    viewName: null,

    canExecute: function (view) {
        var p = view.getParent();
        if (!p) { return true; }

        return p.getCurrent() != null;
    },
    execute: function (listView, source) {
        var me = this;
        var cmdMeta = me.getMeta();
        if (cmdMeta.targetClass) {
            Rafy.AutoUI.getMeta({
                model: cmdMeta.targetClass,//这个参数需要在服务端使用自定义参数进行设置
                viewName: me.viewName,
                ignoreCommands: true,
                isReadonly: true,
                callback: function (block) {
                    block.gridConfig.selType = 'checkboxmodel';
                    block.gridConfig.selModel = { checkOnly: true };
                    block.storeConfig.pageSize = 15;

                    var view = Rafy.AutoUI.createListView(block);

                    Rafy.Window.show({
                        title: cmdMeta.text, animateTarget: source, items: view.getControl(),
                        width: 600, height: 400, callback: function (btn) {
                            if (btn == "确定".t()) {
                                me._onSelected(listView, view.getSelection());
                            }
                        }
                    });

                    view.loadData();
                }
            });
        }
    },

    _onSelected: function (listView, selection) {
        /// <summary>
        /// protected virtual
        /// </summary>
        /// <param name="listView">当前按钮对应的列表视图。</param>
        /// <param name="selection">弹出窗口中选择的实体对象集合。</param>
        Rafy.markAbstract();
    }
});
