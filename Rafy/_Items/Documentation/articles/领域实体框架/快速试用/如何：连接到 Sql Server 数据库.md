本章说明如何使 Rafy 连接到 Sql Server 数据库。如果您期望继续使用 SQLCE 完成本示例，可以跳过本章。  

##前提
请先完成示例：[创建第一个程序](创建第一个程序.html)后，再尝试本示例。

##如何：连接 Sql Server 数据库
**步骤**
1. 打开 ConsoleApp 项目中的 App.config 文件。
```xml
   把连接配置项：'<add name="JXC" connectionString="Data Source=Data\JXC.sdf" providerName="System.Data.SqlServerCe" />'
    替换为：<add name="JXC" connectionString="Data Source=.\SQLExpress;Initial Catalog=JXCDemo;User ID=UserName;Password=Password" providerName="System.Data.SqlClient"/>即可。
```
 注意，连接字符串要保证正确。支持 Sql Server 2005 以上版本。
2. 检查
  编译并运行解决方案。提示已经把数据存储到数据库中。
  这时，打开 SqlServer 的数据库管理界面，可以看到 JXCDemo 的数据库已经生成完成了。这个数据库中有一个 Warehouse 表，表中有三个字段，分别是 Id、Code、Name。
