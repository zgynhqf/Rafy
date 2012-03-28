Ext.define('Oea.view.DetailView', {
    extend: 'Oea.view.View',
    getData: function () {
        return this.getCurrent();
    },
    setData: function (value) {
        this.setCurrent(value);
    },
    //override
    _serializeData: function (opt) {
        var me = this;
        var c = me.getCurrent();
        if (c != null) {

            var api = Oea.data.ListChangeSet;
            var data = api._getItemData(c, me._isTree, opt.withUnchanged);

            if (opt.withChildren) {
                api._eachItemInData(data, me._isTree, function (item) {
                    me._serailizeChildrenData(item, opt);
                    return false;
                });
            }

            return data;
        }
    },
    //    getCurrent: function () {
    //        return this.getControl().getRecord();
    //    },
    _onCurrentChanged: function (oldValue, entity) {
        this.callParent(arguments);

        this.getControl().loadRecord(entity);

        this._onDataChanged(entity);
    },
    updateEntity: function () {
        var form = this.getControl().getForm();
        var entity = form.getRecord();
        form.updateRecord(entity);
    }
});