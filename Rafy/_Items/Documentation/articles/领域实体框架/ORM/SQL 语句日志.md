使用数据访问接口 `IDbAccesser `访问数据库时，系统可以记录所有运行过程中执行的 Sql 语句及相关日志。这对于开发人员做系统调试、性能优化提供了较好的帮助。  
ORM 框架中所有的数据访问，都是直接或间接地通过 `IDbAccesser `类型来完成的，所以本机制可以显示框架与数据库的所有交互。  

##使用文件记录
要打开这个功能，需要到应用程序的配置文件中添加名为 Rafy.FileLogger.SqlTraceFileName 的应用程序配置项，该配置项的值是用于记录日志的文件地址，示例代码如下：

```cs
<configuration>
    <appSettings>
        <!--如果是监听应用程序所有的 SQL 语句，请打开以下配置-->
        <add key="Rafy.FileLogger.SqlTraceFileName" value="D:\SQLTraceLog.txt"/>
    </appSettings>
</configuration>
```

打开此配置项后，文件即会被自动添加，其中日志的格式如下：

```cs
2013/10/2 16:16:43
Database:  Test_RafyUnitTest
SELECT [Book].[Id],[Book].[Author],[Book].[BookCategoryId],[Book].[BookLocId],[Book].[Code],[Book].[Name],[Book].[Price],[Book].[Publisher]
FROM [Book]
ORDER BY [Book].[Id] ASC


2013/10/2 16:16:43
Database:  Test_RafyUnitTest
INSERT INTO [Users] ([Age],[UserName],[TasksTime],[TestUserExt_UserCode]) VALUES (@p0,@p1,@p2,@p3)
Parameters:10,"QueryExt_User",0,"DefaultUserCode"


2013/10/2 16:16:43
Database:  Test_RafyUnitTest
SELECT @@IDENTITY;


2013/10/2 16:16:43
Database:  Test_RafyUnitTest
SELECT [Users].[Id],[Users].[Age],[Users].[UserName],[Users].[TasksTime],[Users].[TestUserExt_UserCode]
FROM [Users]
WHERE [Users].[Age] = @p0
ORDER BY [Users].[Id] ASC
Parameters:10
```


##高级
如果不想使用文件记录日志，或者想自定义 Sql 日志格式，请使用
方法替换为自己的日志实体类型。
