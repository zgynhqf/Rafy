在应用开发过程中，有 80% 的场景下，开发者所需要的实体查询，查询条件中其实都是一些简单的属性匹配，又或是一些属性匹配的简单组合。Rafy 为这样的场景提供了更为方便使用的 API：`CommonQueryCriteria`。  

##属性匹配
在查询时，当需要使用一个或几个属性的限定匹配来进行查询时，我们可以通过 CommonQueryCriteria 
来使用以下方法进行快速查询。例如，以下查询实现了通过用户的编码的精确匹配来查询唯一指定的用户：

```cs
public User GetByCode(string code)
{
    return this.GetFirstBy(new CommonQueryCriteria
    {
        new PropertyMatch(User.CodeProperty, PropertyOperator.Equal, code)
    });
}
```

例如，以下查询实现同时通过用户名称的模糊匹配、年龄的精确匹配来查询一组用户（由于 Age 未指定 PropertyOperator，所以使用的是 Equal）：

```cs
public UserList GetByNameAge(string name, int age)
{
    return this.GetBy(new CommonQueryCriteria
    {
        new PropertyMatch(User.NameProperty, PropertyOperator.Contains, name),
        new PropertyMatch(User.AgeProperty, age)
    });
}
```

上述查询默认使用 And 进行多条件的连接。如果需要修改，可以通过 CommonQueryCriteria
的构造器传入或属性进行设置。PropertyOperator 表示属性匹配的方式，可用的操作有：

 - Equal
 - NotEqual

 - Greater

 - GreaterEqual

 - Less

 - LessEqual

 - Like

 - NotLike

 - Contains

 - NotContains

 - StartsWith

 - NotStartsWith

 - EndsWith

 - NotEndsWith

 - In

 - NotIn
   ​     
为了方便开发者使用 CommonQueryCriteria，RafySDK 提供了代码段 RafyQuery_Common 来生成上述代码。                

##使用多个属性匹配组进行查询
上面是比较简单的查询，只是对单个属性或使用 And、Or 连接的几个条件进行匹配。我们还可以通过属性匹配组来实现相对复杂的查询。  
一个 CommonQueryCriteria 中可以通过 And、Or 连接多个属性匹配组，而每一个属性匹配组也可以通过 And、Or 连接多个具体的属性匹配条件。
下面的代码演示了如何使用（'Name contains name' And 'Age equal age' Or 'Code equal code'）的条件进行查询：

```cs
this.GetBy(new CommonQueryCriteria(BinaryOperator.Or)
{
    new PropertyMatchGroup
    {
        new PropertyMatch(TestUser.NameProperty, PropertyOperator.Contains, name)
        new PropertyMatch(TestUser.AgeProperty, age)
    },
    new PropertyMatchGroup
    {
        new PropertyMatch(TestUser.CodeProperty, code)
    }
});
```


##相对于 Linq 查询的优势
使用 CommonQueryCriteria 进行查询时，相对于[Linq 查询](../../../领域实体框架\实体仓库\编写查询\Linq 查询.html)而言，有以下的优势：
 - 更加方便、简单  
  仓库类型上已经提供了参数是`CommonQueryCriteria`的公有查询方法，开发可以直接使用这些方法进行查询，没有必要再封装一个相应的公有方法。
  例如，上面的示例中，也可以不封装 GetByCode 方法，而是由仓库的调用者直接使用 GetBy(CommonQueryCriteria) 方法。
 - 性能更好  
  使用 Linq 查询时，编译器会使用反射生成表达式树，然后 Rafy 框架才会解析这棵树，生成最终的 Sql 树。但是使用`CommonQueryCriteria`通用查询时，Rafy 框架会直接将`CommonQueryCriteria`中的条件生成对应的 Sql 树，这就节省了表达式树的生成和解析的环节，提升了性能。
