Ext.define('LookupSelectAddEntity', {
    extend: 'Oea.cmd.LookupSelectAdd',
    targetRefProperty: null,
    onSelected: function (listView, selection) {
        if (selection.length > 0) {
            var store = listView.getData();
            var pName = this.targetRefProperty;

            //只需要把选中对象的 id 赋值到新对象上即可。
            for (var i = 0; i < selection.length; i++) {
                var selected = selection[i];
                var id = selected.get("Id");
                var index = store.findExact(pName, id);
                if (index < 0) {
                    var item = listView.addNew();
                    item.set(pName, id);
                }
            }

            listView.save(function (res) {
                if (res.success) {
                    Ext.Msg.alert('提示', '保存成功！');
                }
                else {
                    Ext.Msg.alert('保存失败', res.msg);
                }
            });
        }
    }
});

//block end

Ext.define('LookupSelectAddOrgPosition', {
    extend: 'LookupSelectAddEntity',

    constructor: function () {
        this.callParent(arguments);

        this.viewName = "只显示编码名称";
        this.targetRefProperty = "PositionId";
    }
});

//block end

Ext.define('LookupSelectAddOrgPositionUser', {
    extend: 'LookupSelectAddEntity',

    constructor: function () {
        this.callParent(arguments);

        this.targetRefProperty = "UserId";
    }
});