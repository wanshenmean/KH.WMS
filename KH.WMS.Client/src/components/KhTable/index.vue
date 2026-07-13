<template>
  <div class="kh-table">
    <!-- 顶部工具栏 -->
    <div v-if="showToolbar" class="kh-table__toolbar">
      <div class="kh-table__toolbar-left">
        <span v-if="title" class="kh-table__title">{{ title }}</span>
        <span v-if="showSelection && showSelectionInfo && selectionCount > 0" class="kh-table__selection-info">
          已选择 <strong>{{ selectionCount }}</strong> 项
        </span>
        <div class="kh-table__toolbar-actions">
          <slot name="toolbar-left" />
          <template v-for="btn in visibleToolbarButtons" :key="btn.label">
            <el-dropdown v-if="btn.dropdown" split-button :type="btn.type || ''" @click="btn.onClick?.()" @command="(cmd) => btn.dropdown[cmd]?.onClick?.()">
              {{ btn.label }}
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item v-for="(item, idx) in btn.dropdown" :key="item.label" :command="idx">{{ item.label }}</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
            <el-button v-else :type="btn.type || ''" :plain="btn.plain" :circle="btn.circle" :loading="btn.loading"
              :disabled="btn.disabled" :icon="btn.icon" @click="btn.onClick?.()">{{ btn.label }}</el-button>
          </template>
        </div>
      </div>
      <div class="kh-table__toolbar-right">
        <slot name="toolbar-right">
          <el-button v-if="showHeaderFilter && hasActiveFilters" link type="primary" size="small"
            @click="clearAllFilters">
            <el-icon class="el-icon--left"><RefreshRight /></el-icon>清除筛选
          </el-button>
          <el-tooltip v-if="showRefresh" content="刷新" placement="top">
            <el-button :icon="RefreshIcon" circle @click="handleRefresh" />
          </el-tooltip>
          <el-tooltip v-if="showColumnSetting && false" content="列设置" placement="top">
            <el-button :icon="SettingIcon" circle @click="columnSettingVisible = true" />
          </el-tooltip>
        </slot>
      </div>
    </div>

    <!-- 表格 -->
    <el-table ref="tableRef" :data="tableData" :border="border" :stripe="effectiveStripe" :size="size" :height="height"
      :max-height="maxHeight" :row-key="rowKey" :highlight-current-row="highlightCurrentRow"
      :default-expand-all="defaultExpandAll" :tree-props="treeProps" :header-cell-style="headerCellStyle"
      :row-style="getRowStyle" :row-class-name="rowClassName"
      :class="{ 'kh-table--has-filter-row': showHeaderFilter && hasSearchableColumns }" v-bind="$attrs"
      @selection-change="handleSelectionChange" @sort-change="handleSortChange" @current-change="handleCurrentChange"
      @row-click="handleRowClick" @row-dblclick="handleRowDblclick">
      <!-- 展开列 (主从表格) -->
      <el-table-column v-if="hasExpandColumn" type="expand">
        <template #default="scope">
          <slot name="expand" v-bind="scope" />
        </template>
      </el-table-column>

      <!-- 选择列 -->
      <el-table-column v-if="showSelection" type="selection" :width="selectionWidth" :fixed="selectionFixed"
        align="center" />

      <!-- 序号列 -->
      <el-table-column v-if="showIndex" type="index" label="序号" :width="indexWidth" :fixed="indexFixed" align="center"
        :index="indexMethod" />

      <!-- 数据列 -->
      <template v-for="col in visibleColumns" :key="col.prop">
        <el-table-column :prop="col.prop" :label="col.label" :width="col.width" :min-width="col.minWidth"
          :fixed="col.fixed" :sortable="col.sortable" :align="col.align || 'center'"
          :header-align="col.headerAlign || col.align || 'center'"
          :show-overflow-tooltip="col.showOverflowTooltip !== false"
          :class-name="showHeaderFilter && col.searchable ? 'kh-table__filterable-col' : ''"
          :cell-style="(data) => getCellStyle(col, data)" v-bind="col.bindProps">
          <!-- 表头 -->
          <template #header>
            <div class="kh-table__col-header">
              <el-popover v-if="showHeaderFilter && col.searchable" placement="bottom"
                v-model:visible="popoverVisibleMap[col.prop]" :width="getFilterPopoverWidth(col)" trigger="click"
                :show-arrow="false" :offset="4" :hide-after="0"
                popper-class="kh-table__filter-popover" @show="onFilterPopoverShow(col.prop)"
                @hide="onFilterPopoverHide(col.prop)">
                <template #reference>
                  <span class="kh-table__filter-icon" :class="{ 'is-active': isFilterActive(col) }">
                    <el-icon :size="14">
                      <Search />
                    </el-icon>
                  </span>
                </template>
                <div class="kh-table__filter-dropdown" @mousedown.stop>
                  <div class="kh-table__filter-dropdown-title">{{ col.label }}</div>

                  <!-- 文本输入 (默认) -->
                  <el-input v-if="col.filterType === 'input' || !col.filterType" ref="filterInputRefs"
                    v-model="headerFilters[col.prop]" size="small" clearable :placeholder="`搜索${col.label}`"
                    @keyup.enter="confirmFilter">
                    <template #prefix>
                      <el-icon :size="14">
                        <Search />
                      </el-icon>
                    </template>
                  </el-input>

                  <!-- 下拉单选 -->
                  <el-select v-else-if="col.filterType === 'select'" v-model="headerFilters[col.prop]" size="small"
                    clearable filterable :placeholder="`请选择${col.label}`" class="kh-table__filter-select"
                    :loading="isFilterOptionsLoading(col)" :teleported="false">
                    <el-option v-for="opt in getFilterOptions(col)" :key="opt.value ?? opt"
                      :label="opt.label ?? opt" :value="opt.value ?? opt">
                      <template v-if="hasColumnTagColors(col)" #default>
                        <span class="kh-table__tag-select-option">
                          <span v-if="getTagColorForValue(col, opt.value ?? opt)"
                            class="kh-table__color-dot" :class="'is-' + getTagColorForValue(col, opt.value ?? opt)" />
                          <span>{{ opt.label ?? opt }}</span>
                        </span>
                      </template>
                    </el-option>
                  </el-select>

                  <!-- 下拉多选 -->
                  <el-select v-else-if="col.filterType === 'multiple-select'" v-model="headerFilters[col.prop]"
                    size="small" multiple clearable filterable collapse-tags collapse-tags-tooltip
                    :placeholder="`请选择${col.label}`" class="kh-table__filter-select"
                    :loading="isFilterOptionsLoading(col)" :teleported="false">
                    <el-option v-for="opt in getFilterOptions(col)" :key="opt.value ?? opt"
                      :label="opt.label ?? opt" :value="opt.value ?? opt">
                      <template v-if="hasColumnTagColors(col)" #default>
                        <span class="kh-table__tag-select-option">
                          <span v-if="getTagColorForValue(col, opt.value ?? opt)"
                            class="kh-table__color-dot" :class="'is-' + getTagColorForValue(col, opt.value ?? opt)" />
                          <span>{{ opt.label ?? opt }}</span>
                        </span>
                      </template>
                    </el-option>
                  </el-select>

                  <!-- 数值区间 -->
                  <div v-else-if="col.filterType === 'number-range'" class="kh-table__filter-number-range">
                    <el-input-number v-model="headerFilters[col.prop].min" size="small" :placeholder="`最小值`"
                      :controls="false" class="kh-table__filter-number-input" />
                    <span class="kh-table__filter-number-sep">~</span>
                    <el-input-number v-model="headerFilters[col.prop].max" size="small" :placeholder="`最大值`"
                      :controls="false" class="kh-table__filter-number-input" />
                  </div>

                  <!-- 日期范围 -->
                  <el-date-picker v-else-if="col.filterType === 'date-range'" v-model="headerFilters[col.prop]"
                    type="daterange" size="small" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期"
                    value-format="YYYY-MM-DD" :clearable="true" :teleported="false" class="kh-table__filter-date" />

                  <!-- 日期时间范围 -->
                  <el-date-picker v-else-if="col.filterType === 'datetime-range'" v-model="headerFilters[col.prop]"
                    type="datetimerange" size="small" range-separator="至" start-placeholder="开始时间"
                    end-placeholder="结束时间" value-format="YYYY-MM-DD HH:mm:ss" :clearable="true"
                    :teleported="false" class="kh-table__filter-date" />

                  <div class="kh-table__filter-dropdown-footer">
                    <el-button size="small" link @click="clearSingleFilter(col.prop)">重置</el-button>
                    <el-button size="small" type="primary" @click="confirmFilter">确定</el-button>
                  </div>
                </div>
              </el-popover>
              <span class="kh-table__col-label">{{ col.label }}</span>
            </div>
          </template>

          <template #default="{ row, $index }">
            <slot v-if="col.type === 'slot'" :name="col.prop" :row="row" :column="col" :index="$index" />
            <template v-else-if="col.type === 'tag'">
              <el-tag v-if="getColValue(col, row) !== null && getColValue(col, row) !== undefined && getColValue(col, row) !== ''" :type="getTagType(col, row)" :effect="col.tagEffect || 'light'" :disable-transitions="true">{{
                getTagLabel(col, row) }}</el-tag>
              <span v-else>-</span>
            </template>
            <template v-else-if="col.type === 'image'">
              <el-image :src="getColValue(col, row)"
                :style="{ width: col.imageWidth || '40px', height: col.imageHeight || '40px' }"
                :preview-src-list="[getColValue(col, row)]" fit="cover" preview-teleported />
            </template>
            <template v-else-if="col.type === 'switch'">
              <el-switch :model-value="getColValue(col, row)" @change="(val) => handleCellChange(col, row, val)"
                :active-value="col.activeValue" :inactive-value="col.inactiveValue"
                :active-text="col.activeText" :inactive-text="col.inactiveText"
                v-bind="col.switchProps" />
            </template>
            <template v-else-if="col.type === 'tag-select'">
              <el-select :model-value="getColValue(col, row)"
                @change="(val) => handleCellChange(col, row, val)" size="small" v-bind="col.selectProps"
                class="kh-table__tag-select">
                <el-option v-for="opt in getTagSelectOptions(col)" :key="opt.value" :label="opt.label"
                  :value="opt.value">
                  <span class="kh-table__tag-select-option">
                    <span v-if="getTagColorForValue(col, opt.value)" class="kh-table__color-dot"
                      :class="'is-' + getTagColorForValue(col, opt.value)" />
                    <span>{{ opt.label }}</span>
                  </span>
                </el-option>
              </el-select>
            </template>
            <template v-else-if="col.type === 'link'">
              <el-link type="primary" :underline="false" @click="col.onClick?.(row, $index)">{{ getColValue(col, row)
                }}</el-link>
            </template>
            <template v-else-if="col.formatter">{{ col.formatter(row, col, getColValue(col, row), $index) }}</template>
            <template v-else>{{ getColValue(col, row) ?? '-' }}</template>
          </template>
        </el-table-column>
      </template>

      <!-- 操作列 -->
      <el-table-column v-if="$slots.action || actionColumns?.length || hasAnyVisibleAction" :label="actionLabel"
        :width="actionWidth" :min-width="actionMinWidth" :fixed="actionFixed" align="center">
        <template #default="{ row, $index }">
          <slot name="action" :row="row" :index="$index">
            <!-- 兼容旧 actionColumns 配置 -->
            <template v-for="(act, idx) in actionColumns" :key="act.text">
              <el-button :type="act.type || 'primary'" :link="true" :size="size" :disabled="act.disabled?.(row)"
                @click="act.onClick?.(row, $index)">{{ act.text }}</el-button>
              <el-divider v-if="idx < actionColumns.length - 1" direction="vertical" />
            </template>
            <!-- 新 actionButtons 配置（支持权限过滤、函数/字符串 label/confirm） -->
            <template v-for="(btn, idx) in getVisibleActionButtons(row)" :key="idx">
              <el-popconfirm v-if="btn.confirm" :title="typeof btn.confirm === 'function' ? btn.confirm(row) : btn.confirm" @confirm="btn.onClick?.(row, $index)">
                <template #reference>
                  <el-button :type="btn.type || 'primary'" :link="btn.link !== false" :size="btn.size || 'small'"
                    :disabled="btn.disabled?.(row)">{{ typeof btn.label === 'function' ? btn.label(row) : btn.label }}</el-button>
                </template>
              </el-popconfirm>
              <el-button v-else :type="btn.type || 'primary'" :link="btn.link !== false" :size="btn.size || 'small'"
                :disabled="btn.disabled?.(row)" @click="btn.onClick?.(row, $index)">{{ typeof btn.label === 'function' ? btn.label(row) : btn.label }}</el-button>
              <el-divider v-if="idx < getVisibleActionButtons(row).length - 1" direction="vertical" />
            </template>
          </slot>
        </template>
      </el-table-column>

      <template #empty>
        <slot name="empty">
          <div class="kh-table__empty"><el-empty description="暂无数据" /></div>
        </slot>
      </template>
    </el-table>

    <!-- 分页 -->
    <div v-if="showPagination" class="kh-table__pagination">
      <el-pagination :current-page="pageParam.pageNum" :page-size="pageParam.pageSize" :page-sizes="pageSizes"
        :total="actualTotal" :background="true" layout="total, sizes, prev, pager, next, jumper"
        @size-change="handleSizeChange" @current-change="handlePageChange" />
    </div>

    <!-- 列设置弹窗 -->
    <el-dialog v-model="columnSettingVisible" title="列设置" width="420px" append-to-body>
      <div class="kh-table__column-setting">
        <el-checkbox v-model="checkAll" :indeterminate="isIndeterminate" @change="handleCheckAllChange">全选</el-checkbox>
        <el-divider style="margin: 8px 0" />
        <div class="kh-table__column-list">
          <div v-for="col in columnList" :key="col.prop" class="kh-table__column-item">
            <el-icon class="drag-handle">
              <Rank />
            </el-icon>
            <el-checkbox v-model="col.visible">{{ col.label }}</el-checkbox>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
/**
 * @file KhTable - 通用业务表格组件
 * @description 基于 Element Plus el-table 封装的企业级表格组件，提供分页、列设置、表头筛选、
 *              多种列渲染类型（标签、图片、开关、链接、插槽、格式化）等功能。
 *              支持工具栏、操作列、展开行（主从表格）等常用业务场景。
 *
 * @component KhTable
 *
 * ============================================================
 *  Props 属性
 * ============================================================
 *
 * @prop {Array}                 data               - 表格数据数组，默认 []
 * @prop {Array}                 columns            - 列配置数组（必填），默认 []，详见下方 columns 配置说明
 * @prop {Boolean}               loading            - 是否显示加载状态，默认 false
 * @prop {String|Function}       rowKey             - 行数据的 Key，用于树形数据或行选中，默认 'id'
 * @prop {Boolean}               border             - 是否带有纵向边框，默认 true
 * @prop {Boolean}               stripe             - 是否为斑马纹表格，默认 false
 * @prop {String}                size               - 表格尺寸，可选 'large' | 'default' | 'small'，默认 'default'
 * @prop {String|Number}         height             - 表格固定高度，默认 undefined（自适应）
 * @prop {String|Number}         maxHeight          - 表格最大高度，默认 undefined（不限制）
 * @prop {Boolean}               highlightCurrentRow - 是否高亮当前行，默认 false
 * @prop {Boolean}               defaultExpandAll   - 是否默认展开所有树节点，默认 false
 * @prop {Object}                treeProps          - 树形数据配置，默认 { children: 'children', hasChildren: 'hasChildren' }
 * @prop {Object}                headerCellStyle    - 表头单元格样式，默认 { background: '#f5f7fa', color: '#606266' }
 * @prop {Boolean}               showSelection      - 是否显示多选列，默认 false
 * @prop {Number}                selectionWidth     - 多选列宽度，默认 55
 * @prop {String|Boolean}        selectionFixed     - 多选列是否固定，默认 false
 * @prop {Boolean}               showIndex          - 是否显示序号列，默认 false
 * @prop {Number}                indexWidth         - 序号列宽度，默认 65
 * @prop {String|Boolean}        indexFixed         - 序号列是否固定，默认 false
 * @prop {Array}                 actionColumns      - 操作列按钮配置数组，默认 []
 * @prop {String}                actionLabel        - 操作列标题，默认 '操作'
 * @prop {String|Number}         actionWidth        - 操作列宽度，默认 undefined（自适应）
 * @prop {String|Number}         actionMinWidth     - 操作列最小宽度，默认 undefined
 * @prop {String|Boolean}        actionFixed        - 操作列固定方向，默认 'right'
 * @prop {Boolean}               showPagination     - 是否显示分页组件，默认 true
 * @prop {Number}                total              - 数据总条数，默认 0
 * @prop {Number}                pageNum            - 当前页码，默认 1
 * @prop {Number}                pageSize           - 每页条数，默认 10
 * @prop {Array}                 pageSizes          - 每页条数选项，默认 [10, 20, 50, 100]
 * @prop {Boolean}               showToolbar        - 是否显示顶部工具栏，默认 false
 * @prop {Boolean}               showRefresh        - 是否显示刷新按钮（工具栏内），默认 true
 * @prop {Boolean}               showColumnSetting  - 是否显示列设置弹窗（工具栏内），默认 false
 * @prop {Boolean}               showHeaderFilter   - 是否启用表头筛选功能，默认 false
 *
 * ============================================================
 *  Events 事件
 * ============================================================
 *
 * @event {Function} update:pageNum    - 页码变化时触发，参数: (pageNum: number)
 * @event {Function} update:pageSize   - 每页条数变化时触发，参数: (pageSize: number)
 * @event {Function} refresh           - 点击刷新按钮时触发，无参数
 * @event {Function} search            - 需要重新请求数据时触发（翻页/刷新/确认筛选），无参数
 * @event {Function} selection-change  - 选中行变化时触发，参数: (selection: Array)
 * @event {Function} sort-change       - 排序变化时触发，参数: ({ column, prop, order })
 * @event {Function} current-change    - 当前高亮行变化时触发，参数: (currentRow: Object)
 * @event {Function} row-click         - 行单击时触发，参数: (row: Object)
 * @event {Function} row-dblclick      - 行双击时触发，参数: (row: Object)
 * @event {Function} cell-change       - 单元格内可编辑组件值变化时触发，参数: (prop: string, row: Object, value: any)
 * @event {Function} header-filter     - 表头筛选条件变化时触发，参数: (filters: Object) 键为列 prop，值类型取决于 filterType
 *
 * ============================================================
 *  Expose 暴露的方法与属性
 * ============================================================
 *
 * @expose {Ref}                 tableRef           - el-table 实例引用
 * @expose {Function}            getSelectionRows   - 获取当前选中的行数据
 *   @param  - 无参数
 *   @returns {Array} 选中的行数据数组
 * @expose {Function}            clearSelection     - 清除所有选中状态
 *   @param  - 无参数
 *   @returns {void}
 * @expose {Function}            toggleRowSelection - 切换某一行的选中状态
 *   @param {Object}  row      - 目标行数据
 *   @param {Boolean} selected - 是否选中
 *   @returns {void}
 * @expose {Function}            clearSort          - 清除所有排序状态
 *   @param  - 无参数
 *   @returns {void}
 * @expose {Function}            sort               - 手动触发排序
 *   @param {string}  prop  - 列的 prop 字段名
 *   @param {string}  order - 排序方式: 'ascending' | 'descending' | null
 *   @returns {void}
 * @expose {Function}            setCurrentRow      - 设置某一行为当前高亮行
 *   @param {Object} row - 目标行数据
 *   @returns {void}
 * @expose {Function}            clearFilter        - 清除指定列的筛选状态
 *   @param {Array} [columnKeys] - 需要清除筛选的列 prop 数组，不传则清除所有列
 *   @returns {void}
 * @expose {Ref<Array>}          columnList         - 列配置列表（可读写，包含 visible 属性）
 * @expose {Reactive<Object>}    headerFilters      - 当前表头筛选条件对象（响应式）
 * @expose {Function}            clearHeaderFilters - 清除所有表头筛选条件并触发 header-filter 事件
 *   @param  - 无参数
 *   @returns {void}
 *
 * ============================================================
 *  Slots 插槽
 * ============================================================
 *
 * @slot toolbar-left   - 工具栏左侧内容区域（与 toolbarButtons 配置共存）
 * @slot toolbar-right  - 工具栏右侧内容区域（默认包含刷新和列设置按钮）
 * @slot expand         - 展开行内容，接收 { row, $index, store, expanded } 等作用域参数
 * @slot [col.prop]     - 自定义列内容（当列 type='slot' 时），接收 { row, column, index } 参数
 * @slot action         - 操作列内容，接收 { row, index } 参数；不使用插槽时可配置 actionButtons 属性
 * @slot empty          - 空数据时的自定义内容，默认显示 "暂无数据"
 *
 * ============================================================
 *  columns 列配置数组项属性说明
 * ============================================================
 *
 * --- 通用属性 ---
 * @property {string}          prop                - 字段名，对应行数据中的属性名
 * @property {string}          label               - 列标题文本
 * @property {string|number}   [width]             - 列宽度
 * @property {string|number}   [minWidth]          - 列最小宽度
 * @property {string|boolean}  [fixed]             - 列是否固定，可选 'left' | 'right' | true | false
 * @property {boolean|string}  [sortable]          - 是否可排序，可选 true | false | 'custom'
 * @property {string}          [align]             - 列内容对齐方式，可选 'left' | 'center' | 'right'，默认 'center'
 * @property {string}          [headerAlign]       - 列头对齐方式，默认与 align 一致
 * @property {boolean}         [showOverflowTooltip] - 内容溢出时是否显示 tooltip，默认 true
 * @property {boolean}         [visible]           - 列是否可见，默认 true
 * @property {string}          [type]              - 列渲染类型，可选 'slot' | 'tag' | 'tag-select' | 'image' | 'switch' | 'link' | 'expand' | 'formatter' | undefined（普通文本）
 * @property {number}          [span]              - 列合并数
 * @property {Object}          [bindProps]         - 透传给 el-table-column 的额外属性
 *
 * --- type='slot' 自定义插槽列 ---
 * @description 使用列的 prop 作为插槽名称，渲染自定义内容。
 *   作用域参数: { row, column, index }
 *
 * --- type='tag' 标签列 ---
 * @property {Object}          [tagMap]            - 值到标签类型或显示文本的映射
 *   当值为合法标签类型（'success'|'warning'|'danger'|'info'|'primary'|''）时，同时作为类型和显示原文
 *   当值为其他字符串时，作为显示文本，标签类型由 tagTypeMap 指定
 *   示例（纯类型映射）: { 1: 'success', 0: 'danger' }
 *   示例（文本映射，需配合 tagTypeMap）: { normal: '正常', frozen: '冻结' }
 * @property {Object}          [tagTypeMap]        - 值到标签类型的映射（优先级高于 tagMap 中的类型推断）
 *   示例: { normal: 'success', frozen: 'danger', warning: 'warning' }
 * @property {string}          [tagEffect]         - 标签主题效果，可选 'dark' | 'light' | 'plain'，默认 'light'
 *
 * --- type='image' 图片列 ---
 * @property {string}          [imageWidth]        - 图片显示宽度，默认 '40px'
 * @property {string}          [imageHeight]       - 图片显示高度，默认 '40px'
 *
 * --- type='switch' 开关列 ---
 * @property {*}               [activeValue]       - 开关打开时的值，默认 true。设为 1/0 等可实现数值型开关
 * @property {*}               [inactiveValue]     - 开关关闭时的值，默认 false
 * @property {string}          [activeText]        - 开关打开时的文字描述
 * @property {string}          [inactiveText]      - 开关关闭时的文字描述
 * @property {Object}          [switchProps]       - 透传给 el-switch 的额外属性
 *
 * --- type='tag-select' 标签选择列 ---
 * @description 带颜色标识的可编辑下拉选择列，下拉选项前显示颜色圆点。
 *   tagMap、tagTypeMap、tagEffect 属性与 type='tag' 相同。值变化时触发 cell-change 事件。
 * @property {Array|Object}    [options]           - 下拉选项数组 [{ label, value }] 或字典引用 'dict:xxx'，不指定时自动从 tagMap / tagTypeMap 合成
 * @property {Object}          [selectProps]       - 透传给 el-select 的额外属性，如 { placeholder, clearable }
 *
 * --- type='link' 链接列 ---
 * @property {Function}        [onClick]           - 点击链接时的回调函数，接收参数 (row, index)
 *
 * --- type='expand' 展开列 ---
 * @description 配置一个 type='expand' 的列即可启用展开行功能，
 *   展开内容通过 expand 插槽自定义渲染（适用于主从表格场景）。
 *
 * --- type='formatter' 格式化列 ---
 * @property {Function}        [formatter]         - 自定义格式化函数，接收参数 (row, column, cellValue, index)
 *   应返回渲染内容（字符串或 VNode）
 *
 * --- searchable 表头筛选 ---
 * @property {boolean}         [searchable]        - 是否启用该列的表头筛选功能，需要配合 showHeaderFilter=true 使用
 * @property {string}          [filterType]        - 筛选控件类型，默认 'input'
 *   可选值: 'input'(文本输入) | 'select'(下拉单选) | 'multiple-select'(下拉多选) | 'number-range'(数值区间) | 'date-range'(日期范围) | 'datetime-range'(日期时间范围)
 * @property {string}          [filterMatchMode]   - 筛选匹配模式（运算符），覆盖 filterType 的默认运算符
 *   可选值: 'equals'(等于) | 'contains'(包含) | 'startswith'(前缀匹配) | 'endwith'(后缀匹配) | 'gt'(大于) | 'gte'(大于等于) | 'lt'(小于) | 'lte'(小于等于) | 'in'(包含于) | 'notnull'(不为空) | 'isnull'(为空)
 *   各 filterType 默认运算符: input→'contains', select→'equals', multiple-select→'in', number-range→'gte'/'lte', date-range→'gte'/'lte'
 *   示例: 对主键列使用精确匹配: { prop: 'id', filterType: 'input', filterMatchMode: 'equals' }
 *   示例: 筛选不为空的记录: { prop: 'remark', searchable: true, filterMatchMode: 'notnull' }
 * @property {Array}           [filterOptions]     - 筛选下拉选项数组，仅 filterType 为 'select' 或 'multiple-select' 时有效
 *   格式: [{ label: string, value: string|number }]，支持字典引用 'dict:xxx'
 *   当列同时配置了 tagMap 或 tagTypeMap 时，下拉选项前会自动显示颜色圆点标识
 * 
 *  1. API 异步函数（最高优先级）

  filterOptions 设为 async 函数，点击筛选图标时自动调用：

  {
    prop: 'zone',
    label: '库区',
    type: 'tag',
    tagMap: 'dict:zone_status',
    searchable: true,
    filterType: 'select',
    filterOptions: async () => {
      const res = await api.getZoneList()
      return res.data.map(item => ({ label: item.name, value: item.id }))
    },
  }

  2. 自动从 tagMap / tagTypeMap 绑定（无需手动配置）

  只要列有 tagMap 或 tagTypeMap，筛选下拉自动生成相同选项：

  {
    prop: 'binStatus',
    label: '状态',
    type: 'tag',
    tagMap: 'dict:location_status',   // 字典自动加载
    searchable: true,
    filterType: 'select',
    // filterOptions 不用写，自动从 tagMap 推导
  }

  3. 硬编码 / 字典引用

  // 字典引用
  { filterOptions: 'dict:warehouse_list' }

  // 静态数组
  { filterOptions: [{ label: '是', value: 1 }, { label: '否', value: 0 }] }

  // ref（响应式）
  { filterOptions: zoneOptions }  // ref，自动解包
 */
import { usePermissionStore } from '@/stores/permission'
import { useDictStore } from '@/stores/dict'
import { resolveColumn, collectDictTypes } from '@/utils/dict-resolve'
import { resolveRowStyle } from '@/utils/row-style-presets'

// 图标引用（供模板 :icon 绑定使用，需在 script 中声明才能暴露到 $setup）
const RefreshIcon = Refresh
const SettingIcon = Setting

const dictStore = useDictStore()

const props = defineProps({
  /** 表格数据数组 */
  data: { type: Array, default: () => [] },
  /** 列配置数组（必填），定义表格中每一列的展示方式和行为 */
  columns: { type: Array, required: true, default: () => [] },
  /** 是否显示加载遮罩层 */
  loading: { type: Boolean, default: false },
  /** 行数据的 Key，用于树形数据或行选中，默认 'id' */
  rowKey: { type: [String, Function], default: 'id' },
  /** 是否带有纵向边框，默认 true */
  border: { type: Boolean, default: true },
  /** 是否为斑马纹表格，默认 false */
  stripe: { type: Boolean, default: true },
  /** 表格尺寸: 'large' | 'default' | 'small'，默认 'default' */
  size: { type: String, default: 'default' },
  /** 表格固定高度，undefined 表示自适应内容高度 */
  height: { type: [String, Number], default: undefined },
  /** 表格最大高度，undefined 表示不限制 */
  maxHeight: { type: [String, Number], default: undefined },
  /** 是否高亮当前选中行，默认 false */
  highlightCurrentRow: { type: Boolean, default: false },
  /** 是否默认展开所有树形节点，默认 false */
  defaultExpandAll: { type: Boolean, default: false },
  /** 树形数据的子节点和是否有子节点字段映射 */
  treeProps: { type: Object, default: () => ({ children: 'children', hasChildren: 'hasChildren' }) },
  /** 表头单元格样式对象，默认浅灰色背景 + 深灰色文字 */
  headerCellStyle: { type: Object, default: () => ({ background: '#f5f7fa', color: '#606266' }) },
  /**
   * 行样式函数，根据行数据返回行内联样式
   * @type {Function|null}
   * @param {Object} row - 行数据
   * @param {number} rowIndex - 行索引
   * @returns {Object} CSS 样式对象，如 { backgroundColor: '#fff2f0' }
   * @default null
   */
  rowStyle: { type: Function, default: null },
  /**
   * 1. 行背景色 — rowStyle prop

  在 <KhTable> 上设置，函数返回 CSS 样式对象：

  <KhTable
    :columns="columns"
    :data="tableData"
    :row-style="(row) => ({
      backgroundColor: row.binStatus === '锁定' ? '#fff2f0' : '',
    })"
  />

  2. 单元格字体颜色 — 列配置 cellStyle

  在 columns 的某一列上设置：

  const columns = [
    {
      prop: 'binStatus',
      label: '状态',
      type: 'tag',
      tagMap: 'dict:location_status',
      // 根据条件设置单元格样式
      cellStyle: (row, col, value, rowIndex) => {
        if (value === '维护中') return { color: '#e6a23c', fontWeight: '600' }
        if (value === '锁定') return { color: '#f56c6c', fontWeight: '600' }
        if (value === '空闲') return { color: '#67c23a' }
        return {}
      },
    },
    {
      prop: 'quantity',
      label: '数量',
      cellStyle: (row) => {
        if (row.quantity > 100) return { color: '#f56c6c', fontWeight: 'bold' }
        return {}
      },
    },
  ]

  3. 行类名 — rowClassName prop

  也可以通过 CSS 类名控制样式（适合复杂样式）：

  <KhTable
    :row-class-name="(row) => row.binStatus === '锁定' ? 'row-locked' : ''"
  />

  :deep(.row-locked) {
    background-color: #fff2f0 !important;
  }
  :deep(.row-locked td) {
    color: #f56c6c !important;
  }
   */
  /**
   * 行类名函数，根据行数据返回自定义 CSS 类名
   * @type {Function|null}
   * @param {Object} row - 行数据
   * @param {number} rowIndex - 行索引
   * @returns {string} CSS 类名
   * @default null
   */
  rowClassName: { type: [String, Function], default: '' },
  /** 是否显示左侧多选（checkbox）列，默认 true */
  showSelection: { type: Boolean, default: true },
  /** 多选列的宽度，默认 55px */
  selectionWidth: { type: Number, default: 55 },
  /** 多选列是否固定及固定方向，默认 false（不固定） */
  selectionFixed: { type: [String, Boolean], default: false },
  /** 是否显示序号列，默认 true */
  showIndex: { type: Boolean, default: true },
  /** 序号列的宽度，默认 65px */
  indexWidth: { type: Number, default: 65 },
  /** 序号列是否固定及固定方向，默认 false（不固定） */
  indexFixed: { type: [String, Boolean], default: false },
  /** 操作列按钮配置数组，每项包含 { text, type, onClick, disabled } */
  actionColumns: { type: Array, default: () => [] },
  /** 操作列的标题文字，默认 '操作' */
  actionLabel: { type: String, default: '操作' },
  /** 操作列宽度，undefined 表示自适应 */
  actionWidth: { type: [String, Number], default: undefined },
  /** 操作列最小宽度，undefined 表示不设置 */
  actionMinWidth: { type: [String, Number], default: undefined },
  /** 操作列固定方向，默认固定在右侧 'right' */
  actionFixed: { type: [String, Boolean], default: 'right' },
  /** 是否显示底部分页组件，默认 true */
  showPagination: { type: Boolean, default: true },
  /** 数据总条数，用于分页计算 */
  total: { type: Number, default: 0 },
  /** 当前页码，默认第 1 页 */
  pageNum: { type: Number, default: 1 },
  /** 每页显示条数，默认 30 */
  pageSize: { type: Number, default: 30 },
  /** 每页条数选择器的可选项，默认 [30, 50, 100, 200] */
  pageSizes: { type: Array, default: () => [30, 50, 100, 200] },
  /** 是否显示顶部工具栏区域，默认 false */
  showToolbar: { type: Boolean, default: false },
  /** 工具栏标题，显示在工具栏左侧 */
  title: { type: String, default: '' },
  /** 工具栏中是否显示刷新按钮，默认 true */
  showRefresh: { type: Boolean, default: true },
  /** 工具栏中是否显示列设置（列显隐 + 拖拽排序）按钮，默认 false */
  showColumnSetting: { type: Boolean, default: true },
  /** 是否启用表头筛选功能（可搜索列的表头会显示筛选图标），默认 false */
  showHeaderFilter: { type: Boolean, default: false },
  /** 是否在工具栏左侧显示已选行数提示（需配合 showSelection 使用），默认 true */
  showSelectionInfo: { type: Boolean, default: true },
  /**
   * 数据加载函数，传入后表格自动管理数据加载。
   * @type {Function|null}
   * @param {Object} params - 查询参数 { pageNum, pageSize, ...extraParams }
   * @returns {Promise<{ data: Array, total: number }>}
   * @default null
   */
  load: { type: Function, default: null },
  /** 挂载时是否自动调用 load 加载数据，默认 true */
  autoLoad: { type: Boolean, default: true },
  /** 每次调用 load 时额外携带的参数对象（响应式），默认空对象 */
  extraParams: { type: Object, default: () => ({}) },
  /**
   * 工具栏按钮配置数组，支持权限自动过滤。
   * 每项: { label, type, icon, permission, onClick, disabled, loading, plain, circle, show }
   * @default []
   */
  toolbarButtons: { type: Array, default: () => [] },
  /**
   * 操作列按钮配置数组，支持权限自动过滤和 confirm 确认。
   * 每项继承 toolbarButtons 全部属性，额外支持: { link, size, confirm, show(row) }
   * @default []
   */
  actionButtons: { type: Array, default: () => [] },
})

const emit = defineEmits([
  /** 页码变化时触发，参数: (pageNum: number) */
  'update:pageNum',
  /** 每页条数变化时触发，参数: (pageSize: number) */
  'update:pageSize',
  /** 点击刷新按钮时触发，无参数 */
  'refresh',
  /** 需要重新请求数据时触发（翻页、刷新、确认筛选均会触发），无参数 */
  'search',
  /** 选中行变化时触发，参数: (selection: Array) 当前所有选中的行数据 */
  'selection-change',
  /** 排序条件变化时触发，参数: ({ column, prop, order }) */
  'sort-change',
  /** 当前高亮行变化时触发，参数: (currentRow: Object) */
  'current-change',
  /** 行单击时触发，参数: (row: Object) */
  'row-click',
  /** 行双击时触发，参数: (row: Object) */
  'row-dblclick',
  /** 单元格内可编辑组件（如 switch）的值变化时触发，参数: (prop: string, row: Object, value: any) */
  'cell-change',
  /** 表头筛选条件变化时触发，参数: (filters: Object) 键为列 prop，值为筛选文本 */
  'header-filter',
  /** 数据加载前触发，参数: (params: Object) 可修改参数或返回 false 取消加载 */
  'before-load',
  /** 数据加载并绑定完成后触发，参数: (response: { data, total }) */
  'after-load',
  /** 数据加载失败时触发，参数: (error: Error) */
  'load-error',
])

/** el-table 组件实例引用 */
const tableRef = ref(null)

/** 当前选中的行数 */
const selectionCount = ref(0)
const columnSettingVisible = ref(false)

/** 权限 store */
const permissionStore = usePermissionStore()

/** 插槽实例，用于判断是否有自定义内容 */
const slots = useSlots()

/**
 * 判断单个按钮是否可见（权限 + show 函数）
 * @param {Object} btn - 按钮配置
 * @param {Object} [row] - 行数据（仅 actionButtons 传入）formColumns
 * @returns {boolean}
 */
const isButtonVisible = (btn, row) => {
  if (btn.permission && !permissionStore.hasButtonPermission(btn.permission)) return false
  if (typeof btn.show === 'function') {
    return row ? btn.show(row) : btn.show()
  }
  return true
}

/** 经权限过滤后的工具栏按钮列表 */
const visibleToolbarButtons = computed(() => props.toolbarButtons.filter((btn) => isButtonVisible(btn)))

/**
 * 获取指定行的可见操作按钮列表
 * @param {Object} row - 行数据
 * @returns {Array} 可见的操作按钮
 */
const getVisibleActionButtons = (row) => props.actionButtons.filter((btn) => isButtonVisible(btn, row))

/** 是否存在任何可见的操作按钮（仅检查权限，不依赖行数据） */
const hasAnyVisibleAction = computed(() => {
  if (props.actionButtons.length === 0) return false
  // 权限未加载时默认全部可见
  if (!permissionStore.permissionsLoaded) return true
  return props.actionButtons.some(btn => {
    if (btn.permission && !permissionStore.hasButtonPermission(btn.permission)) return false
    return true
  })
})

/** 表头筛选输入框的引用数组，用于自动聚焦 */
const filterInputRefs = ref([])

// ---- 内部数据加载模式 ----

/** 是否启用内部加载模式（传入了 load 函数） */
const isInternalMode = computed(() => !!props.load)

/** 内部加载状态 */
const innerLoading = ref(false)

/** 内部表格数据 */
const innerData = ref([])

/** 内部数据总数 */
const innerTotal = ref(0)

/** 实际使用的 loading 状态：内部模式用 innerLoading，外部模式用 props.loading */
const actualLoading = computed(() => isInternalMode.value ? innerLoading.value : props.loading)

/** 实际渲染的表格数据：内部模式用 innerData，外部模式用 props.data */
const tableData = computed(() => isInternalMode.value ? innerData.value : props.data)

/** 实际使用的 total：内部模式用 innerTotal，外部模式用 props.total */
const actualTotal = computed(() => isInternalMode.value ? innerTotal.value : props.total)

/** 实际使用的 stripe：有 rowStyle 时自动关闭，避免 td 背景色覆盖 tr 行样式 */
const effectiveStripe = computed(() => props.rowStyle ? false : props.stripe)

/** 当前排序条件列表 */
const sortConditions = ref([])

/**
 * 将单个筛选条件转换为后端 filters 数组条目
 * @param {Object} col - 列配置
 * @param {*} val - 筛选值
 * @returns {Array<{field: string, operator: string, value?: *}>} 过滤条件数组
 */
const buildFilterEntries = (col, val) => {
  const field = col.prop
  const ft = col.filterType
  // 优先使用列配置的 filterMatchMode，否则根据 filterType 推断默认运算符
  const matchMode = col.filterMatchMode

  // 无值运算符（notnull / isnull），不需要 value
  if (matchMode === 'notnull') return [{ field, operator: 'notnull' }]
  if (matchMode === 'isnull') return [{ field, operator: 'isnull' }]

  // 文本输入
  if (ft === 'input' || (!ft && !matchMode)) {
    if (!val) return []
    return [{ field, operator: matchMode || 'contains', value: val }]
  }

  // 下拉单选
  if (ft === 'select') {
    if (val === undefined || val === null || val === '') return []
    return [{ field, operator: matchMode || 'equals', value: val }]
  }

  // 下拉多选
  if (ft === 'multiple-select') {
    if (!Array.isArray(val) || val.length === 0) return []
    return [{ field, operator: matchMode || 'in', value: val }]
  }

  // 数值区间
  if (ft === 'number-range') {
    const entries = []
    if (val.min !== undefined && val.min !== null) {
      entries.push({ field, operator: 'gte', value: val.min })
    }
    if (val.max !== undefined && val.max !== null) {
      entries.push({ field, operator: 'lte', value: val.max })
    }
    return entries
  }

  // 日期范围
  if (ft === 'date-range' || ft === 'datetime-range') {
    if (!Array.isArray(val) || val.length !== 2) return []
    const entries = []
    if (val[0]) entries.push({ field, operator: 'gte', value: val[0] })
    if (val[1]) entries.push({ field, operator: 'lte', value: val[1] })
    return entries
  }

  return []
}

/**
 * 获取当前查询参数
 * @returns {Object} 包含 pageNum、pageSize、sortConditions、filters 和 extraParams 的查询参数对象
 */
const getQueryParams = () => {
  // 将 headerFilters 转换为后端 filters 数组格式
  const filters = []
  for (const [prop, val] of Object.entries(headerFilters)) {
    if (val === undefined || val === null) continue
    if (val === '') continue
    if (Array.isArray(val) && val.length === 0) continue
    if (typeof val === 'object' && !Array.isArray(val) && val.min === undefined && val.max === undefined) continue
    const col = props.columns.find((c) => c.prop === prop)
    if (!col) continue
    const entries = buildFilterEntries(col, val)
    filters.push(...entries)
  }
  return {
    pageNum: pageParam.value.pageNum,
    pageSize: pageParam.value.pageSize,
    sortConditions: sortConditions.value,
    filters,
    ...props.extraParams,
  }
}

/**
 * 执行数据加载
 * @param {Object} overrideParams - 覆盖参数（如重置 pageNum=1）
 */
const doLoad = async (overrideParams) => {
  if (!props.load) return
  const params = { ...getQueryParams(), ...overrideParams }

  // 触发 before-load 事件
  const beforeResult = emit('before-load', params)
  // 注意：emit 在 Vue 3 中不返回值，before-load 仅用于通知

  innerLoading.value = true
  try {
    const res = await props.load(params)
    const data = res?.data ?? res ?? []
    const total = res?.total ?? (Array.isArray(data) ? data.length : 0)
    innerData.value = Array.isArray(data) ? data : []
    innerTotal.value = total
    emit('after-load', { data: innerData.value, total })
  } catch (error) {
    console.error('[KhTable] 数据加载失败:', error)
    emit('load-error', error)
  } finally {
    innerLoading.value = false
  }
}

/**
 * 重置到第 1 页并重新加载数据
 */
const reload = () => {
  pageParam.value.pageNum = 1
  if (isInternalMode.value) {
    doLoad({ pageNum: 1 })
  } else {
    emit('update:pageNum', 1)
    emit('search')
  }
}

/**
 * 当前页重新加载数据（不重置页码）
 */
const refresh = () => {
  if (isInternalMode.value) {
    doLoad()
  } else {
    emit('refresh')
    emit('search')
  }
}

// ---- 自动加载 ----

onMounted(() => {
  if (isInternalMode.value && props.autoLoad) {
    doLoad()
  }
})

// ---- 列管理 ----

/** 内部列配置列表，每项扩展了 visible 属性用于控制列显隐 */
const columnList = ref([])

/**
 * 初始化列配置
 * @description 将 props.columns 深拷贝到内部 columnList，并默认设置 visible 为 true。
 *   在组件挂载时和 props.columns 变化时调用。
 */
const initColumns = () => {
  columnList.value = props.columns.map((col) => ({ ...col, visible: col.visible !== false }))
  // 预加载字典数据
  const dictTypes = collectDictTypes(props.columns)
  dictTypes.forEach(type => dictStore.getDict(type))
}
onMounted(initColumns)
watch(() => props.columns, initColumns, { deep: true })

/**
 * 计算属性：是否存在展开列（type='expand'）
 * @returns {boolean} 如果列配置中有 type='expand' 的列则返回 true
 */
const hasExpandColumn = computed(() => props.columns.some((c) => c.type === 'expand'))

/**
 * 计算属性：当前可见的数据列（排除 expand 列和 visible=false 的列）
 * @returns {Array} 过滤后的可见列配置数组
 */
const visibleColumns = computed(() => {
  return columnList.value
    .filter((c) => c.visible && c.type !== 'expand')
    .map(col => resolveColumn(col, dictStore.cache))
})

/**
 * 计算属性：是否存在可搜索的列
 * @returns {boolean} 如果任一列配置了 searchable=true 则返回 true
 */
const hasSearchableColumns = computed(() => props.columns.some((c) => c.searchable))

/** 计算属性：全选复选框的状态（getter 返回是否所有列都可见） */
const checkAll = computed({
  get: () => columnList.value.every((c) => c.visible),
  set: () => { }
})

/**
 * 计算属性：全选复选框的半选状态
 * @returns {boolean} 非全选但部分列可见时返回 true
 */
const isIndeterminate = computed(() => !checkAll.value && columnList.value.some((c) => c.visible))

/**
 * 全选/取消全选切换处理
 * @param {boolean} val - true 表示全选，false 表示取消全选
 */
const handleCheckAllChange = (val) => { columnList.value.forEach((c) => (c.visible = val)) }

// ---- 表头筛选 ----

/**
 * 筛选选项的动态加载状态存储。
 * key 为列 prop，value 为 { _options: [], _loading: false }。
 * 用于存储 filterOptions 为异步函数时加载的选项。
 */
const filterOptionsState = reactive({})

/**
 * 获取列的筛选下拉选项。
 * 按以下优先级：
 * 1. filterOptionsState 中异步加载的选项（API）
 * 2. col.filterOptions 静态数组或字典解析后的数组
 * 3. 从 col.tagMap / tagTypeMap 自动推导（与 tag 列绑定相同数据）
 * 4. 空数组
 */
const getFilterOptions = (col) => {
  const state = filterOptionsState[col.prop]
  if (state?._options?.length) return state._options
  if (Array.isArray(col.filterOptions) && col.filterOptions.length) return col.filterOptions
  // 从 tagMap / tagTypeMap 自动推导
  if (col.tagMap && typeof col.tagMap === 'object') {
    return Object.keys(col.tagMap).map((key) => ({
      label: getLabelForValue(col, key),
      value: isNaN(key) ? key : Number(key),
    }))
  }
  if (col.tagTypeMap && typeof col.tagTypeMap === 'object') {
    return Object.keys(col.tagTypeMap).map((key) => ({
      label: String(key),
      value: isNaN(key) ? key : Number(key),
    }))
  }
  return col.filterOptions || []
}

/**
 * 异步加载列的筛选选项（filterOptions 为函数时）
 * @param {Object} col - 列配置
 */
const loadFilterOptions = async (col) => {
  if (typeof col.filterOptions !== 'function') return
  const state = filterOptionsState[col.prop] || { _options: [], _loading: false }
  filterOptionsState[col.prop] = state
  state._loading = true
  state._options = []
  try {
    const options = await col.filterOptions()
    state._options = options || []
  } catch (e) {
    console.error(`[KhTable] filterOptions load error (${col.prop}):`, e)
    state._options = []
  } finally {
    state._loading = false
  }
}

/**
 * 获取列的筛选选项加载状态
 */
const isFilterOptionsLoading = (col) => filterOptionsState[col.prop]?._loading || false

/** 是否存在活跃的筛选条件 */
const hasActiveFilters = computed(() => {
  return props.columns.some((col) => col.searchable && isFilterActive(col))
})

/**
 * 清除所有表头筛选条件并重新加载数据
 */
const clearAllFilters = () => {
  props.columns.forEach((col) => {
    if (col.searchable) {
      headerFilters[col.prop] = getFilterDefaultValue(col)
    }
  })
  emit('header-filter', { ...headerFilters })
  if (isInternalMode.value) {
    doLoad({ pageNum: 1 })
  } else {
    emit('update:pageNum', 1)
    emit('search')
  }
}

/**
 * 根据筛选类型获取默认值
 * @param {Object} col - 列配置对象
 * @returns {*} 对应类型的默认筛选值
 */
const getFilterDefaultValue = (col) => {
  const ft = col.filterType
  if (ft === 'multiple-select') return []
  if (ft === 'number-range') return { min: undefined, max: undefined }
  if (ft === 'date-range' || ft === 'datetime-range') return null
  return '' // input / select
}

/**
 * 获取筛选弹出框的宽度
 * @param {Object} col - 列配置对象
 * @returns {number} 弹出框宽度(px)
 */
const getFilterPopoverWidth = (col) => {
  const ft = col.filterType
  if (ft === 'date-range' || ft === 'datetime-range') return 380
  if (ft === 'number-range') return 280
  if (ft === 'multiple-select') return 240
  return 200
}

/**
 * 判断列的筛选条件是否激活（有值）
 * @param {Object} col - 列配置对象
 * @returns {boolean} 是否有筛选值
 */
const isFilterActive = (col) => {
  const val = headerFilters[col.prop]
  if (val === undefined || val === null) return false
  const ft = col.filterType
  if (ft === 'multiple-select') return Array.isArray(val) && val.length > 0
  if (ft === 'number-range') return val.min !== undefined || val.max !== undefined
  if (ft === 'date-range' || ft === 'datetime-range') return Array.isArray(val) && val.length === 2
  return !!val // input / select
}

/** 响应式对象：存储各列的表头筛选条件，键为列 prop，值类型取决于 filterType */
const headerFilters = reactive({})

/** 当前正在操作的筛选列 prop，用于跟踪弹出框状态 */
const activeFilterProp = ref('')

/** 各列筛选弹出框的显隐状态，key 为列 prop，value 为 boolean */
const popoverVisibleMap = reactive({})

/**
 * 监听列配置变化，为新增的 searchable 列自动初始化筛选值
 */
watch(
  () => props.columns,
  (cols) => {
    cols.forEach((col) => {
      if (col.searchable && headerFilters[col.prop] === undefined) {
        headerFilters[col.prop] = getFilterDefaultValue(col)
      }
    })
  },
  { immediate: true }
)

/**
 * 筛选弹出框显示时的处理
 * @description 记录当前激活的筛选列 prop，并在下一帧自动聚焦输入框
 * @param {string} prop - 当前筛选列的字段名
 */
const onFilterPopoverShow = async (prop) => {
  activeFilterProp.value = prop
  const col = props.columns.find((c) => c.prop === prop)
  if (col && typeof col.filterOptions === 'function') {
    await loadFilterOptions(col)
  }
  nextTick(() => {
    const inputEl = document.querySelector('.kh-table__filter-popover .el-input__inner')
    if (inputEl) inputEl.focus()
  })
}

/**
 * 筛选弹出框隐藏时的处理
 * @description 清除当前激活的筛选列 prop
 * @param {string} prop - 当前筛选列的字段名
 */
const onFilterPopoverHide = (prop) => {
  activeFilterProp.value = ''
}

/**
 * 关闭当前筛选弹出框
 */
const closeFilterPopover = () => {
  if (activeFilterProp.value) {
    popoverVisibleMap[activeFilterProp.value] = false
  }
  activeFilterProp.value = ''
}

/**
 * 确认筛选
 * @description 将当前筛选条件通过 header-filter 事件传递给父组件，触发查询，然后关闭弹出框
 */
const confirmFilter = () => {
  emit('header-filter', { ...headerFilters })
  if (isInternalMode.value) {
    doLoad({ pageNum: 1 })
  } else {
    emit('update:pageNum', 1)
    emit('search')
  }
  closeFilterPopover()
}

/**
 * 重置单个列的筛选条件
 * @description 清除指定列的筛选值（恢复为该类型的默认值），并触发 header-filter 事件通知父组件
 * @param {string} prop - 需要重置的列字段名
 */
const clearSingleFilter = (prop) => {
  const col = props.columns.find((c) => c.prop === prop)
  headerFilters[prop] = getFilterDefaultValue(col || {})
  emit('header-filter', { ...headerFilters })
}

// ---- 序号 ----

/**
 * 序号列的自定义索引计算方法
 * @description 根据当前页码和每页条数计算全局序号（跨页连续编号）
 * @param {number} index - 当前行在当前页中的从 0 开始的索引
 * @returns {number} 全局序号（从 1 开始）
 */
const indexMethod = (index) => (props.pageNum - 1) * props.pageSize + index + 1

// ---- 工具函数 ----

/**
 * 获取行数据中指定列的值
 * @param {Object} col - 列配置对象
 * @param {Object} row - 行数据对象
 * @returns {*} 该列对应的值，如果列没有 prop 则返回空字符串
 */
const getColValue = (col, row) => (col.prop ? row[col.prop] : '')

/**
 * 获取行的内联样式
 * @param {Object} row - 行数据
 * @param {number} rowIndex - 行索引
 * @returns {Object} CSS 样式对象
 */
const getRowStyle = ({ row, rowIndex }) => {
  if (!props.rowStyle) return {}
  const result = props.rowStyle(row, rowIndex)
  // 支持返回预设名称字符串（如 'danger'、'success'）
  return resolveRowStyle(result)
}

/**
 * 获取单元格的内联样式（支持列配置的 cellStyle 函数）
 * @param {Object} col - 列配置
 * @param {Object} data - el-table 传递的单元格信息 { row, column, rowIndex, columnIndex }
 * @returns {Object} CSS 样式对象
 */
const getCellStyle = (col, data) => {
  if (typeof col.cellStyle === 'function') {
    return col.cellStyle(data.row, col, getColValue(col, data.row), data.rowIndex)
  }
  return {}
}

/**
 * 根据 tagTypeMap / tagMap 映射获取标签类型
 * @description 优先从 tagTypeMap 中查找（值为→标签类型映射），
 *   若无则回退到 tagMap（当 tagMap 值为合法标签类型时复用），
 *   未找到则返回空字符串（使用默认标签样式）
 * @param {Object} col - 列配置对象
 * @param {Object} row - 行数据对象
 * @returns {string} el-tag 的 type 属性值，如 'success' | 'warning' | 'danger' | 'info' | ''
 */
const validTagTypes = ['success', 'warning', 'danger', 'info', 'primary']
const getTagType = (col, row) => getTagColorForValue(col, getColValue(col, row))

/**
 * 获取标签显示文本
 * @description 当 tagMap 的值不是合法标签类型时，视为显示文本映射
 */
const getTagLabel = (col, row) => getLabelForValue(col, getColValue(col, row))

/**
 * 根据列配置获取某个值对应的标签颜色类型
 * @param {Object} col - 列配置（已解析字典引用）
 * @param {*} value - 选项值
 * @returns {string|undefined} 标签类型，如 'success' | 'warning' | 'danger' | 'info' | 'primary' | undefined
 */
const getTagColorForValue = (col, value) => {
  if (col.tagTypeMap && col.tagTypeMap[value] !== undefined) return col.tagTypeMap[value] || undefined
  if (col.tagMap && col.tagMap[value] !== undefined) {
    const mapped = col.tagMap[value]
    return validTagTypes.includes(mapped) ? mapped : undefined
  }
  return undefined
}

/**
 * 根据列配置获取某个值对应的显示文本
 * @param {Object} col - 列配置（已解析字典引用）
 * @param {*} value - 选项值
 * @returns {string} 显示文本
 */
const getLabelForValue = (col, value) => {
  if (col.tagMap && col.tagMap[value] !== undefined) {
    const mapped = col.tagMap[value]
    return validTagTypes.includes(mapped) ? String(value) : mapped
  }
  return String(value ?? '')
}

/**
 * 获取 tag-select 列的下拉选项列表
 * @description 优先使用显式指定的 options，其次从 tagMap / tagTypeMap 合成
 * @param {Object} col - 列配置（已解析字典引用）
 * @returns {Array<{label: string, value: *}>} 选项数组
 */
const getTagSelectOptions = (col) => {
  if (Array.isArray(col.options) && col.options.length > 0) return col.options
  if (col.tagMap && typeof col.tagMap === 'object') {
    return Object.keys(col.tagMap).map((key) => ({
      label: getLabelForValue(col, key),
      value: isNaN(key) ? key : Number(key),
    }))
  }
  if (col.tagTypeMap && typeof col.tagTypeMap === 'object') {
    return Object.keys(col.tagTypeMap).map((key) => ({
      label: String(key),
      value: isNaN(key) ? key : Number(key),
    }))
  }
  return []
}

/**
 * 判断列是否具有标签颜色映射（用于筛选下拉是否显示颜色点）
 * @param {Object} col - 列配置
 * @returns {boolean}
 */
const hasColumnTagColors = (col) => !!(col.tagTypeMap || (col.tagMap && Object.values(col.tagMap).some((v) => validTagTypes.includes(v))))

// ---- 事件处理 ----

/**
 * 处理选中行变化事件
 * @param {Array} s - 当前所有被选中的行数据数组
 */
const handleSelectionChange = (s) => {
  selectionCount.value = s.length
  emit('selection-change', s)
}

/**
 * 处理排序变化事件
 * @param {Object} param - 排序信息对象
 * @param {Object} param.column - 排序列的配置
 * @param {string} param.prop   - 排序字段的 prop 名
 * @param {string} param.order  - 排序方向: 'ascending' | 'descending' | null
 */
const handleSortChange = ({ column, prop, order }) => {
  emit('sort-change', { column, prop, order })
  // 内部模式下维护排序条件并重新加载数据
  if (isInternalMode.value) {
    if (prop && order) {
      const direction = order === 'ascending' ? 'asc' : 'desc'
      // 同一字段只保留最新排序
      sortConditions.value = sortConditions.value.filter(s => s.field !== prop)
      sortConditions.value.push({ field: prop, direction })
    } else {
      sortConditions.value = []
    }
    doLoad()
  }
}

/**
 * 处理当前高亮行变化事件
 * @param {Object} r - 新的高亮行数据
 */
const handleCurrentChange = (r) => emit('current-change', r)

/**
 * 处理行单击事件
 * @param {Object} r - 被点击的行数据
 */
const handleRowClick = (row) => {
  if (props.showSelection) {
    tableRef.value?.toggleRowSelection(row)
  }
  emit('row-click', row)
}

/**
 * 处理行双击事件
 * @param {Object} r - 被双击的行数据
 */
const handleRowDblclick = (r) => emit('row-dblclick', r)

/**
 * 处理单元格内可编辑组件（如 switch）的值变化事件
 * @param {Object} col - 列配置对象
 * @param {Object} row - 行数据对象
 * @param {*}      val - 变化后的新值
 */
const handleCellChange = (col, row, val) => emit('cell-change', col.prop, row, val)

// ---- 分页 ----

/** 分页参数的内部响应式状态，用于 v-model 双向绑定 el-pagination */
const pageParam = ref({ pageNum: props.pageNum, pageSize: props.pageSize })

/** 监听外部 pageNum 变化，同步更新内部状态 */
watch(() => props.pageNum, (v) => { pageParam.value.pageNum = v })

/** 监听外部 pageSize 变化，同步更新内部状态 */
watch(() => props.pageSize, (v) => { pageParam.value.pageSize = v })

/**
 * 处理每页条数变化事件
 * @description 重置页码为第 1 页，更新 pageSize 和 pageNum，并触发 search 事件
 * @param {number} val - 新的每页条数
 */
const handleSizeChange = (val) => {
  pageParam.value.pageNum = 1
  pageParam.value.pageSize = val
  if (isInternalMode.value) {
    doLoad({ pageNum: 1, pageSize: val })
  } else {
    emit('update:pageSize', val)
    emit('update:pageNum', 1)
    emit('search')
  }
}

/**
 * 处理页码变化事件
 * @description 更新 pageNum 并触发 search 事件
 * @param {number} val - 新的页码
 */
const handlePageChange = (val) => {
  pageParam.value.pageNum = val
  if (isInternalMode.value) {
    doLoad({ pageNum: val })
  } else {
    emit('update:pageNum', val)
    emit('search')
  }
}

// ---- 工具栏 ----

/**
 * 处理刷新按钮点击事件
 * @description 内部模式调用 refresh()，外部模式触发 refresh 和 search 事件
 */
const handleRefresh = () => refresh()

/**
 * 暴露给父组件的方法和属性
 * @description 通过 defineExpose 暴露表格操作方法和内部状态，
 *   父组件可通过 ref 调用这些方法操控表格
 */
defineExpose({
  /** el-table 组件实例引用 */
  tableRef,

  /** 重置到第 1 页并重新加载数据 */
  reload,
  /** 当前页重新加载数据（不重置页码） */
  refresh,
  /** 获取当前查询参数 */
  getQueryParams,

  /**
   * 获取当前所有选中的行数据
   * @returns {Array} 选中的行数据数组，如果没有选中则返回空数组
   */
  getSelectionRows: () => tableRef.value?.getSelectionRows() || [],

  /**
   * 清除所有行的选中状态
   * @returns {void}
   */
  clearSelection: () => { tableRef.value?.clearSelection(); selectionCount.value = 0 },

  /**
   * 切换某一行的选中状态
   * @param {Object}  row      - 目标行数据对象
   * @param {Boolean} selected - 是否选中，true 选中，false 取消选中
   * @returns {void}
   */
  toggleRowSelection: (row, selected) => tableRef.value?.toggleRowSelection(row, selected),

  /**
   * 清除表格的所有排序状态
   * @returns {void}
   */
  clearSort: () => tableRef.value?.clearSort(),

  /**
   * 手动触发列排序
   * @param {string} prop  - 需要排序的列字段名
   * @param {string} order - 排序方向: 'ascending'（升序） | 'descending'（降序） | null（取消排序）
   * @returns {void}
   */
  sort: (prop, order) => tableRef.value?.sort(prop, order),

  /**
   * 设置某一行数据为当前高亮行
   * @param {Object} row - 需要高亮的目标行数据
   * @returns {void}
   */
  setCurrentRow: (row) => tableRef.value?.setCurrentRow(row),

  /**
   * 清除指定列的筛选状态（el-table 内置筛选，非表头筛选）
   * @param {Array} [columnKeys] - 需要清除筛选的列 prop 数组，不传则清除所有列的筛选
   * @returns {void}
   */
  clearFilter: (columnKeys) => tableRef.value?.clearFilter(columnKeys),

  /** 列配置列表（响应式），包含每列的 visible 属性，可用于外部读取或修改列的可见性 */
  columnList,

  /** 当前表头筛选条件对象（响应式），键为列 prop，值类型取决于对应列的 filterType */
  headerFilters,

  /**
   * 清除所有表头筛选条件
   * @description 将所有可搜索列的筛选值重置为对应类型的默认值，并触发 header-filter 事件通知父组件
   * @returns {void}
   */
  clearHeaderFilters: () => {
    props.columns.forEach((col) => {
      if (col.searchable) {
        headerFilters[col.prop] = getFilterDefaultValue(col)
      }
    })
    emit('header-filter', { ...headerFilters })
  },
})
</script>

<style scoped>
.kh-table__toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  margin-bottom: 12px;
}

.kh-table__toolbar-left {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.kh-table__title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  white-space: nowrap;
  flex-shrink: 0;
}

.kh-table__toolbar-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-left: auto;
}

.kh-table__toolbar-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.kh-table__selection-info {
  font-size: 13px;
  color: #606266;
}

.kh-table__selection-info strong {
  color: #409eff;
}

.kh-table__pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 0 4px;
}

.kh-table__column-setting {
  max-height: 400px;
  overflow-y: auto;
}

.kh-table__column-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.kh-table__column-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  border-radius: 4px;
  transition: background 0.2s;
}

.kh-table__column-item:hover {
  background: #f5f7fa;
}

.kh-table__column-item .drag-handle {
  cursor: move;
  color: #c0c4cc;
}

.kh-table__column-item .drag-handle:hover {
  color: #409eff;
}

.kh-table__empty {
  padding: 20px 0;
}

/* 列标题 + 筛选图标 */
.kh-table__col-header {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: default;
}

.kh-table__col-label {
  white-space: nowrap;
}

.kh-table__filter-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border-radius: 4px;
  color: #b1b3b8;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
}

.kh-table__filter-icon:hover {
  color: #409eff;
  background-color: rgba(64, 158, 255, 0.1);
}

.kh-table__filter-icon.is-active {
  color: #409eff;
}

/* tag-select 可编辑下拉列 */
.kh-table__tag-select {
  width: 100%;
}

.kh-table__tag-select-option {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
</style>

<style>
/* 筛选弹出框样式 (不能 scoped，因为 popover 渲染在 body 下) */
.kh-table__filter-popover {
  padding: 0 !important;
  min-width: 0 !important;
  overflow: visible !important;
}

/* 日期选择器面板在 popover 内渲染时，确保不被裁剪且层级正确 */
.kh-table__filter-popover .el-popper,
.kh-table__filter-popover .el-date-picker {
  overflow: visible !important;
}

.kh-table__filter-dropdown {
  padding: 4px;
}

.kh-table__filter-dropdown-title {
  font-size: 12px;
  color: #909399;
  padding: 4px 8px 8px;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 8px;
}

.kh-table__filter-dropdown .el-input {
  width: 100%;
}

.kh-table__filter-dropdown .el-input .el-input__wrapper {
  border-radius: 6px;
}

/* 下拉选择筛选 */
.kh-table__filter-select {
  width: 100%;
}

/* 数值区间筛选 */
.kh-table__filter-number-range {
  display: flex;
  align-items: center;
  gap: 4px;
}

.kh-table__filter-number-input {
  flex: 1;
  min-width: 0;
}

.kh-table__filter-number-input .el-input__wrapper {
  padding-left: 8px;
}

.kh-table__filter-number-sep {
  color: #909399;
  font-size: 12px;
  flex-shrink: 0;
}

/* 日期范围筛选 */
.kh-table__filter-date {
  width: 100%;
}

.kh-table__filter-dropdown-footer {
  display: flex;
  justify-content: flex-end;
  gap: 4px;
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid #f0f0f0;
}

/* 颜色圆点 (el-select 下拉选项和筛选弹出框中，需要非 scoped) */
.kh-table__color-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: var(--el-color-info);
  flex-shrink: 0;
}

.kh-table__color-dot.is-success {
  background-color: var(--el-color-success);
}

.kh-table__color-dot.is-warning {
  background-color: var(--el-color-warning);
}

.kh-table__color-dot.is-danger {
  background-color: var(--el-color-danger);
}

.kh-table__color-dot.is-info {
  background-color: var(--el-color-info);
}

.kh-table__color-dot.is-primary {
  background-color: var(--el-color-primary);
}
</style>
