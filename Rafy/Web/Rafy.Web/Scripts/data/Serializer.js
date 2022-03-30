/*******************************************************
 * 
 * 作者：胡庆访
 * 说明：实体的序列化器。
 * 创建日期：20130815
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130815 16:44
 * 
*******************************************************/

//internal 本类是包类使用，外部不要使用。这是因为服务器与客户端数据传输的格式的变化比较大，需要封装在这个类中。
//实体到服务端的序列化器。
Ext.define('Rafy.data.Serializer', {
    //是否要递归序列化聚合子集合。
    //默认为: false, 是否需要转换聚合子视图中的数据。
    //    （否则在 Org->OrgPosition->OrgPositionUser 这种情况下，更改了 OrgPositionUser 而没有更改 OrgPosition，导致无法提交数据。 ）
    _withChildren: false,

    //-------------------------------------  API -------------------------------------
    statics: {
        serialize: function (component, withChildren) {
            /// <summary>
            /// 序列化指定的实体对象或数据集。
            /// </summary>
            /// <param name="component">要序列化的实体对象或数据集。</param>
            /// <param name="withChildren">是否要递归序列化聚合子集合。</param>
            /// <returns type="Object">存放数据的对象。</returns>

            var instance = new Rafy.data.Serializer();
            instance._withChildren = !!withChildren;

            var changeSet = new Rafy.data.ListChangeSet();

            if (component.isModel) {
                var data = instance._serializeEntity(component);
                Ext.apply(changeSet, data);
                changeSet.model = Ext.getClassName(component);
            }
            else {
                var storeData = instance._serializeStore(component);
                Ext.apply(changeSet, storeData);
                changeSet.model = Ext.getClassName(component.model || component);
            }

            return changeSet;
        }
    },

    _serializeEntity: function (entity) {
        /// <summary>
        /// 获取某个实体中需要提交到服务器上的数据。
        /// </summary>
        /// <param name="entity" type="Rafy.data.Entity">要序列化的实体对象。</param>
        /// <returns type="Object">返回存放实体数据的纯 json 对象。</returns>

        var me = this;

        //注意，单个实体的数据，依然是以 EntityList 的方式提交。
        //这样不但统一了数据的格式，而且还简单用实体列表的集合来分辨当前实体的状态（IsNew、IsDeleted）。
        //添加属性 _isEntity 用于分辨二者。
        var dto = { _isEntity: 1 };

        if (entity.isNew()) {
            dto.c = [entity];
            me._getPersistArray(dto, 'c');
        }
        else if (entity.isSelfDirty()) {
            dto.u = [entity];
            me._getPersistArray(dto, 'u');
        }
        else if (entity.isDirty()) {
            dto.uc = [entity];
            me._getPersistArray(dto, 'uc');
        }

        return dto;
    },
    _serializeStore: function (store) {
        /// <summary>
        /// 序列化指定的数据集。
        /// </summary>
        /// <param name="store">要序列化的数据集。</param>
        /// <returns type="Object">存放数据的对象。</returns>

        var dto = null;
        if (store instanceof Ext.data.TreeStore) {
            dto = this._serializeStore_TreeStore(store);
        }
        else {
            dto = this._serializeStore_Store(store);
        }

        return dto;
    },
    _serailizeChildrenRecur: function (entity, dto) {
        /// <summary>
        /// 把某个实体 entity 中已经加载的所有聚合子实体都序列化到 dto 中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>

        var me = this;

        //entity.associations 中存储了 entity 所有的关联配置 AssociationConfig。
        Rafy.each(entity.associations, function (a) {
            if (a.type == 'hasMany') {
                var associatedStore = entity[a.storeName];
                if (associatedStore && associatedStore.getCount() > 0) {
                    var children = me._serializeStore(associatedStore, true);
                    dto[a.name] = children;
                }
            }
        });
    },

    //-------------------------------------  Common -------------------------------------
    _getPersistData: function (entity, deleted) {
        /// <summary>
        /// 获取某个实体序列化后的数据
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="deleted">该实体是否已经被删除</param>
        /// <returns type=""></returns>

        var dto = {};

        //如果该对象是被删除了，则只需要传输 Id 即可。
        if (deleted) {
            dto[Rafy._IdPropertyName] = entity.getId();
        }
        else {
            //属性需要可以保存，并且是大写
            var fields = Rafy.data.EntityRepository.getPersistFields(entity);
            Rafy.each(fields, function (f) { dto[f.name] = entity.get(f.name); });

            if (this._withChildren) {
                this._serailizeChildrenRecur(entity, dto);
            }
        }

        return dto;
    },
    _getPersistArray: function (dto, property) {
        /// <summary>
        /// 把 dto 中指定名称 property 的一个实体集合转换为数据的集合。
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="property"></param>
        var me = this;

        var raw = dto[property];
        if (!raw || raw.length == 0) {
            delete dto[property];
            return;
        }

        var deleted = property == 'd';

        var list = [];
        Rafy.each(raw, function (item) {
            list.push(me._getPersistData(item, deleted));
        });
        dto[property] = list;
    },

    //-------------------------------------  Store -------------------------------------
    _serializeStore_Store: function (store) {
        var data = {
            c: store.getNewRecords(),
            u: store.getUpdatedRecords(),
            d: Ext.Array.filter(store.getRemovedRecords(), function (i) { return !i.isNew(); }),
            //本身未改变，组合子发生改变的实体，放到 uc 集合中提交。
            uc: store.data.filterBy(function (item) { return !item.isSelfDirty() && item.isDirty(); }).items
        };

        this._getPersistArray(data, 'c');//toCreate
        this._getPersistArray(data, 'u');//toUpdate
        this._getPersistArray(data, 'd');//toDelete
        this._getPersistArray(data, 'uc');//unchanged

        return data;
    },

    //-------------------------------------  TreeStore -------------------------------------
    _serializeStore_TreeStore: function (treeStore) {
        var dto = { d: Ext.Array.filter(treeStore.getRemovedRecords(), function (i) { return !i.isNew(); }) };
        this._getPersistArray(dto, 'd');

        //为了保证树型对象间的父子、前后关系，这里需要把客户端的整个树型列表上传到服务器端。
        var rootNode = treeStore.getRootNode();
        dto.roots = this._convertNodeRecur(rootNode).TreeChildren;

        return dto;
    },
    _convertNodeRecur: function (node) {
        var item = this._getPersistData(node);

        if (node.isNew()) {
            item.isNew = 1;
        }

        if (!node.isLeaf()) {
            item.TreeChildren = [];
            for (var i = 0; i < node.childNodes.length; i++) {
                var child = this._convertNodeRecur(node.childNodes[i]);
                item.TreeChildren.push(child);
            }
        }

        return item;
    }
    //TreeStore 差异保存方案，暂留。
    //    ,_convertTreeCreateData: function (entityList, property, option) {
    //        var me = this;
    //        //树结点时，要处理树型结点之间的关系
    //        var raw = entityList[property];
    //        if (!raw || raw.length == 0) {
    //            delete entityList[property];
    //            return;
    //        }
    //        //roots 中只传输根对象
    //        var roots = [];
    //        //当前已经处理完的元素列表
    //        var added = [];
    //        //当前还没有处理的元素列表
    //        var toAdd = Ext.Array.clone(raw);
    //        Rafy.each(raw, function (item) {
    //            var isParent = item.parentNode.isRoot();
    //            if (!isParent) {
    //                var parent = Rafy.findFirst(raw, function (p) { return p == item.parentNode; });
    //                isParent = parent == null;
    //            }
    //            if (isParent) {
    //                var data = me._getPersistData(item);
    //                data._rawNode = item;
    //                roots.push(data);
    //                added.push(data);
    //                Ext.Array.remove(toAdd, item);
    //            }
    //        });
    //        var i = 1;
    //        while (toAdd.length > 0) {
    //            if (i++ > 1000) { Rafy.notSupport("死循环异常……"); }
    //            Rafy.each(toAdd, function (item) {
    //                var data = me._getPersistData(item);
    //                data._rawNode = item;
    //                var parent = Rafy.findFirst(added, function (p) { return p._rawNode == item.parentNode; });
    //                if (parent) {
    //                    if (!parent.TreeChildren) parent.TreeChildren = [];
    //                    parent.TreeChildren.push(data);
    //                    added.push(data);
    //                    Ext.Array.remove(toAdd, item);
    //                    return false;
    //                }
    //            });
    //        }
    //        Rafy.each(added, function (i) { delete i._rawNode; });
    //        entityList[property] = roots;
    //    }
});
