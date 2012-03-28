Ext.define('Oea.cmd.Command', {
    constructor: function (config) {
        this.initConfig(config);
    },

    //internal
    _modifyMeta: function (meta) {
        //把本类中定义的 meta 再次修改到参数中，接下来会用修改后的 meta 来自成界面
        Ext.applyIf(meta, this.config.meta);
    },

    //virtual
    canExecute: function (view, source) {
        return true;
    },
    //virtual
    execute: function (view, source) {
        Oea.markAbstract();
    },

    //方便的方法：

    isParentSelected: function (view) {
        var p = view.getParent();
        if (!p) { return true}

        return p.getCurrent() != null;
    }
});