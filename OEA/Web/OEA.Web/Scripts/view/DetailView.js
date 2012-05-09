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
        /// <summary>
        /// 实现类的数据序列化逻辑：
        /// 把实体类中的数据读取出来，转换为 json 对象。
        /// </summary>
        /// <param name="opt"></param>
        var me = this;
        var c = me.getCurrent();
        if (c != null) {

            //注意，单个实体的数据，依然是以 EntityList 的方式提交。
            //这样不但统一了数据的格式，而且还简单用实体列表的集合来分辨当前实体的状态（IsNew、IsDeleted）。
            var api = Oea.data.ListChangeSet;
            var data = api._getItemData(c, me._isTree, opt.withUnchanged, this.getModel());

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

    //protected override
    onIsReadonlyChanged: function (value) {
        Oea.notImplement("暂时没有实现表单的 IsReadonly 属性。");
    },

    updateEntity: function () {
    	/// <summary>
        /// 使用当前表单中的值来更新当前实体对象中的值。
    	/// </summary>
    	/// <returns>返回当前的实体</returns>
        var form = this.getControl().getForm();
        var entity = form.getRecord();
        form.updateRecord(entity);
        return entity;
    },

    updateControl: function () {
    	/// <summary>
    	/// 使用当前使用的值来更新界面上的表单
    	/// </summary>
        var c = this.getCurrent();
        if (c) { this.getControl().loadRecord(c); }
    },

    findEditor: function (property) {
        /// <summary>
        /// 根据属性名称来查找对象的 ext field 对象
        /// </summary>
        /// <param name="property">属性名称</param>

        var editors = this.getControl().items;
        for (var i = 0; i < editors.getCount() ; i++) {
            var editor = editors.getAt(i);
            if (editor.name == property) { return editor; }
        }

        return null;
    }
});