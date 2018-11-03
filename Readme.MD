## XProxy代理服务器
XProxy是一个代理服务器宿主环境，可用于寄宿多种`ProxyBase`服务，主要是`NATProxy`反向代理。  

2005~2008期间，第一代XProxy同时也是第一代网络库，主要支持NAT、加解密、反向代理、直接代理、间接代理等，以Http代理缓存以及数据拦截修改为主。  
2018年v2.0，采用更成熟稳定且具有千万级吞吐的第三代网络库，主要支持反向代理，转发MySql、Oracle、Redis等。  

## 使用说明
双击启动`XProxy.exe`，自动生成配置文件`Config\XProxy.config`，默认如下：
![Demo](Doc/Demo.png)

XProxy支持单服务同时寄宿多个代理服务，配置文件的Items下，每一行ProxyItem代表一个代理服务。  
以管理员运行`XProxy.exe`，可以选择把它安装成为Windows服务，并控制它的启动和停止。  

## 新生命开发团队
本应用作为[新生命开发团队](https://github.com/NewLifeX)2018年开源路线图的一部分，依赖库：
+ [X组件](https://github.com/NewLifeX/X)  
+ [NewLife.Net网络库](https://github.com/NewLifeX/NewLife.Net)  


## 新生命开源项目矩阵

| 项目 | 年份 | 状态 | .NET Core | 说明 |
| :------: | :------: | :------: | :------: | ------ |
| [NewLife.Core](https://github.com/NewLifeX/X) | 2002 | 维护中 | √ | 日志、网络、RPC、序列化、缓存、Windows服务、多线程 |
| [XCode](https://github.com/NewLifeX/X) | 2005 | 维护中 | √ | 数据中间件，MySQL、SQLite、SqlServer、Oracle |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 2010 | 维护中 | √ | Web魔方，权限基础框架，集成OAuth |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 2005 | 维护中 | √ | 网络库，千万级吞吐率，学习gRPC、Thrift |
| [XAgent](https://github.com/NewLifeX/XAgent) | 2008 | 重构中 | √ | 集群节点分布式计算平台，学习Hadoop |
| [Stardust](https://github.com/NewLifeX/Stardust) | 2018 | 设计中 | √ | 星尘，微服务平台，学习Dubbo |
| [XCoder](https://github.com/NewLifeX/XCoder) | 2006 | 维护中 | × | 码神工具，开发者必备 |
| [XProxy](https://github.com/NewLifeX/XProxy) | 2005 | 维护中 | √ | 产品级反向代理 |
| [XScript](https://github.com/NewLifeX/XScript) | 2010 | 维护中 | × | C#脚本引擎 |
| [XTemplate](https://github.com/NewLifeX/XTemplate) | 2008 | 维护中 | × | 模版引擎，学习TT |
| [NewLife.DNS](https://github.com/NewLifeX/NewLife.DNS) | 2011 | 维护中 | × | DNS代理服务器 |
| [NewLife.CMX](https://github.com/NewLifeX/NewLife.CMX) | 2013 | 维护中 | × | 内容管理系统 |
| [SmartOS](https://github.com/NewLifeX/SmartOS) | 2014 | 保密中 | C++11 | 嵌入式操作系统，面向ARM Cortex-M芯片架构 |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 2015 | 维护中 | × | Git管理系统 |
| [XLink](https://github.com/NewLifeX/XLink) | 2016 | 保密中 | √ | 物联网云平台 |
| [NoDb](https://github.com/NewLifeX/NoDb) | 2017 | 开发中 | √ | NoSQL数据库，百万级读写性能，学习Redis+MongoDB |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | 2017 | 维护中 | √ | Redis客户端，微秒级延迟，百亿级项目验证 |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | 2018 | 开发中 | √ | MySql驱动 |
| [NewLife.Cache](https://github.com/NewLifeX/NewLife.Cache) | 2018 | 维护中 | √ | 自定义缓存服务器 |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018 | 维护中 | √ | 支持Apache RocketMQ和阿里云消息队列 |
| NewLife.Kafka | 2019 | 设计中 | √ | Kafka组件 |
| ConfigServer | 2019 | 设计中 | √ | 分布式配置中心，学习Apollo |
| [X组件 .NET2.0](https://github.com/NewLifeX/X_NET20) | 2002 | 存档中 | .NET2.0 | 日志、网络、RPC、序列化、缓存、Windows服务、多线程 |
| [X组件 .NET4.0](https://github.com/NewLifeX/X_NET40) | 2002 | 存档中 | .NET4.0 | 日志、网络、RPC、序列化、缓存、Windows服务、多线程 |

QQ群：1600800  
博客：https://nnhy.cnblogs.com  
