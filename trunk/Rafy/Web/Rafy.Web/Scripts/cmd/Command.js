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

Ext.define('Rafy.cmd.Command', {
    extend: 'Ext.util.Observable',

    config: {
        meta: {
            text: '按钮',//text 用于描述按钮的显示文本。
            group: 'view'//group 描述按钮所在的分组，一般有以下分类：edit,view,business
        }
    },

    _ownerView: null,//当前正在执行的视图

    constructor: function (config) {
        this.callParent(arguments);

        this.initConfig(config);
        this.addEvents('executed', 'executeFailed');
    },

    _modifyMeta: function (meta) {
        /// <summary>
        /// internal 
        /// 
        /// 把本类中定义的 meta 再次修改到参数中，接下来会用修改后的 meta 来生成界面
        /// </summary>
        /// <param name="meta"></param>

        //getMeta 是 initConfig 生成的方法。
        Ext.applyIf(meta, this.getMeta());
    },
    _setOwnerView: function(value){
    	/// <summary>
        /// internal
        /// 设置本命令对应的视图。
    	/// </summary>
        /// <param name="value"></param>
        this._ownerView = value;
    },

    tryExecute: function (source) {
        /// <summary>
        /// 尝试执行某个命令
        /// </summary>
        /// <param name="source">引发本命令的源控件。</param>

        var v = this._ownerView;

        if (this.canExecute(v, source)) {
            this._executeInternal(v, source);
            return true;
        }

        return false;
    },

    getOwnerView: function () {
        /// <summary>
        /// 获取本命令所在的视图
        /// </summary>
        /// <returns></returns>
        return this._ownerView;
    },

    canExecute: function (view, source) {
        /// <summary>
        /// virtual
        /// 子类重写此方法来执行本命令的是否可执行的逻辑。
        /// </summary>
        /// <param name="view">本命令作用的视图对象。</param>
        /// <param name="source">引发本命令的源控件。</param>
        /// <returns>返回是否可执行，默认 true。</returns>
        return true;
    },
    execute: function (view, source) {
        /// <summary>
        /// abstract
        /// 子类重写此方法来执行本命令的逻辑。
        /// </summary>
        /// <param name="view">本命令作用的视图对象。</param>
        /// <param name="source">引发本命令的源控件。</param>
        Rafy.markAbstract();
    },

    _executeInternal: function (view, source) {
        /// <summary>
        /// 直接执行本命令。
        /// </summary>
        /// <param name="view">本命令作用的视图对象。</param>
        /// <param name="source">引发本命令的源控件。</param>

        //如果是要调试状态，则不需要截住异常并记录错误日志，否则 chrome 调试器不能定位到异常代码处。
        if (Rafy.isDebugging()) {
            this.execute(view, source);
            this.fireEvent('executed', { view: view });
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
    }
});