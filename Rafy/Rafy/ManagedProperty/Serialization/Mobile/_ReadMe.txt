使用 NetDataContractSerializer 则需要调用双方都使用 .NET；这在 Web、SilverLight 环境下都不可用。
而 DataContractJsonSerializer、DataContractSerializer 在序列化和反序列化时，都需要知道类型信息。
所以本命名空间下的类型，以一种“适配器”的模式，MobileObjectFormatter 来解决了上述问题：
* 统一了传输的类型：SerializationInfoContainer。
* 使用 DataContractJsonSerializer 来序列化和反序列化实体。
* 在 MobileObject 基类中，实现了统一的序列化和反序列化的回调。

本实现，拷贝自 CSLA。