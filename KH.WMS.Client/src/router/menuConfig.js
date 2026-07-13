export const pcMenuList = [
  { title: '首页', icon: 'HomeFilled', path: '/home' },
  {
    title: '入库管理', icon: 'Download', index: 'inbound', permission: 'inbound',
    children: [
      { title: '收货登记', icon: 'Box', path: '/inbound/receiving', permission: 'in_order' },
      { title: '到货验收', icon: 'CircleCheck', path: '/inbound/inspection', permission: 'in_order' },
      { title: '上架管理', icon: 'TopRight', path: '/inbound/putaway', permission: 'in_order' },
    ]
  },
  {
    title: '出库管理', icon: 'Upload', index: 'outbound', permission: 'outbound',
    children: [
      { title: '出库订单', icon: 'Document', path: '/outbound/order', permission: 'out_order' },
      { title: '拣货管理', icon: 'List', path: '/outbound/picking', permission: 'out_order' },
      { title: '分拣复核', icon: 'Finished', path: '/outbound/sorting', permission: 'out_order' },
      { title: '发货管理', icon: 'Van', path: '/outbound/shipping', permission: 'out_order' },
    ]
  },
  {
    title: '库存管理', icon: 'Coin', index: 'inventory',
    children: [
      { title: '库存查询', icon: 'Search', path: '/inventory/stock' },
      { title: '盘点管理', icon: 'EditPen', path: '/inventory/count' },
      { title: '库存调整', icon: 'Edit', path: '/inventory/adjust' },
      { title: '库存冻结', icon: 'Lock', path: '/inventory/freeze' },
    ]
  },
  {
    title: '分拣管理', icon: 'Sort', index: 'sorting',
    children: [
      { title: '波次计划', icon: 'Timer', path: '/sorting/wave' },
      { title: '分拣作业', icon: 'Operation', path: '/sorting/dispatch' },
      { title: '分拣验证', icon: 'CircleCheck', path: '/sorting/verify' },
    ]
  },
  {
    title: '库位管理', icon: 'MapLocation', index: 'warehouse', permission: 'warehouse',
    children: [
      { title: '仓库管理', icon: 'OfficeBuilding', path: '/location/warehouse', permission: 'wh_warehouse' },
      { title: '库区管理', icon: 'Grid', path: '/location/zone', permission: 'wh_zone' },
      { title: '库位管理', icon: 'Position', path: '/location/bin', permission: 'wh_location' },
      { title: '库位图', icon: 'PictureFilled', path: '/location/map' },
      { title: '站台管理', icon: 'Link', path: '/location/port', permission: 'wh_port' },
    ]
  },
  {
    title: '物料管理', icon: 'Goods', index: 'material',
    children: [
      { title: '物料分类', icon: 'Files', path: '/material/category' },
      { title: '物料信息', icon: 'Box', path: '/material/info' },
    ]
  },
  {
    title: '任务中心', icon: 'Bell', index: 'task', permission: 'task',
    children: [
      { title: '入库任务', icon: 'Download', path: '/task/inbound', permission: 'task_list' },
      { title: '出库任务', icon: 'Upload', path: '/task/outbound', permission: 'task_list' },
      { title: '调拨任务', icon: 'Switch', path: '/task/transfer', permission: 'task_list' },
    ]
  },
  {
    title: '报表中心', icon: 'TrendCharts', index: 'report', permission: 'report',
    children: [
      { title: '出入库报表', icon: 'DataLine', path: '/report/inbound', permission: 'rpt_inout' },
      { title: '出库报表', icon: 'Upload', path: '/report/outbound', permission: 'rpt_inout' },
      { title: '库存报表', icon: 'Coin', path: '/report/inventory', permission: 'rpt_inventory' },
      { title: '库位利用率', icon: 'PieChart', path: '/report/location', permission: 'rpt_inventory' },
    ]
  },
  { title: '数据大屏', icon: 'Monitor', path: '/dashboard' },
  {
    title: '设备监控', icon: 'Cpu', index: 'equipment',
    children: [
      { title: '堆垛机监控', icon: 'SetUp', path: '/equipment/crane' },
      { title: '输送线监控', icon: 'Right', path: '/equipment/conveyor' },
    ]
  },
  {
    title: '系统管理', icon: 'Setting', index: 'system', permission: 'system',
    children: [
      { title: '用户管理', icon: 'User', path: '/system/user', permission: 'sys_user' },
      { title: '角色管理', icon: 'UserFilled', path: '/system/role', permission: 'sys_role' },
      { title: '菜单管理', icon: 'Menu', path: '/system/menu', permission: 'sys_menu' },
      { title: '字典管理', icon: 'Collection', path: '/system/dict', permission: 'sys_dict' },
      { title: '操作日志', icon: 'Notebook', path: '/system/log', permission: 'sys_log' },
      { title: '系统参数', icon: 'Tools', path: '/system/parameter', permission: 'sys_param' },
    ]
  },
]
