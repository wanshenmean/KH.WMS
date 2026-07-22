export const architectureFiles = [
  'backend/架构设计/KH.WMS架构总览.md',
  'backend/架构设计/KH.WMS前端架构设计思路.md',
  'backend/架构设计/KH.WMS后端架构设计思路.md'
];

export const frontendFiles = [
  'backend/KH.WMS前端开发指引 V3.0.md',
  'backend/KH.WMS前端配置与启动指引.md',
  'backend/KH.WMS前端路由菜单与权限开发指引.md',
  'backend/KH.WMS前端请求封装与接口开发指引.md',
  'backend/KH.WMS前端组件体系与页面开发指引.md',
  'backend/KH.WMS主从表页面配置与开发实战指引.md',
  'backend/KH.WMS前端状态管理与公共工具指引.md',
  'backend/KH.WMS前端常用组件详细使用文档.md',
  'backend/KH.WMS前端E2E测试与质量检查指引.md'
];

export const backendTutorialFiles = [
  'backend/KH.WMS后端开发指引 V3.0.md',
  'backend/后端开发指引V3教程/00-这份文档怎么读.md',
  'backend/后端开发指引V3教程/01-KH.WMS后端整体地图.md',
  'backend/后端开发指引V3教程/02-后端基础配置与启动机制.md',
  'backend/后端开发指引V3教程/04-职责边界.md',
  'backend/后端开发指引V3教程/05-服务自动注册RegisteredService.md',
  'backend/后端开发指引V3教程/03-请求链路事务异常TraceIdAOP.md',
  'backend/后端开发指引V3教程/06-完整CRUD底层执行链路.md',
  'backend/后端开发指引V3教程/07-CRUD基类能力详解.md',
  'backend/后端开发指引V3教程/08-后端开发标准流程.md',
  'backend/后端开发指引V3教程/09-CrudController与ExtDataCrudController怎么选.md',
  'backend/后端开发指引V3教程/10-跨模块Contract契约.md',
  'backend/后端开发指引V3教程/11-业务流程事务和校验扩展.md',
  'backend/后端开发指引V3教程/12-结合源码的后端深度走读.md',
  'backend/后端开发指引V3教程/A-常用命令.md',
  'backend/后端开发指引V3教程/B-开发检查清单.md',
  'backend/后端开发指引V3教程/C-常见坑.md',
  'backend/后端开发指引V3教程/README.md'
];

export const conceptGroups = [
  {
    text: '运行时与模块化',
    files: [
      'backend/后端底层概念/README.md',
      'backend/后端底层概念/01-启动入口与程序集扫描.md',
      'backend/后端底层概念/02-模块边界与分层职责.md',
      'backend/后端底层概念/03-依赖注入自动注册与AOP代理.md',
      'backend/后端底层概念/04-AOP拦截器与特性使用.md',
      'backend/后端底层概念/05-请求管道统一响应异常TraceId日志.md'
    ]
  },
  {
    text: '数据与扩展',
    files: [
      'backend/后端底层概念/06-数据访问仓储与事务边界.md',
      'backend/后端底层概念/07-CRUD基类与扩展钩子.md',
      'backend/后端底层概念/08-ExtData动态字段.md'
    ]
  },
  {
    text: '契约、配置与规则',
    files: [
      'backend/后端底层概念/09-跨模块Contract契约.md',
      'backend/后端底层概念/10-Config配置底座.md',
      'backend/后端底层概念/11-可插拔校验器.md',
      'backend/后端底层概念/12-编码规则与单据状态.md',
      'backend/后端底层概念/14-策略算法底座.md'
    ]
  },
  {
    text: '安全与运维',
    files: [
      'backend/后端底层概念/13-鉴权缓存用户上下文与运行期开关.md',
      'backend/后端底层概念/15-应用配置Options与运行时配置边界.md',
      'backend/后端底层概念/16-Swagger与OpenAPI接口文档底座.md',
      'backend/后端底层概念/17-JSON序列化与HTTP契约约定.md',
      'backend/后端底层概念/18-MiniProfiler性能观测底座.md',
      'backend/后端底层概念/19-License授权许可与运行时拦截.md',
      'backend/后端底层概念/20-限流RateLimit底座.md',
      'backend/后端底层概念/21-后台服务作业与无请求上下文.md',
      'backend/后端底层概念/22-登录加密与密码哈希.md'
    ]
  }
];

export const apiFiles = [
  'api/README.md',
  'api/PUBLIC-TYPE-INDEX.md',
  'api/KH.WMS.Core-API.md',
  'api/KH.WMS.Config-API.md',
  'api/KH.WMS.Algorithms-API.md'
];

export const referenceFiles = [
  'backend/KH.WMS.Algorithms外部调用与扩展手册.md',
  'backend/KH.WMS.Core-API-参考文档.md',
  'backend/KH.WMS全栈部署配置与环境变量指引.md',
  'backend/KH.WMS前后端联调与接口契约指引.md',
  'backend/KH.WMS后端Contract与模块协作指引.md',
  'backend/KH.WMS后端业务流程专题指引.md',
  'backend/KH.WMS后端排错与日志追踪指引.md',
  'backend/KH.WMS后端接口路由与前端联调清单.md',
  'backend/KH.WMS后端测试与验收指引.md',
  'backend/KH.WMS后端部署与环境配置指引.md',
  'backend/KH.WMS后端配置驱动开发指引.md',
  'backend/全局配置说明.md',
  'backend/物料属性业务说明.md'
];

export const trainingFiles = [
  'backend/KH.WMS 阶段培训计划与文档.md',
  'backend/实战培训/README.md',
  'backend/KH.WMS第一次培训考题.md',
  'backend/培训PPT/新版WMS前端页面开发培训_题库100题.md'
];

export const archivedFiles = [
  'backend/KH.WMS前端开发指引 V1.0.md',
  'backend/KH.WMS后端开发指引 V1.0.md',
  'backend/KH.WMS项目技术栈与目录指引 V2.0.md',
  'backend/KH.WMS后端开发指引V2.0培训考题.md',
  'backend/KH.WMS第一次培训考题 -.md'
];

export const allMappedFiles = [
  ...architectureFiles,
  ...frontendFiles,
  ...backendTutorialFiles,
  ...conceptGroups.flatMap((group) => group.files),
  ...apiFiles,
  ...referenceFiles,
  ...trainingFiles,
  ...archivedFiles
];
