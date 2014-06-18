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

Ext.define('Rafy.view.DetailView', {
    extend: 'Rafy.view.View',
    getData: function () {
        return this.getCurrent();
    },
    setData: function (value) {
        this.setCurrent(value);
    },
    //    getCurrent: function () {
    //        return this.getControl().getRecord();
    //    },
    _onCurrentChanged: function (oldValue, entity) {
        this.callParent(arguments);

        this.getControl().loadRecord(entity);

        this._onDataChanged(entity);

        this._attachDynamicVisibility(entity);
    },

    //protected override
    _onIsReadonlyChanged: function (value) {
        Rafy.notImplement("暂时没有实现表单的 IsReadonly 属性。");
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
    },

    //------------------------------------- Dynamic Visibility -------------------------------------
    _dve: null,
    _attachDynamicVisibility: function (curEntity) {
        /// <summary>
        /// 根据当前对象来处理它的动态可见性属性。
        /// </summary>
        /// <param name="curEntity"></param>
        var me = this;
        if (curEntity != null) {
            var dve = this._getDynamicVisibleEditors();
            if (dve.length) {
                this._initDynamicVisibility();

                curEntity.on('propertyChanged', function (e) {
                    //由于没有把事件从之前的 current 对象上移除，所以这里需要对事件的发起者进行过滤。
                    if (e.entity == me.getCurrent()) {
                        for (var i = 0; i < dve.length; i++) {
                            var de = dve[i];
                            if (de.visibilityIndicator == e.property) {
                                var editor = me.findEditor(de.property);
                                editor.setVisible(e.value);

                                return false;
                            }
                        }
                    }
                });
            }
        }
    },
    _initDynamicVisibility: function () {
        var dve = this._getDynamicVisibleEditors();
        if (dve.length) {
            //由于没有把事件从之前的 current 对象上移除，所以这里需要对事件的发起者进行过滤。
            var entity = this.getCurrent();

            for (var i = 0; i < dve.length; i++) {
                var de = dve[i];
                var value = entity.get(de.visibilityIndicator);
                this.findEditor(de.property).setVisible(value);
            }
        }
    },
    _getDynamicVisibleEditors: function () {
        /// <summary>
        /// 获取一个动态可见性的属性列表，每一个元素有两个属性：property，visibilityIndicator，
        /// 表示某个属性应该根据另外一个属性来动态改变它的可见性。
        /// </summary>
        /// <returns></returns>
        if (!this._dve) {
            var a = [];

            var items = this.getMeta().formConfig.items;
            Rafy.each(items, function (i) {
                if (i.visibilityIndicator) {
                    a.push({
                        property: i.name,
                        visibilityIndicator: i.visibilityIndicator
                    });
                }
            });

            this._dve = a;
        }

        return this._dve;
    },
    _setControl: function (value) {
        this.callParent(arguments);

        //由于要支持 DynamicVisibility，所以让所有的 checkBox 支持及时反馈数据到实体。
        if (value) {
            var me = this;
            Rafy.each(value.items, function (field) {
                //!!!以下代码参考：Ext.Checkbox.onBoxClick。
                if (field.isCheckbox && !field.disabled && !field.readOnly) {
                    field.on('render', function () {
                        field.getEl().on('click', function () {
                            var e = me.getCurrent();
                            if (e) {
                                var n = field.getName();
                                var v = field.getValue();
                                e.set(n, v);
                            }
                        });
                    });
                }
            });
        }
    }
});