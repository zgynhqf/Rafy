/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合SQL的简单API
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OEA.MetaModel;
using Linq = System.Linq.Expressions;

namespace OEA.Library
{
    /// <summary>
    /// 聚合SQL的简单API。
    /// “Facade”
    /// </summary>
    public class AggregateSQL
    {
        #region 使用方法

        //var loadOptions = AggregateSQL.Instance
        //    .Begin<PBS>().LoadChildren(pbs => pbs.PBSPropertys)
        //    .Continue<PBSProperty>().LoadChildren(p => p.PBSPropertyOptionalValues)
        //    .Continue<PBSPropertyOptionalValue>().LoadChildren(p => p.PBSPropertyOptionalValueFilters);

        //var sql = AggregateSQL.Instance.GenerateQuerySQL(loadOptions, Guid.NewGuid());
        //var sql2 = AggregateSQL.Instance.GenerateQuerySQL(loadOptions, "PBS.ProjectId = '82E799D0-6CC0-4EC6-BEAA-A088D7462868'"); 

        #endregion

        public static readonly AggregateSQL Instance = new AggregateSQL();

        private AggregateSQL() { }

        #region 更简单的API

        public void LoadEntities<TEntity>(EntityList list, Action<PropertySelector<TEntity>> loader, int parentId)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            var sql = GenerateQuerySQL(loadOptions, parentId);
            LoadEntities(list, sql, loadOptions);
        }

        public void LoadEntities<TEntity>(EntityList list, Action<PropertySelector<TEntity>> loader, string whereCondition = null, string joinCondition = null)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            var sql = GenerateQuerySQL(loadOptions, whereCondition, joinCondition);
            LoadEntities(list, sql, loadOptions);
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// 
        /// 此方法使用父对象的Id作为查询条件。
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public string GenerateQuerySQL<TEntity>(Action<PropertySelector<TEntity>> loader, int parentId)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            return GenerateQuerySQL(loadOptions, parentId);
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <param name="whereCondition">
        /// 简单的过滤条件，如：
        /// PBS.PBSTypeId = '...'
        /// </param>
        /// <returns></returns>
        public string GenerateQuerySQL<TEntity>(Action<PropertySelector<TEntity>> loader, string whereCondition = null, string joinCondition = null)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            return GenerateQuerySQL(loadOptions, whereCondition, joinCondition);
        }

        #endregion

        #region 基本的API

        /// <summary>
        /// 开始为TEntity进行加载。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public PropertySelector<TEntity> BeginLoadOptions<TEntity>()
            where TEntity : Entity
        {
            return new PropertySelector<TEntity>(new AggregateDescriptor());
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// 
        /// 此方法使用父对象的Id作为查询条件。
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public string GenerateQuerySQL(LoadOptionSelector loadOptions, int parentId)
        {
            var aggregateSQL = new AggregateSQLGenerator(loadOptions.InnerDescriptor, null);
            var result = aggregateSQL.Generate();
            result = string.Format(result, parentId);
            return result;
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <param name="whereCondition">
        /// 简单的过滤条件，如：
        /// PBS.PBSTypeId = '...'
        /// </param>
        /// <returns></returns>
        public string GenerateQuerySQL(LoadOptionSelector loadOptions, string whereCondition = null, string joinCondition = null)
        {
            var aggregateSQL = new AggregateSQLGenerator(loadOptions.InnerDescriptor, whereCondition, joinCondition);
            var result = aggregateSQL.Generate();
            return result;
        }

        /// <summary>
        /// 通过聚合SQL加载整个聚合对象列表。
        /// </summary>
        /// <param name="sql">聚合SQL</param>
        /// <param name="loadOptions">聚合加载选项</param>
        /// <returns></returns>
        public void LoadEntities(EntityList list, string sql, LoadOptionSelector loadOptions)
        {
            var loader = new AggregateEntityLoader(loadOptions.InnerDescriptor);
            loader.Query(list, sql);
        }

        #endregion
    }

    /// <summary>
    /// 存储了加载的项
    /// </summary>
    public abstract class LoadOptionSelector
    {
        internal LoadOptionSelector(AggregateDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        private AggregateDescriptor _descriptor;

        internal AggregateDescriptor InnerDescriptor
        {
            get
            {
                return _descriptor;
            }
        }
    }

    /// <summary>
    /// 属性选择器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PropertySelector<TEntity> : LoadOptionSelector
        where TEntity : Entity
    {
        internal PropertySelector(AggregateDescriptor descriptor) : base(descriptor) { }

        /// <summary>
        /// 需要同时加载外键
        /// </summary>
        /// <typeparam name="TFKEntity"></typeparam>
        /// <param name="fkEntityExp">
        /// 需要加载的外键实体属性表达式
        /// </param>
        /// <returns></returns>
        public PropertySelector<TFKEntity> LoadFK<TFKEntity>(Expression<Func<TEntity, TFKEntity>> fkEntityExp)
            where TFKEntity : Entity
        {
            var entityPropertyName = GetPropertyName(fkEntityExp);
            var propertyName = entityPropertyName + "Id";

            var entityInfo = CommonModel.Entities.Get(typeof(TEntity));
            var propertyInfo = entityInfo.EntityProperties.FirstOrDefault(p => p.Name == propertyName);

            //构造一个临时代理方法，实现：TEntity.EntityProperty = TFKEntity
            var pE = Linq.Expression.Parameter(typeof(TEntity), "e");
            var pEFK = Linq.Expression.Parameter(typeof(TFKEntity), "efk");
            var propertyExp = Linq.Expression.Property(pE, entityPropertyName);
            var body = Linq.Expression.Assign(propertyExp, pEFK);
            var result = Linq.Expression.Lambda<Action<TEntity, TFKEntity>>(body, pE, pEFK);
            var fkSetter = result.Compile();

            var option = new LoadOptionItem(propertyInfo, (e, eFK) => fkSetter(e as TEntity, eFK as TFKEntity));

            //避免循环
            if (this.InnerDescriptor.Items.Any(i => i.OwnerType == option.PropertyEntityType))
            {
                throw new InvalidOperationException("有循环的实体设置。");
            }

            this.InnerDescriptor.AddItem(option);

            return new PropertySelector<TFKEntity>(this.InnerDescriptor);
        }

        /// <summary>
        /// 需要同时加载孩子
        /// </summary>
        /// <typeparam name="TChildren"></typeparam>
        /// <param name="propExp">
        /// 需要加载的孩子属性表达式
        /// </param>
        /// <returns></returns>
        public ChildrenSelector LoadChildren<TChildren>(Expression<Func<TEntity, TChildren>> propExp)
            where TChildren : EntityList
        {
            var propertyName = GetPropertyName(propExp);
            var entityInfo = CommonModel.Entities.Get(typeof(TEntity));
            var propertyInfo = entityInfo.ChildrenProperties.FirstOrDefault(p => p.Name == propertyName);

            this.InnerDescriptor.AddItem(new LoadOptionItem(propertyInfo));

            return new ChildrenSelector(this.InnerDescriptor);
        }

        private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> propExp)
        {
            var member = propExp.Body as MemberExpression;
            var property = member.Member as PropertyInfo;
            if (property == null) throw new ArgumentNullException("property");
            var propertyName = property.Name;

            return propertyName;
        }
    }

    /// <summary>
    /// 孩子选择器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ChildrenSelector : LoadOptionSelector
    {
        internal ChildrenSelector(AggregateDescriptor descriptor) : base(descriptor) { }

        public OrderByLoadOption<TEntity> Order<TEntity>()
            where TEntity : Entity
        {
            return new OrderByLoadOption<TEntity>(this.InnerDescriptor);
        }

        /// <summary>
        /// 把孩子集合转换为实体对象，需要继续加载它的子对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public PropertySelector<TEntity> Continue<TEntity>()
            where TEntity : Entity
        {
            return new PropertySelector<TEntity>(this.InnerDescriptor);
        }
    }

    public class OrderByLoadOption<TEntity> : LoadOptionSelector
        where TEntity : Entity
    {
        internal OrderByLoadOption(AggregateDescriptor descriptor) : base(descriptor) { }

        public PropertySelector<TEntity> By<TKey>(Func<TEntity, TKey> keySelector)
        {
            this.InnerDescriptor.Items.Last.Value
                .OrderBy = e => keySelector(e as TEntity);

            return new PropertySelector<TEntity>(this.InnerDescriptor);
        }
    }
}