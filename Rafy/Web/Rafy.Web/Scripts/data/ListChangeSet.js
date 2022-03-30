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
    //internal string
    //根实体的类型
    model: null,

    //-------------------------------------  Common -------------------------------------
    getModel: function () {
        /// <summary>
        /// 获取根实体的类型。
        /// </summary>
        /// <returns type="Class"></returns>
        return Rafy.getModel(this.model);
    },
    isEmpty: function () {
        /// <summary>
        /// 判断数据变更集中是否有需要提交到服务端的数据。
        /// </summary>
        /// <param name="changeSetData"></param>
        /// <returns type=""></returns>

        var d = this;
        var modelClass = this.getModel();
        if (modelClass.isTree) {
            return !(d.d && d.d.length
                || d.roots && d.roots.length);
        }
        else {
            return !(d.u && d.u.length
                || d.c && d.c.length
                || d.d && d.d.length
                || d.uc && d.uc.length);
        }
    }
});