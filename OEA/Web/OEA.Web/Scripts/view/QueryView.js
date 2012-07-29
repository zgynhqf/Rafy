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

Ext.define('Oea.view.QueryView', {
    extend: 'Oea.view.DetailView',
    getResultView: function () {
        return this.findRelationView(Oea.view.RelationView.result);
    },
    attachNewEntity: function () {
        var model = this.getModelClass();
        var entity = Ext.create(model);
        this.setCurrent(entity);
    },
    tryExecuteQuery: function () {
        //暂时直接发起查询
        this._executeQuery();
    },
    _executeQuery: function () {
        this.updateEntity();
        var e = this.getCurrent();
        this.getResultView().loadData({ criteria: e });
    }
});

Ext.define('Oea.view.NavigationView', {
    extend: 'Oea.view.QueryView'
});

Ext.define('Oea.view.ConditionView', {
    extend: 'Oea.view.QueryView'
});