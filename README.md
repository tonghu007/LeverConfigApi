# LeverConfigApi
一个可动态配置WebApi的Api配置系统

# 原理
使用Lua脚本作为配置api时的一些简单业务逻辑处理，包括提交参数的判断、sql的构建、返回数据的整合等。对于复杂的业务逻辑通过开发扩展插件实现，开发好后把对应的库已经类名配置到相应接口即可。配置界面是在网上找的一个网友朋友用Layui所写的一套后台管理框架。

# 架构
使用的是.netcore2.2，一个简单的三层结构，为了配置处理的方便，数据库部分使用的是自己用ado.net封装的库。配置接口的数据存放在SQLITE之中，业务数据库可以用mysql，postgresql、oracle、db2、sqlserver、sybase、sqlite,其中mysql是做过应用测试的。

# 优点
使用方便，配置接口快速，配置即可支持.netcore的权限验证

# 缺点
本应用是一时兴起所写，代码还有些凌乱，也没有做过多的测试，生产环境应用也有待验证。此外Lua脚本语言对于Array和Map都是Table类型，所以在数据传递转换中会比较麻烦，容易出问题，后面有时间准备使用javascript来代替

# 后续开发计划
1、用javascript替换掉lua
2、加入用户权限，使用户权限能控制到具体的接口
3、重构整个框架，搭建成可配置的微服务分布式框架
