Linq 查询是数据层查询中最简单易用的查询，一般情况下，都推荐使用这种查询。  

##代码示例

```cs
[RepositoryQuery]
public virtual WarehouseList GetByCode(string warehouseCode, string nameKeywords, PagingInfo pagingInfo)
{
    var q = this.CreateLinqQuery();

    //条件对比
    q = q.Where(e => e.Code == warehouseCode);
    if (!string.IsNullOrEmpty(nameKeywords))
    {
        q = q.Where(e => e.Name.Contains(nameKeywords));
    }

    //排序
    q = q.OrderByDescending(w => w.Name);

    return (WarehouseList)this.QueryData(q, pagingInfo);//以指定的分页信息 pagingInfo 分页
}
```

上面的代码使用 Linq 查询实现了一个较复杂的查询示例。Linq 查询中可使用属性对比，排序，分页等。  
PagingInfo 是 Rafy 中为分布设计的一个类型。详情见：[分页查询](分页查询.html)

##支持的操作符
Rafy 并不支持所有的 Linq 标准查询。可以使用的 Linq 标准查询方法如下：
| 方法名               |         所属类型          |                 备注 |
| ----------------- | :-------------------: | -----------------: |
| Where             | System.Linq.Queryable |      添加 Where 条件约束 |
| OrderBy           | System.Linq.Queryable |        按照指定属性正序排列。 |
| OrderByDescending | System.Linq.Queryable |        按照指定属性倒序排列。 |
| ThenBy            | System.Linq.Queryable |     再次，按照指定属性正序排列。 |
| ThenByDescending  | System.Linq.Queryable |     再次，按照指定属性倒序排列。 |
| Count             | System.Linq.Queryable | 直接返回 Linq 查询对应的行数。 |
而 where 方法中，对实体的属性进行条件对比时：
 -   支持以下双目操作符：=、!=、>、>=、<、<=。                        
 -   支持单目操作符：!。                        
 -   支持两个属性条件间的连接条件：&&、||。例如：

 ```cs
q = q.Where(e => e.Code == warehouseCode && e.Name.Contains(nameKeywords) || e.Id > 100);
 ```

 - 支持引用查询。即间接使用引用实体的属性来进行查询。例如：

 ```cs
q = q.Where(warehouse => warehouse.Administrator.Name == "admin");
 ```

以上代码将会过滤出所有管理员名称是 "admin" 的仓库。<br>
在生成 Sql 语句时，将会生成 INNER JOIN 语句，连接上这些被使用的引用实体对应的表。
 - 支持在属性对比条件中，使用以下方法对属性进行比较：

方法名：Contains  
所属类型：System.String  
备注：判断指定属性是否包含指定字符串  
示例：
```cs 
q = q.Where(e => e.Name.Contains(nameKeywords));
```
  

方法名：StartsWith  
所属类型：System.String  
备注：判断指定属性是否以指定字符串开头   
示例：

```cs 
q = q.Where(e => e.Name.StartsWith(nameKeywords));
```
方法名：EndsWith  
所属类型：System.String  
备注：判断指定属性是否以指定字符串结尾  
示例：
```cs 
q = q.Where(e => e.Name.EndsWith(nameKeywords));
```
方法名：IsNullOrEmpty  
所属类型：System.String  
备注：判断指定属性是否为空字符串  
示例：
```cs 
q = q.Where(e => !string.IsNullOrEmpty(e.Name));
```
方法名：Contains  
所属类型：System.Linq.Enumerable  
备注：判断指定属性的值是否在指定的列表中  
示例：
```cs 
int[] values = new int[] { 1, 2 };
q = q.Where(e => values.Contains(e.Id));
```
方法名：Contains  
所属类型：System.Collections.Generic.List<T>  
备注：判断指定属性的值是否在指定的列表中  
示例：
```cs 
List<int> values = new List<int> { 1, 2 };
q = q.Where(e => values.Contains(e.Id));
```
方法名：Any  
所属类型：Rafy.Domain.EntityList  
备注：用于判断某个实体的聚合子实体中是否存在一个实体满足某个条件，在使用前，需要使用 Cast<T> 方法把非泛型的 EntityList 转换为具体实体的集合类型 IEnumerable<T>  
示例：
```cs 
q = q.Where(book => book.ChapterList.Cast<Chapter>().Any(c => c.Name == chapterName));
q = q.Where(book => book.ChapterList.Concrete().Any(c => c.SectionList.Cast<Section>().Any(s => s.Name.Contains(sectionName))));
```
方法名：All   
所属类型：Rafy.Domain.EntityList  
备注：用于判断某个实体的聚合子实体中是否所有实体满足某个条件，在使用前，需要使用 Cast<T> 方法把非泛型的 EntityList 转换为具体实体的集合类型 IEnumerable<T>  
示例：
```cs 
q = q.Where(book => book.ChapterList.Cast<Chapter>().All(c => c.Name == chapterName));
```
##代码生成
框架为常用的 Linq 查询提供了代码段生成，使用 RafyQuery 即可生成相应代码段：

```cs
[RepositoryQuery]
public virtual $EntityListName$ $MethodName$($Parameters$)
{
    var q = this.CreateLinqQuery();
    q = q.Where(e => e.$WhereExpression$);$end$
    return ($EntityListName$)this.QueryData(q);
}
```

详见：
[代码段](../../../领域实体框架\其它\代码段.html)。
