/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：客户端命令
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Ext.define('Oea.cmd.Command', {
    extend: 'Ext.util.Observable',

    _curView: null,//当前正在执行的视图

    constructor: function (config) {
        this.callParent(arguments);

        this.initConfig(config);
        this.addEvents('executed', 'executeFailed');
    },

    //internal
    _modifyMeta: function (meta) {
        //把本类中定义的 meta 再次修改到参数中，接下来会用修改后的 meta 来生成界面
        Ext.applyIf(meta, this.config.meta);
    },

    tryExecute: function (view, source) {
        /// <summary>
        /// 尝试执行某个命令
        /// </summary>
        /// <param name="view"></param>
        /// <param name="source"></param>
        if (this.canExecute(view, source)) {
            this._executeInternal(view, source);
            return true;
        }

        return false;
    },

    //protected
    getView: function () {
    	/// <summary>
        /// 获取当前正在执行的视图
    	/// </summary>
    	/// <returns></returns>
        return this._curView;
    },

    //virtual
    canExecute: function (view, source) {
        /// <summary>
        /// 子类重写此方法来执行本命令的是否可执行的逻辑。
        /// </summary>
        /// <param name="view">本命令作用的视图对象。</param>
        /// <param name="source">引发本命令的源控件。</param>
        /// <returns>返回是否可执行，默认 true。</returns>
        return true;
    },
    //abstract
    execute: function (view, source) {
        /// <summary>
        /// 子类重写此方法来执行本命令的逻辑。
        /// </summary>
        /// <param name="view">本命令作用的视图对象。</param>
        /// <param name="source">引发本命令的源控件。</param>
        Oea.markAbstract();
    },

    //internal
    _executeInternal: function (view, source) {
        this._curView = view;

        if (Oea.isDebugging()) {
            this.execute(view, source);
        }
        else {
            try {
                this.execute(view, source);

                this.fireEvent('executed', { view: view });
            } catch (e) {
                //如果事件处理函数设置了 cancel 为 true，则表示异常已经被处理。
                var args = { exception: e, view: view };
                this.fireEvent('executeFailed', args);
                if (!args.cancel) throw e;
            }
        }
    },

    //方便的方法：

    isParentSelected: function (view) {
        var p = view.getParent();
        if (!p) { return true }

        return p.getCurrent() != null;
    }
});