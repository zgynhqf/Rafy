/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130816
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130816 15:14
 * 
*******************************************************/

//internal
//本类型用于封装从客户端实体中获取用于提交到服务端的变更集，所有实体都是以列表的形式来进行提交。
Ext.define('Rafy.data.ListChangeSet', {
    //internal
    _data: null,

    //internal
    //根实体的类型
    _model: null,

    //-------------------------------------  Common -------------------------------------
    getModel: function () {
        /// <summary>
        /// 获取根实体的类型。
        /// </summary>
        /// <returns type="Class"></returns>
        return this._model;
    },
    isEmpty: function () {
        /// <summary>
        /// 判断数据变更集中是否有需要提交到服务端的数据。
        /// </summary>
        /// <param name="changeSetData"></param>
        /// <returns type=""></returns>

        var d = this._data;

        if (this._model.isTree) {
            return !(d.d && d.d.length
                || d.roots && d.roots.length);
        }
        else {
            return !(d.u && d.u.length
                || d.c && d.c.length
                || d.d && d.d.length
                || d.uc && d.uc.length);
        }
    },
    getSubmitData: function () {
        /// <summary>
        /// 转换为可以提交到服务端的 Json 对象（非字符串）。
        /// </summary>
        /// <returns type=""></returns>

        //根实体，在数据提交到服务端前，我们需要在纯粹的数据上添加字符串属性 _model，
        //用以告诉服务端这个数据应该解析为哪个实体类。
        var d = this._data;
        if (!d._model) {
            d._model = Ext.getClassName(this._model);
        }
        return d;
    }
});