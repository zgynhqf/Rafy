/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111207
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111207
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Threading;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 实体列表版本号的仓库。
    /// （目前使用领域模型中的ScopeVersion进行存储。）
    /// </summary>
    internal class EntityListVersionRepository : IEntityListVersionRepository
    {
        /// <summary>
        /// 获取指定的列表范围，如果不存储，则构造一个新的。
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public EntityListVersion GetOrNew(Type region, Type scopeClass, string scopeId)
        {
            var result = Get(region, scopeClass, scopeId);
            if (result == null)
            {
                result = Add(region, scopeClass, scopeId);
            }
            return result;
        }

        /// <summary>
        /// 添加一个指定的范围版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public EntityListVersion Add(Type region, Type scopeClass, string scopeId)
        {
            var item = new ScopeVersion();

            item.ClassRegion = region.FullName;
            item.ScopeClass = scopeClass == null ? string.Empty : scopeClass.FullName;
            item.ScopeId = scopeId ?? string.Empty;
            item = RF.ResolveInstance<ScopeVersionRepository>().Save(item) as ScopeVersion;

            _versionListCache.Expire();

            return new EntityListVersion()
            {
                ClassRegion = region,
                ScopeClass = scopeClass,
                ScopeId = scopeId,
                Value = item.Value
            };
        }

        /// <summary>
        /// 获取一个指定的范围版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public EntityListVersion Get(Type region, Type scopeClass, string scopeId)
        {
            var item = GetDbEntity(region, scopeClass, scopeId);

            if (item != null)
            {
                return new EntityListVersion()
                {
                    ClassRegion = region,
                    ScopeClass = scopeClass,
                    ScopeId = scopeId,
                    Value = item.Value
                };
            }

            return null;
        }

        /// <summary>
        /// 清空所有版本号，所有缓存都会过期。
        /// </summary>
        public void Clear()
        {
            var cacheRepo = RF.Find<ScopeVersion>();

            var allList = cacheRepo.GetAll();
            allList.Clear();

            cacheRepo.Save(allList);
        }

        #region 更新缓存

        /// <summary>
        /// 通知整个表被改变
        /// </summary>
        /// <param name="region"></param>
        public void UpdateVersion(Type region)
        {
            UpdateVersion(region, null, null);
        }

        /// <summary>
        /// 更新指定的范围的版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        public void UpdateVersion(Type region, Type scopeClass, string scopeId)
        {
            //导入Excel后，在客户端无法执行，huqf提出建议，应该可以在客户端执行，所有注释掉下面的一句
            //if (!RafyEnvironment.Location.IsOnServer()) throw new InvalidOperationException("!CslaEnvironment.IsOnServer must be false.");

            var batch = BatchSaveOperationScope.GetWholeScope();
            if (batch != null)
            {
                batch.AddVersionUpdateItem(region, scopeClass, scopeId);
            }
            else
            {
                ////此操作使用异步执行，原因：
                ////1 操作比较耗时
                ////2 由于最后的更新需要跨库进行更新，但是目前并不支持分布式事务。
                //AsyncHelper.InvokeSafe(() =>
                //{
                var item = GetDbEntity(region, scopeClass, scopeId);
                if (item != null)
                {
                    (item as IEntityWithStatus).MarkModifiedIfSaved();
                    RF.Save(item);

                    _versionListCache.Expire();
                }
                //});
            }
        }

        /// <summary>
        /// 如果要进行批量的更新，调用此方法。
        /// </summary>
        /// <returns></returns>
        public IDisposable BatchSaveScope()
        {
            return new BatchSaveOperationScope();
        }

        private class BatchSaveOperationScope : AppContextScope<BatchSaveOperationScope>
        {
            private List<SaveItem> _items;

            protected override void EnterWholeScope()
            {
                //do nothing.
            }

            protected override void ExitWholeScope()
            {
                this.DoUpdateVersions();
                _items = null;
            }

            public void AddVersionUpdateItem(Type region, Type scopeClass, string scopeId)
            {
                if (_items == null) _items = new List<SaveItem>();

                //防止有重复项。
                for (int i = 0, c = this._items.Count; i < c; i++)
                {
                    var item = this._items[i];
                    if (item.Region == region && item.ScopeClass == scopeClass && item.ScopeId == scopeId) { return; }
                }

                this._items.Add(new SaveItem()
                {
                    Region = region,
                    ScopeClass = scopeClass,
                    ScopeId = scopeId
                });
            }

            private void DoUpdateVersions()
            {
                if (_items != null)
                {
                    //此操作使用异步执行，原因：
                    //1 操作比较耗时
                    //2 由于最后的更新需要跨库进行更新，但是目前并不支持分布式事务。
                    //ThreadHelper.SafeInvoke(() =>
                    //{
                    for (int i = 0, c = _items.Count; i < c; i++)
                    {
                        var saveItem = _items[i];

                        var item = GetDbEntity(saveItem.Region, saveItem.ScopeClass, saveItem.ScopeId);
                        if (item != null)
                        {
                            (item as IEntityWithStatus).MarkModifiedIfSaved();
                            RF.Save(item);
                        }
                    }

                    _versionListCache.Expire();
                    //});
                }
            }

            private struct SaveItem
            {
                public Type Region;
                public Type ScopeClass;
                public string ScopeId;
            }
        }

        #endregion

        #region 数据层实现

        private static ScopeVersion GetDbEntity(Type region, Type scopeClass, string scopeId)
        {
            var classRegion = region.FullName;
            string scopeClassName = scopeClass == null ? string.Empty : scopeClass.FullName;
            scopeId = scopeId ?? string.Empty;

            return _versionListCache.Find(classRegion, scopeClassName, scopeId);
        }

        private static ScopeVersionListCache _versionListCache = new ScopeVersionListCache();

        private class ScopeVersionListCache
        {
            /// <summary>
            /// 类中以字典的形式存储，以提供高速查找。
            /// </summary>
            private IDictionary<string, List<ScopeVersion>> _regionValues;

            /// <summary>
            /// 当前版本号缓存是何时在服务器上获取的
            /// </summary>
            private DateTime _serverTime;

            /// <summary>
            /// 本地时间，用于校验版本号缓存的过期时间。
            /// </summary>
            private DateTime _localTime;

            /// <summary>
            /// 在缓存的版本号中找到对应主键的数据。
            /// </summary>
            /// <param name="classRegion"></param>
            /// <param name="scopeClassName"></param>
            /// <param name="scopeId"></param>
            /// <returns></returns>
            public ScopeVersion Find(string classRegion, string scopeClassName, string scopeId)
            {
                //检测是否过期
                if (this._regionValues == null || this.IsExpired())
                {
                    lock (this)
                    {
                        if (this._regionValues == null)
                        {
                            var allValues = RF.ResolveInstance<ScopeVersionRepository>().GetAll() as ScopeVersionList;
                            this.SetAllValues(allValues);
                        }
                        //如果过期，则差异更新。
                        else if (this.IsExpired())
                        {
                            var values = RF.ResolveInstance<ScopeVersionRepository>().GetList(_serverTime);
                            this.UpdateByDifference(values);
                        }
                    }
                }

                List<ScopeVersion> regionValues = null;
                if (this._regionValues.TryGetValue(classRegion, out regionValues))
                {
                    return regionValues.FirstOrDefault(v => v.ScopeClass == scopeClassName && v.ScopeId == scopeId);
                }

                return null;
            }

            /// <summary>
            /// 为缓存设置所有的数据
            /// </summary>
            /// <param name="values"></param>
            private void SetAllValues(ScopeVersionList values)
            {
                UpdateTime(values);

                var dic = values.AsEnumerable().Cast<ScopeVersion>()
                    .GroupBy(v => v.ClassRegion)
                    .ToDictionary(g => g.Key, g => g.ToList());

                this._regionValues = new SortedDictionary<string, List<ScopeVersion>>(dic);
            }

            /// <summary>
            /// 更新两个时间戳
            /// </summary>
            /// <param name="values"></param>
            private void UpdateTime(ScopeVersionList values)
            {
                this._localTime = DateTime.Now;
                this._serverTime = values.ServerTime;
            }

            /// <summary>
            /// 为缓存设置更新指定的数据，先清空再添加
            /// </summary>
            /// <param name="values"></param>
            private void UpdateByDifference(ScopeVersionList values)
            {
                UpdateTime(values);

                if (values.Count > 0)
                {
                    for (int i = 0, c = values.Count; i < c; i++)
                    {
                        var item = values[i] as ScopeVersion;

                        string classRegion = item.ClassRegion;

                        //在字典中找到对应ClassRegion的列表
                        List<ScopeVersion> oldVersions = null;
                        if (!this._regionValues.TryGetValue(classRegion, out oldVersions))
                        {
                            oldVersions = new List<ScopeVersion>();
                            this._regionValues.Add(classRegion, oldVersions);
                        }

                        //把Value的值更新到oldVersions列表中。
                        var oldItem = oldVersions.FirstOrDefault(v => v.ScopeClass == item.ScopeClass && v.ScopeId == item.ScopeId);
                        if (oldItem != null)
                        {
                            oldItem.Value = item.Value;
                        }
                        else
                        {
                            oldVersions.Add(item);
                        }
                    }
                }
            }

            /// <summary>
            /// 强制让版本号直接过期
            /// </summary>
            public void Expire()
            {
                this._regionValues = null;
                this._localTime = default(DateTime);
            }

            /// <summary>
            /// 检查是否过期。
            /// </summary>
            /// <returns></returns>
            private bool IsExpired()
            {
                return (DateTime.Now - this._localTime).TotalSeconds > DiskCachingPlugin.CacheExpiredSeconds;
            }
        }

        #endregion
    }
}