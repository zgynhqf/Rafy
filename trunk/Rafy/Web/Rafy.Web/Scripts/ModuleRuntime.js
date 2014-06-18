/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120502
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120502
 * 
*******************************************************/

//模块的运行时类型。
Ext.define('Rafy.ModuleRuntime', {
    extend: 'Ext.util.Observable',

    _moduleMeta: null,

    constructor: function () {
        this.callParent(arguments);

        this.addEvents('uiGenerated');
    },

    getMeta: function () {
        /// <summary>
        /// 返回本模板对应的模块元数据类型。
        /// </summary>
        /// <returns type=""></returns>
        return this._moduleMeta;
    },
    _setMeta: function (value) {
    	/// <summary>
        /// internal
    	/// </summary>
    	/// <param name="value"></param>
        this._moduleMeta = value;
    },

    _notifyUIGenerated: function (ui) {
    	/// <summary>
    	/// internal
    	/// </summary>
    	/// <param name="ui"></param>
        this._onUIGenerated(ui);
    },

    _onUIGenerated: function (ui) {
        /// <summary>
        /// protected virtual
        /// 当模块界面生成完毕时调用此方法。
        /// </summary>
        /// <param name="ui"></param>
        this.fireEvent('uiGenerated', { ui: ui });
    }
});