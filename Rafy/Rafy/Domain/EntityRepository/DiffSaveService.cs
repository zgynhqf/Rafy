/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100505
 * 说明：差异保存的实现
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100501
 * 抽取出DataClear类。 胡庆访 20100505
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Utils;

namespace Rafy.Domain
{
    /// <summary>
    /// 差异保存类
    /// </summary>
    [Serializable]
    [Contract, ContractImpl]
    internal class DiffSaveService : Service
    {
        [ServiceOutput]
        public object NewId { get; set; }

        private Entity _diffEntity;

        /// <summary>
        /// 创建一个数据的“清理器”
        /// </summary>
        /// <returns></returns>
        protected IDataClear CreateClear()
        {
            return new CleanDataClear();
        }

        protected override void Execute()
        {
            bool isNew = _diffEntity.IsNew;

            _diffEntity.GetRepository().Save(_diffEntity);
            this.NewId = _diffEntity.Id;

            //如果是新加的数据，则不需要传回客户端了。
            if (isNew)
            {
                _diffEntity = null;
            }
        }

        public static DiffSaveService Execute(Entity entity)
        {
            //清除数据
            var svc = ServiceFactory.Create<DiffSaveService>();
            var clear = svc.CreateClear();

            if (entity.IsNew)
            {
                svc._diffEntity = entity;
            }
            else
            {
                //清除数据
                svc._diffEntity = ObjectCloner.Clone(entity);
                clear.ClearData(svc._diffEntity);
            }

            //保存数据
            svc.Invoke();

            //整合新旧数据
            clear.MakeSaved(entity);

            return svc;
        }

        //改为异步后，可以使用这个：
        //public static DiffSaveService Execute(Entity businessBase, IProgressReporter p = null)
        //{
        //    if (p == null)
        //    {
        //        p = new EmptyProgressReporter();
        //    }

        //    //清除数据
        //    var cmd = new DiffSaveService();

        //    if (businessBase.IsNew)
        //    {
        //        cmd._diffEntity = businessBase;

        //        //保存数据
        //        cmd = DataPortal.Execute<TCommand>(cmd);

        //        //这里可能会引起界面的跨线程异常
        //        try
        //        {
        //            businessBase.MarkOld();
        //        }
        //        catch (InvalidOperationException) { }
        //    }
        //    else
        //    {
        //        p.Report(10, "开始保存...");
        //        var clear = cmd.CreateClear();

        //        p.Report(15, "开始复制要传送到服务端的数据...");

        //        //清除数据
        //        cmd._diffEntity = (businessBase as ICloneable).Clone() as Entity;
        //        clear.ClearData(cmd._diffEntity);

        //        p.Report(30, "保存到服务器...");

        //        //保存数据
        //        cmd = DataPortal.Execute<TCommand>(cmd);

        //        p.Report(90, "保存完毕，正在执行清理...");

        //        //这里可能会引起界面的跨线程异常
        //        try
        //        {
        //            //整合新旧数据
        //            clear.MakeOld(businessBase);
        //        }
        //        catch (InvalidOperationException) { }

        //        p.Report(95, "完成！");
        //    }

        //    return cmd;
        //}
    }

    /// <summary>
    /// 数据清理器
    /// </summary>
    internal interface IDataClear
    {
        /// <summary>
        /// 删除不必要的对象，只留下需要保存的数据
        /// </summary>
        /// <param name="entity"></param>
        void ClearData(Entity entity);

        /// <summary>
        /// 设置对象树的状态为“已保存”。
        /// </summary>
        /// <param name="entity"></param>
        void MakeSaved(Entity entity);
    }

    /// <summary>
    /// 把未修改的数据都清除的清理器
    /// </summary>
    internal class CleanDataClear : IDataClear
    {
        public void ClearData(Entity diffEntity)
        {
            var entityInfo = CommonModel.Entities.Get(diffEntity.GetType());

            this.ClearDataCore(diffEntity, entityInfo);
        }

        public void MakeSaved(IList oldList)
        {
            if (oldList.Count > 0)
            {
                var entityInfo = CommonModel.Entities.Get(oldList[0].GetType());

                foreach (Entity entity in oldList)
                {
                    this.MakeSavedCore(entity, entityInfo);
                }
            }
        }

        public void MakeSaved(Entity oldEntity)
        {
            var entityInfo = CommonModel.Entities.Get(oldEntity.GetType());

            this.MakeSavedCore(oldEntity, entityInfo);
        }

        /// <summary>
        /// 删除不必要的对象，只留下需要保存的“脏”数据
        /// </summary>
        /// <param name="diffEntity">The difference entity.</param>
        /// <param name="entityInfo">The entity information.</param>
        protected virtual void ClearDataCore(Entity diffEntity, EntityMeta entityInfo)
        {
            foreach (var item in entityInfo.ChildrenProperties)
            {
                var mp = item.ManagedProperty;

                //如果是懒加载属性，并且没有加载数据时，不需要遍历此属性值
                if (!diffEntity.FieldExists(mp)) continue;
                var children = diffEntity.GetProperty(mp) as EntityList;
                if (children == null) continue;

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    if (!child.IsDirty)
                    {
                        children.Remove(child);
                        children.DeletedList.Remove(child);
                    }
                    else
                    {
                        this.ClearData(child);
                    }
                }
            }
        }

        /// <summary>
        /// 清除子对象集合
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <param name="entityInfo">The entity information.</param>
        protected virtual void MakeSavedCore(Entity oldEntity, EntityMeta entityInfo)
        {
            foreach (var item in entityInfo.ChildrenProperties)
            {
                var mp = item.ManagedProperty as IListProperty;

                //如果是懒加载属性，并且没有加载数据时，不需要遍历此属性值
                if (!oldEntity.FieldExists(mp)) continue;

                var children = oldEntity.GetProperty(mp) as EntityList;
                if (children == null) continue;

                //清除已删除数据
                children.CastTo<EntityList>().DeletedList.Clear();

                //所有子对象，都标记为已保存
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i] as Entity;
                    if (child.IsDirty || child.IsNew) MakeSaved(child);
                }
            }

            (oldEntity as IDirtyAware).MarkSaved();
        }
    }
}