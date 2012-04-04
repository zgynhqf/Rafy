Ext.define('LookupSelectAddEntity', {
    extend: 'Oea.cmd.LookupSelectAdd',
    getTargetPropertyName: function (listView, selection) { Oea.markAbstract(); },
    onSelected: function (listView, selection) {
        if (selection.length > 0) {
            var store = listView.getData();
            var pName = this.getTargetPropertyName();

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

//command end

Ext.define('LookupSelectAddOrgPosition', {
    extend: 'LookupSelectAddEntity',
    getViewName: function () { return "只显示编码名称"; },
    getTargetPropertyName: function (listView, selection) { return "PositionId"; }
});

//command end

Ext.define('LookupSelectAddOrgPositionUser', {
    extend: 'LookupSelectAddEntity',
    getTargetPropertyName: function (listView, selection) { return "UserId"; }
});