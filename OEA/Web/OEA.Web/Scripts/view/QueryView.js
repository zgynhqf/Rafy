Ext.define('Oea.view.QueryView', {
    extend: 'Oea.view.DetailView',
    getResultView: function () {
        return this.findRelationView(Oea.view.RelationView.result);
    },
    attachNewEntity: function () {
        var model = this.getModel();
        var entity = Ext.create(model);
        this.setCurrent(entity);
    },
    executeQuery: function () {
        this.updateEntity();
        var e = this.getCurrent();
        this.getResultView().loadData({
            criteria: e
        });
    }
});

Ext.define('Oea.view.NavigationView', {
    extend: 'Oea.view.QueryView'
});

Ext.define('Oea.view.ConditionView', {
    extend: 'Oea.view.QueryView'
});