﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web;
using NewLife.Cube;

// 有关程序集的常规信息通过下列特性集
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("魔方平台")]
[assembly: AssemblyDescription("Mvc权限管理后台，支持模版视图重载覆盖")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("NewLife.Cube")]
[assembly: AssemblyCompany("新生命开发团队")]
[assembly: AssemblyCopyright("©2002-2020 NewLife")]
[assembly: AssemblyTrademark("四叶草")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的某个类型，
// 请针对该类型将 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("f3362f22-bdbe-4345-9436-4ed577113c3c")]

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]

// 程序集的版本信息由下列四个值组成:
//
//      主版本
//      次版本
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“修订号”和“内部版本号”的默认值，
// 方法是按如下所示使用“*”:
[assembly: AssemblyVersion("3.4.*")]
[assembly: AssemblyFileVersion("3.4.2020.0525")]

/*
 * v3.4.2020.0519   支持JWT
 * 
 * v3.3.2020.0308   支持ECharts图表，支持钉钉登录，新增页面数据分享
 * 
 * v3.2.2020.0204   支持数据权限、用户部门地区选择、用户分享令牌授权
 * 
 * v3.1.2020.0115   X组件内部目录统一使用BasePath，支持命令行参数与环境变量，便于函数计算
 * 
 * v3.0.2019.1212   独立发布netcore版本魔方，并作为主线维护
 * 
 * v2.9.2019.1110   优化单点登录和Csv导出，界面调整
 * 
 * v2.8.2019.0623   增加Csv导出
 * 
 * v2.8.2019.0602   支持备份与恢复数据
 * 
 * v2.7.2019.0406   拆分只读实体控制器，用于只读数据展示
 * 
 * v2.6.2019.0221   列表页尾部支持增加汇总统计行
 * 
 * v2.5.2018.1031   优化Excel导出为Csv导出，改善大数据量导出性能
 * 
 * v2.4.2018.0630   OAuthServer支持回调地址和密钥验证
 * 
 * v2.3.2018.0403   支持数据权限，控制器重载ValidPermission
 * 
 * v2.2.2018.0326   重构权限体系，支持多角色
 * 
 * v2.1.2018.0211   集成OAuth客户端服务端，支持QQ、百度、GitHub等
 * 
 * v2.0.2017.1126   借助Ajax支持高级操作，如：删除选中、批量启用禁用等
 * 
 * v1.3.2016.1206   增加文件管理，增强数据字段映射
 * 
 * v1.2.2016.0325   自动检查并下载魔方资源
 * 
 * v1.1.2016.0301   魔方与XCode相互配合，趋于完善，完成了CMX和其它多个项目
 * 
 * v1.0.2015.0511   完善权限体系，支持控制器自定义菜单和权限子项
 * 
 * v1.0.2015.0403   建立魔方
 */
