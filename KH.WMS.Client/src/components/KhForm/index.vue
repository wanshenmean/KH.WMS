<template>
  <div class="kh-form">
    <!-- 收起态：单行快速搜索 -->
    <div v-if="collapsible && collapsed" class="kh-form__quick">
      <el-input v-model="quickSearchValue" :placeholder="quickSearchPlaceholderText" clearable
        @keyup.enter="handleQuickSearch">
        <template #append>
          <el-button :icon="SearchIcon" @click="handleQuickSearch" />
        </template>
      </el-input>
      <el-button link type="primary" class="kh-form__toggle-btn" @click="collapsed = false">
        展开 <el-icon>
          <ArrowDown />
        </el-icon>
      </el-button>
    </div>

    <!-- 展开态 / 不可折叠：完整表单 -->
    <el-form v-else ref="formRef" :model="formData" :rules="formRules" :label-width="labelWidth"
      :label-position="labelPosition" :inline="inline" :size="size" :disabled="disabled"
      :validate-on-rule-change="false" v-bind="$attrs">
      <el-row :gutter="gutter">
        <el-col v-for="item in resolvedColumns" v-show="item.hidden !== true" :key="item.prop"
          :span="item.span || (item.type === 'buttons' ? buttonsSpan : 24 / colCount)">
          <el-form-item :label="item.label" :prop="item.prop" :label-width="item.labelWidth ? `${item.labelWidth}px` : labelWidth"
            :class="{ 'el-form-item--buttons': item.type === 'buttons' }">
            <!-- 输入框 -->
            <el-input v-if="!item.type || item.type === 'input'" v-model="formData[item.prop]"
              :placeholder="item.placeholder || `请输入${item.label}`" :clearable="item.clearable !== false"
              :disabled="item.disabled" :maxlength="item.maxlength" :show-word-limit="!!item.maxlength"
              :type="item.inputType" :rows="item.rows" v-bind="item.bindProps">
              <template v-if="item.prefix" #prefix>
                <el-icon>
                  <component :is="item.prefix" />
                </el-icon>
              </template>
              <template v-if="item.suffix" #suffix>
                <el-icon>
                  <component :is="item.suffix" />
                </el-icon>
              </template>
              <template v-if="item.prepend" #prepend>{{ item.prepend }}</template>
              <template v-if="item.append" #append>{{ item.append }}</template>
            </el-input>

            <!-- 文本域 -->
            <el-input v-else-if="item.type === 'textarea'" v-model="formData[item.prop]" type="textarea"
              :placeholder="item.placeholder || `请输入${item.label}`" :clearable="item.clearable !== false"
              :disabled="item.disabled" :maxlength="item.maxlength" :show-word-limit="!!item.maxlength"
              :rows="item.rows || 3" v-bind="item.bindProps" />

            <!-- 数字输入框 -->
            <el-input-number v-else-if="item.type === 'number'" v-model="formData[item.prop]"
              :placeholder="item.placeholder || `请输入${item.label}`" :disabled="item.disabled" :min="item.min"
              :max="item.max" :step="item.step || 1" :precision="item.precision" :controls="item.controls !== false"
              v-bind="item.bindProps" />

            <!-- 选择器 -->
            <el-select v-else-if="item.type === 'select'" v-model="formData[item.prop]"
              :placeholder="item.placeholder || `请选择${item.label}`" :clearable="item.clearable !== false"
              :disabled="item.disabled" :multiple="item.multiple" :filterable="item.filterable" :remote="item.remote"
              :remote-method="item.remoteMethod" :loading="item.loading" v-bind="item.bindProps"
              @change="(val) => handleChange(item, val)">
              <el-option v-for="opt in (item.options || [])" :key="opt.value ? opt.value : opt"
                :label="opt.label ? opt.label : opt" :value="opt.value ? opt.value : opt" :disabled="opt.disabled">
                <template v-if="hasItemTagColors(item)" #default>
                  <span class="kh-form__select-option">
                    <span v-if="getTagColorForValue(item, opt.value ? opt.value : opt)"
                      class="kh-form__color-dot" :class="'is-' + getTagColorForValue(item, opt.value ? opt.value : opt)" />
                    <span>{{ opt.label ? opt.label : opt }}</span>
                  </span>
                </template>
              </el-option>
            </el-select>

            <!-- 远程数据选择器 -->
            <el-select v-else-if="item.type === 'remote-select'" v-model="formData[item.prop]"
              :placeholder="item.placeholder || `请选择${item.label}`" :clearable="item.clearable !== false"
              :disabled="item.disabled" :multiple="item.multiple" :filterable="true" :remote="true"
              :remote-method="(query) => handleRemoteSearch(item, query)" :loading="item._loading || false"
              v-bind="item.bindProps" @change="(val) => handleChange(item, val)">
              <el-option v-for="opt in (item.options || [])" :key="opt.value ? opt.value : opt"
                :label="opt.label ? opt.label : opt" :value="opt.value ? opt.value : opt">
                <template v-if="hasItemTagColors(item)" #default>
                  <span class="kh-form__select-option">
                    <span v-if="getTagColorForValue(item, opt.value ? opt.value : opt)"
                      class="kh-form__color-dot" :class="'is-' + getTagColorForValue(item, opt.value ? opt.value : opt)" />
                    <span>{{ opt.label ? opt.label : opt }}</span>
                  </span>
                </template>
              </el-option>
            </el-select>

            <!-- 日期选择器 -->
            <el-date-picker v-else-if="item.type === 'date'" v-model="formData[item.prop]"
              :type="item.dateType || 'date'" :placeholder="item.placeholder || `请选择${item.label}`"
              :clearable="item.clearable !== false" :disabled="item.disabled" :format="item.format || 'YYYY-MM-DD'"
              :value-format="item.valueFormat || 'YYYY-MM-DD'" :start-placeholder="item.startPlaceholder"
              :end-placeholder="item.endPlaceholder" v-bind="item.bindProps"
              @change="(val) => handleChange(item, val)" />

            <!-- 开关 -->
            <el-switch v-else-if="item.type === 'switch'" v-model="formData[item.prop]" :disabled="item.disabled"
              :active-text="item.activeText" :inactive-text="item.inactiveText" :active-value="item.activeValue"
              :inactive-value="item.inactiveValue" v-bind="item.bindProps" />

            <!-- 单选 -->
            <el-radio-group v-else-if="item.type === 'radio'" v-model="formData[item.prop]" :disabled="item.disabled"
              v-bind="item.bindProps" @change="(val) => handleChange(item, val)">
              <template v-if="item.isButton">
                <el-radio-button v-for="opt in item.options" :key="opt.value" :value="opt.value"
                  :disabled="opt.disabled">
                  {{ opt.label }}
                </el-radio-button>
              </template>
              <template v-else>
                <el-radio v-for="opt in item.options" :key="opt.value" :value="opt.value" :disabled="opt.disabled">
                  {{ opt.label }}
                </el-radio>
              </template>
            </el-radio-group>

            <!-- 多选框 -->
            <el-checkbox-group v-else-if="item.type === 'checkbox'" v-model="formData[item.prop]"
              :disabled="item.disabled" v-bind="item.bindProps" @change="(val) => handleChange(item, val)">
              <template v-if="item.isButton">
                <el-checkbox-button v-for="opt in item.options" :key="opt.value" :value="opt.value"
                  :disabled="opt.disabled">
                  {{ opt.label }}
                </el-checkbox-button>
              </template>
              <template v-else>
                <el-checkbox v-for="opt in item.options" :key="opt.value" :value="opt.value" :disabled="opt.disabled">
                  {{ opt.label }}
                </el-checkbox>
              </template>
            </el-checkbox-group>

            <!-- 级联选择器 (Element) -->
            <el-cascader v-else-if="item.type === 'cascader'" v-model="formData[item.prop]" :options="item.options"
              :placeholder="item.placeholder || `请选择${item.label}`" :clearable="item.clearable !== false"
              :disabled="item.disabled" :filterable="item.filterable" :props="item.cascaderProps"
              v-bind="item.bindProps" @change="(val) => handleChange(item, val)" />

            <!-- 多级联动选择器 (多个独立下拉框) -->
            <div v-else-if="item.type === 'cascade-select'" class="kh-form__cascade-select">
              <el-select v-for="level in item.cascadeItems" :key="level.prop" v-model="formData[level.prop]"
                :placeholder="level.placeholder || `请选择${level.label}`" :clearable="level.clearable !== false"
                :disabled="item.disabled || level.disabled || (level.parentProp && !formData[level.parentProp]) || getCascadeLevelLoading(item, level)"
                :loading="getCascadeLevelLoading(item, level)"
                :filterable="level.filterable" v-bind="level.bindProps"
                @change="(val) => handleCascadeChange(item, level, val)">
                <el-option v-for="opt in getCascadeOptions(item, level)" :key="opt.value" :label="opt.label"
                  :value="opt.value" :disabled="opt.disabled" />
              </el-select>
            </div>

            <!-- 颜色选择器 -->
            <KhColorPicker
              v-else-if="item.type === 'color-picker'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              :show-alpha="item.showAlpha"
              :predefine="item.predefine"
              :placeholder="item.placeholder"
              @change="(val) => handleChange(item, val)"
            />

            <!-- 图标选择器 -->
            <KhIconPicker
              v-else-if="item.type === 'icon-picker'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              :placeholder="item.placeholder || '请选择图标'"
              @change="(val) => handleChange(item, val)"
            />

            <!-- 插槽 -->
            <slot v-else-if="item.type === 'slot'" :name="item.prop" :model="formData" :item="item" />

            <!-- 按钮组(用于查询表单底部) -->
            <template v-else-if="item.type === 'buttons'">
              <el-button type="primary" :icon="SearchIcon" @click="handleSearch">查询</el-button>
              <el-button :icon="RefreshIcon" @click="handleReset">重置</el-button>
              <slot name="extra-buttons" />
            </template>
          </el-form-item>
        </el-col>
      </el-row>

      <!-- 表单底部按钮 -->
      <el-form-item v-if="showFooter">
        <slot name="footer">
          <el-button type="primary" @click="handleSubmit">确 定</el-button>
          <el-button @click="handleCancel">取 消</el-button>
        </slot>
      </el-form-item>
    </el-form>

    <!-- 收起/展开按钮 (表单下方) -->
    <div v-if="collapsible && !collapsed" class="kh-form__toggle-bar">
      <el-button link type="primary" @click="collapsed = true">
        收起 <el-icon>
          <ArrowUp />
        </el-icon>
      </el-button>
    </div>
  </div>
</template>

<script setup>
/**
 * @file KhForm - 通用表单组件
 * @description 基于 Element Plus 的通用表单组件，支持多种表单项类型、表单验证、
 *   可折叠查询模式、多级联动选择等功能。适用于新增/编辑对话框表单、列表页查询条件表单等场景。
 *   支持通过 columns 配置数组快速生成表单，无需手写大量模板代码。
 *
 * @component KhForm
 *
 * @props
 * @prop {Array<ColumnItem>} columns - 表单列配置数组（必填）。定义表单中所有字段的类型、校验规则、默认值等。
 *   默认值: []。详见下方 ColumnItem 类型说明。
 * @prop {Object} modelValue - 表单数据对象（v-model 双向绑定）。默认值: {}。
 *   外部可通过 v-model 绑定表单数据，内部修改会自动同步到父组件。
 * @prop {string} labelWidth - 表单标签宽度。默认值: '100px'。
 *   对应 el-form 的 label-width 属性。
 * @prop {string} labelPosition - 表单标签位置。默认值: 'right'。
 *   可选值: 'left' | 'right' | 'top'，对应 el-form 的 label-position 属性。
 * @prop {boolean} inline - 是否为行内表单。默认值: false。
 *   设为 true 时，所有表单项排在一行内。
 * @prop {number} colCount - 非行内模式下每行显示的表单项列数。默认值: 1。
 *   例如设为 2 表示每行两列，设为 3 表示每行三列。
 * @prop {number} gutter - 表单项之间的间距（像素）。默认值: 20。
 *   对应 el-row 的 gutter 属性。
 * @prop {string} size - 表单尺寸。默认值: 'default'。
 *   可选值: 'large' | 'default' | 'small'，对应 el-form 的 size 属性。
 * @prop {boolean} disabled - 是否禁用整个表单。默认值: false。
 *   对应 el-form 的 disabled 属性。也可在单个 column 项中设置 disabled 来单独禁用某个字段。
 * @prop {boolean} showFooter - 是否显示表单底部按钮区域。默认值: false。
 *   设为 true 时，在表单底部显示"确定"和"取消"按钮，可通过 footer 插槽自定义。
 * @prop {boolean} collapsible - 是否启用折叠功能（适用于查询表单场景）。默认值: false。
 *   启用后，收起态仅显示一个快速搜索输入框，展开后显示完整表单。
 * @prop {boolean} defaultCollapsed - 默认是否处于收起状态。默认值: false。
 *   仅在 collapsible 为 true 时生效。
 * @prop {string} quickSearchPlaceholder - 收起态快速搜索输入框的占位文字。默认值: '请输入关键字搜索'。
 *   仅在 collapsible 为 true 时生效。
 *
 * @events
 * @event update:modelValue - 表单数据变化时触发（v-model 更新事件）。
 *   @param {Object} formData - 当前表单的所有字段数据（浅拷贝）。
 * @event search - 点击"查询"按钮或收起态快速搜索时触发。
 *   @param {Object} formData - 当前表单的所有字段数据（浅拷贝）。
 * @event reset - 点击"重置"按钮时触发。无参数。
 * @event submit - 点击"确定"按钮且表单验证通过后触发。
 *   @param {Object} formData - 当前表单的所有字段数据（浅拷贝）。
 * @event cancel - 点击"取消"按钮时触发。无参数。
 * @event change - 表单字段值发生变化时触发。
 *   @param {string} prop - 发生变化的字段名。
 *   @param {*} value - 字段的新值。
 *   @param {Object} formData - 当前表单的所有字段数据。
 *
 * @expose
 * @exposed {import('vue').Ref<typeof import('element-plus')['ElForm']>} formRef - el-form 组件的模板引用。
 *   可用于直接操作 Element Plus 表单实例。
 * @exposed {Object} formData - 表单数据的响应式对象（reactive）。
 *   可直接读写表单字段值。
 * @exposed {Function} validate - 手动触发表单验证。
 *   @returns {Promise<boolean>} 验证通过返回 true，否则返回 false。
 * @exposed {Function} resetFields - 重置所有表单字段为初始值并清除校验结果。
 * @exposed {Function} clearValidate - 清除指定字段或所有字段的校验结果。
 *   @param {string|Array<string>} [props] - 要清除校验的字段名或字段名数组，不传则清除全部。
 * @exposed {Function} initFormData - 根据当前 columns 配置重新初始化表单数据。
 *   会根据 defaultValue 或字段类型设置初始值，并合并 modelValue 中的值。
 *
 * @slots
 * @slot [column.prop] - 自定义插槽。当 column 项的 type 为 'slot' 时，使用其 prop 值作为插槽名称。
 *   @binding {Object} model - 当前表单数据对象。
 *   @binding {Object} item - 当前列的配置项。
 * @slot footer - 表单底部按钮区域插槽。当 showFooter 为 true 时显示。
 *   默认内容为"确定"和"取消"按钮，可通过此插槽完全自定义。
 * @slot extra-buttons - 额外按钮插槽。位于 type='buttons' 的"查询"和"重置"按钮之后。
 *   用于在查询表单中添加额外的操作按钮（如"导出"、"高级搜索"等）。
 *
 * @typedef {Object} ColumnItem - 表单列配置项
 *
 * --- 通用属性（所有类型可用）---
 * @property {string} prop - 字段名，对应 formData 中的键名，也是表单校验的 prop 标识。（必填）
 * @property {string} label - 表单项标签文本，显示在输入框左侧。（必填）
 * @property {string} [type] - 表单项类型。默认为 'input'（不传等同于 'input'）。
 *   可选值: 'input' | 'textarea' | 'number' | 'select' | 'remote-select' | 'date' |
 *   'switch' | 'radio' | 'checkbox' | 'cascader' | 'cascade-select' |
 *   'color-picker' | 'icon-picker' | 'slot' | 'buttons'
 * @property {number} [span] - 该字段在 el-col 中占的栅格数（0-24）。默认自动计算: 24 / colCount。
 *   例如 colCount=3 时，默认 span=8。
 * @property {boolean} [required] - 是否必填。默认 false。设为 true 时自动生成必填校验规则。
 * @property {string} [requiredMessage] - 自定义必填校验失败时的提示信息。
 *   默认根据字段类型自动生成（输入框: "请输入xxx"，选择器/日期: "请选择xxx"）。
 * @property {Array<Object>} [rules] - 自定义校验规则数组。设置后将覆盖 required 自动生成的规则。
 *   规则格式同 Element Plus FormRule。
 * @property {string} [trigger] - 校验触发时机。默认 'blur'。可选值: 'blur' | 'change'。
 *   仅在 required=true 且未设置 rules 时生效。
 * @property {string} [placeholder] - 输入框/选择器的占位提示文字。默认根据 label 自动生成。
 * @property {boolean} [disabled] - 是否禁用该字段。默认 false（继承表单级 disabled）。
 * @property {boolean} [clearable] - 是否可清空。默认 true（除 number/switch 外）。
 * @property {*} [defaultValue] - 字段默认值。不传时根据类型自动设置：
 *   checkbox/multiple: []，其他类型: ''。
 * @property {Function} [onChange] - 字段值变化时的回调函数。
 *   @param {*} value - 字段新值。
 *   @param {Object} formData - 当前表单数据。
 * @property {Object} [bindProps] - 透传给底层 Element Plus 组件的额外属性。
 *   例如: { style: { width: '100%' }, class: 'custom-class' }。
 *
 * --- input 类型专属属性---
 * @property {string} [inputType] - el-input 的 type 属性。例如: 'password'。
 * @property {number} [maxlength] - 最大输入字符数。设置后自动显示字数统计。
 * @property {number} [rows] - 当 inputType 为 textarea 时的行数。
 * @property {import('vue').Component} [prefix] - 输入框前缀图标组件。
 * @property {import('vue').Component} [suffix] - 输入框后缀图标组件。
 * @property {string} [prepend] - 输入框前置内容文本。
 * @property {string} [append] - 输入框后置内容文本。
 *
 * --- textarea 类型专属属性---
 * @property {number} [rows] - 文本域行数。默认 3。
 * @property {number} [maxlength] - 最大输入字符数。设置后自动显示字数统计。
 *
 * --- number 类型专属属性---
 * @property {number} [min] - 最小值。
 * @property {number} [max] - 最大值。
 * @property {number} [step] - 步进值。默认 1。
 * @property {number} [precision] - 数值精度（小数位数）。
 * @property {boolean} [controls] - 是否显示增减按钮。默认 true。
 *
 * --- select / remote-select 类型专属属性---
 * @property {Array<{label: string, value: *, disabled?: boolean}>} [options] - 下拉选项列表。支持字典引用 'dict:xxx'。
 * @property {boolean} [multiple] - 是否多选。默认 false。
 * @property {boolean} [filterable] - 是否可搜索。默认 false（remote-select 强制为 true）。
 * @property {Object} [tagMap] - 值到显示文本的映射，支持字典引用 'dict:xxx'。
 *   当配置了 tagMap 或 tagTypeMap 时，下拉选项前会自动显示颜色圆点标识。
 * @property {Object} [tagTypeMap] - 值到标签颜色类型的映射，支持字典引用 'dict:xxx'。
 *   可选值: 'success' | 'warning' | 'danger' | 'info' | 'primary'。
 *
 * --- remote-select 类型专属属性---
 * @property {Function} [remoteMethod] - 远程搜索方法。
 *   @param {string} query - 搜索关键字。
 *   @param {ColumnItem} item - 当前列配置项。
 *   @returns {Promise<void>} 异步搜索完成后需将结果赋值到 item.options。
 * @property {boolean} [_loading] - （内部状态）远程搜索加载状态，由组件自动管理。
 *
 * --- date 类型专属属性---
 * @property {string} [dateType] - 日期选择器类型。默认 'date'。
 *   可选值: 'date' | 'datetime' | 'daterange' | 'datetimerange' | 'month' | 'year' | 'week' 等。
 * @property {string} [format] - 显示格式。默认 'YYYY-MM-DD'。
 * @property {string} [valueFormat] - 绑定值的格式。默认 'YYYY-MM-DD'。
 * @property {string} [startPlaceholder] - 范围选择时开始日期的占位文字。
 * @property {string} [endPlaceholder] - 范围选择时结束日期的占位文字。
 *
 * --- switch 类型专属属性---
 * @property {string} [activeText] - 开关打开时的文字描述。
 * @property {string} [inactiveText] - 开关关闭时的文字描述。
 * @property {*} [activeValue] - 开关打开时的值。默认 true。
 * @property {*} [inactiveValue] - 开关关闭时的值。默认 false。
 *
 * --- radio / checkbox 类型专属属性---
 * @property {Array<{label: string, value: *, disabled?: boolean}>} [options] - 单选/多选选项列表。
 * @property {boolean} [isButton] - 是否使用按钮样式。默认 false（使用圆点/方块样式）。
 *
 * --- cascader 类型专属属性---
 * @property {Array<Object>} [options] - 级联选择器选项数据（树形结构）。
 * @property {Object} [cascaderProps] - 级联选择器的配置项。对应 el-cascader 的 props 属性。
 *   例如: { value: 'id', label: 'name', children: 'children', multiple: true }。
 * @property {boolean} [filterable] - 是否可搜索。默认 false。
 *
 * --- cascade-select 类型专属属性---
 * @property {Array<CascadeLevelItem>} [cascadeItems] - 多级联动各级下拉框的配置数组。
 *   每一级是一个独立的 select，按数组顺序从左到右排列。
 *
 * @typedef {Object} CascadeLevelItem - 多级联动中每一级的配置项
 * @property {string} prop - 该级字段名，对应 formData 中的键名。
 * @property {string} label - 该级标签文本。
 * @property {string} [placeholder] - 该级选择器的占位文字。默认根据 label 自动生成。
 * @property {string} [parentProp] - 父级字段名。用于建立级联关系，当父级值变化时清空下级。
 * @property {Object} [optionsMap] - 选项映射表。key 为父级值，value 为该级选项数组。
 *   格式: { [parentValue]: Array<{label, value, disabled?}> }。
 * @property {Function} [optionsFn] - 动态获取选项的同步函数。优先级低于 loadOptions，高于 options 和 optionsMap。
 *   @param {Object} formData - 当前表单数据。
 *   @param {Array<CascadeLevelItem>} cascadeItems - 所有级联配置项。
 *   @returns {Array<{label, value, disabled?}>} 该级选项数组。
 * @property {Function} [loadOptions] - 异步加载选项的函数。优先级最高，当父级值变化时自动调用。
 *   适用于需要通过 API 获取下级选项的场景。加载期间该级选择器自动显示 loading 并禁用。
 *   @param {*} parentValue - 父级字段的选中值。
 *   @param {Object} formData - 当前表单数据。
 *   @param {CascadeLevelItem} level - 当前级别的配置项。
 *   @returns {Promise<Array<{label, value, disabled?}>>} 该级选项数组。
 * @property {Array<{label: string, value: *, disabled?: boolean}>} [options] - 静态选项列表。
 * @property {boolean} [disabled] - 是否禁用该级选择器。
 * @property {boolean} [clearable] - 是否可清空。默认 true。
 * @property {boolean} [filterable] - 是否可搜索。默认 false。
 * @property {Object} [bindProps] - 透传给 el-select 的额外属性。
 *
 * --- color-picker 类型专属属性---
 * @property {boolean} [showAlpha] - 是否支持透明度选择。默认 true。
 * @property {Array<string>} [predefine] - 预设颜色数组。默认内置 14 种常用颜色。
 *
 * --- icon-picker 类型专属属性---
 * @property {string} [placeholder] - 图标选择器的占位文字。默认 '请选择图标'。
 */

import KhColorPicker from '@/components/KhColorPicker/index.vue'
import KhIconPicker from '@/components/KhIconPicker/index.vue'
import { useDictStore } from '@/stores/dict'
import { resolveColumn, collectDictTypes } from '@/utils/dict-resolve'

// 图标引用（供模板 :icon 绑定使用，需在 script 中声明才能暴露到 $setup）
const SearchIcon = Search
const RefreshIcon = Refresh

const dictStore = useDictStore()

// ======================== Props 定义 ========================

const props = defineProps({
  /** 表单列配置数组，每一项定义一个表单字段。详见顶部 ColumnItem 类型文档。 */
  columns: { type: Array, required: true, default: () => [] },

  /** 表单数据对象，支持通过 v-model 双向绑定。父组件传入的对象会作为初始数据合并到表单中。 */
  modelValue: { type: Object, default: () => ({}) },

  /** 表单标签宽度，如 '80px'、'120px' 等。对应 el-form 的 label-width。 */
  labelWidth: { type: String, default: '100px' },

  /** 表单标签对齐方式。可选值: 'left' | 'right' | 'top'。对应 el-form 的 label-position。 */
  labelPosition: { type: String, default: 'right' },

  /** 是否为行内表单。设为 true 时所有表单项排在一行。对应 el-form 的 inline。 */
  inline: { type: Boolean, default: false },

  /** 非行内模式下每行显示的列数。例如 2 表示一行两列，3 表示一行三列。 */
  colCount: { type: Number, default: 1 },

  /** 表单项列之间的间距（像素），对应 el-row 的 gutter 属性。 */
  gutter: { type: Number, default: 20 },

  /** 表单控件尺寸。可选值: 'large' | 'default' | 'small'。对应 el-form 的 size。 */
  size: { type: String, default: 'default' },

  /** 是否禁用整个表单。对应 el-form 的 disabled。单个字段可通过 column.disabled 单独控制。 */
  disabled: { type: Boolean, default: false },

  /** 是否在表单底部显示按钮区域。默认显示"确定"和"取消"按钮，可通过 footer 插槽自定义。 */
  showFooter: { type: Boolean, default: false },

  /** 是否可折叠 (查询表单)。启用后收起态仅显示快速搜索框，展开后显示完整表单。 */
  collapsible: { type: Boolean, default: false },

  /** 默认是否收起。仅在 collapsible 为 true 时生效。 */
  defaultCollapsed: { type: Boolean, default: true },

  /** 收起时的搜索框占位文字。仅在 collapsible 为 true 时生效。不传时自动使用第一个查询字段的 label。 */
  quickSearchPlaceholder: { type: String, default: '' },
})

// ======================== 事件定义 ========================

const emit = defineEmits([
  /** 表单数据变化时触发，用于 v-model 双向绑定更新。参数: 当前表单数据的浅拷贝。 */
  'update:modelValue',
  /** 点击"查询"按钮或收起态快速搜索时触发。参数: 当前表单数据的浅拷贝。 */
  'search',
  /** 点击"重置"按钮时触发。无参数。 */
  'reset',
  /** 点击"确定"按钮且表单验证通过后触发。参数: 当前表单数据的浅拷贝。 */
  'submit',
  /** 点击"取消"按钮时触发。无参数。 */
  'cancel',
  /** 表单字段值发生变化时触发。参数: (prop: string, value: *, formData: Object)。 */
  'change',
])

// ======================== 响应式状态 ========================

/** el-form 组件的模板引用，用于调用表单实例方法（validate、resetFields 等） */
const formRef = ref(null)

/** 表单数据的响应式对象，通过 reactive 创建。字段名由 columns 配置决定。 */
const formData = reactive({})

/** 是否处于收起状态。仅在 collapsible 模式下有效。 */
const collapsed = ref(props.defaultCollapsed)

/**
 * 是否正在初始化中。
 * 用于防止初始化阶段 modelValue 的 watch 和 formData 的 watch 互相触发导致死循环。
 * 初始化完成后设为 false。
 */
const isInitializing = ref(true)

// ======================== 计算属性 ========================

/**
 * 快速搜索对应的列配置。
 * 取 columns 数组中第一个非 buttons/slot 类型的列，
 * 收起态的快速搜索输入框将绑定到此字段。
 */
const quickSearchColumn = computed(() => {
  return props.columns.find((c) => c.type !== 'buttons' && c.type !== 'slot')
})

/**
 * 快速搜索框的占位文字。
 * 优先使用 props.quickSearchPlaceholder，未传时自动取第一个查询字段的 label 生成。
 */
const quickSearchPlaceholderText = computed(() => {
  if (props.quickSearchPlaceholder) return props.quickSearchPlaceholder
  const col = quickSearchColumn.value
  if (!col) return '请输入关键字搜索'
  const action = col.type === 'select' || col.type === 'date' || col.type === 'cascader' ? '请选择' : '请输入'
  return `${action}${col.label}`
})

/**
 * buttons 类型列的栅格宽度。
 * 按钮始终独占一行右对齐，不与查询条件同行。
 */
const buttonsSpan = computed(() => {
  return 24
})

/**
 * 快速搜索输入框的双向绑定值。
 * 内部映射到 quickSearchColumn 对应的 formData 字段。
 * @type {import('vue').WritableComputedRef<string>}
 */
const quickSearchValue = computed({
  get: () => (quickSearchColumn.value ? formData[quickSearchColumn.value.prop] : ''),
  set: (val) => {
    if (quickSearchColumn.value) formData[quickSearchColumn.value.prop] = val
  },
})

/**
 * 解析后的列配置。
 * 将 'dict:xxx' 格式的 options 引用替换为字典数据数组，
 * 非 dict 引用的列保持原样。
 */
const resolvedColumns = computed(() => {
  return props.columns.map(col => resolveColumn(col, dictStore.cache))
})

// ======================== 初始化方法 ========================

/** 需要进行值类型规范化的字段类型 */
const OPTION_TYPES = new Set(['select', 'remote-select', 'radio', 'checkbox', 'cascader'])

/**
 * 规范化表单值，使其与选项值类型一致。
 * 解决后端返回数值（如 1）与前端选项字符串（如 '1'）不匹配导致无法选中的问题。
 * 通过 String() 松散比较找到匹配项后，返回选项中定义的原始值类型。
 */
function normalizeValue(col, value) {
  if (value === undefined || value === null || value === '') return value
  if (!OPTION_TYPES.has(col.type)) return value
  // 取已解析的选项数组：优先用 resolvedColumns 中的，否则尝试从 dictStore 取字典引用
  let options = col._resolvedOptions
  if (!options && typeof col.options === 'string' && col.options.startsWith('dict:')) {
    const dictType = col.options.slice(5)
    options = dictStore.cache[dictType]
  }
  if (!options || !Array.isArray(options) || !options.length) return value
  for (const opt of options) {
    const optVal = opt.value !== undefined ? opt.value : opt
    if (String(optVal) === String(value)) return optVal
  }
  return value
}

/**
 * 批量规范化表单数据中所有选项类型字段的值。
 * @param {Object} data - 待规范化的表单数据
 * @returns {Object} 规范化后的新对象
 */
function normalizeFormData(data) {
  const result = { ...data }
  props.columns.forEach((col) => {
    if (result[col.prop] !== undefined) {
      result[col.prop] = normalizeValue(col, result[col.prop])
    }
  })
  return result
}

/**
 * 根据当前 columns 配置初始化表单数据。
 *
 * 遍历 columns 数组，为每个非 buttons/slot 类型的字段设置初始值：
 * - 如果该字段在 formData 中已有值，则保留不变（不覆盖）
 * - 如果配置了 defaultValue，则使用 defaultValue 作为初始值
 * - 如果字段类型为 checkbox 或设置了 multiple，默认值为空数组 []
 * - 其他类型默认值为空字符串 ''
 *
 * 对于 cascade-select 类型，还会额外初始化其 cascadeItems 中各级联字段的值。
 *
 * 初始化完成后，将 props.modelValue 中的值合并到 formData 中（外部传入值优先）。
 */
function initFormData() {
  props.columns.forEach((col) => {
    // buttons 和 slot 类型不需要初始化表单数据
    if (col.type === 'buttons' || col.type === 'slot') return

    // cascade-select 类型需要额外初始化各级联字段
    if (col.type === 'cascade-select' && col.cascadeItems) {
      col.cascadeItems.forEach((level) => {
        if (formData[level.prop] === undefined) {
          formData[level.prop] = ''
        }
      })
    }

    // 设置字段初始值（仅当 formData 中尚无该字段时）
    if (formData[col.prop] === undefined) {
      if (col.defaultValue !== undefined) {
        formData[col.prop] = col.defaultValue
      } else if (col.type === 'number') {
        formData[col.prop] = null
      } else if (col.type === 'checkbox' || col.multiple) {
        formData[col.prop] = []
      } else {
        formData[col.prop] = ''
      }
    }
  })

  // 将外部传入的 modelValue 合并到 formData（覆盖默认值），并规范化选项类型字段的值
  const normalized = normalizeFormData(props.modelValue)
  Object.keys(normalized).forEach((key) => {
    if (normalized[key] !== undefined) formData[key] = normalized[key]
  })
}

// ======================== 校验规则 ========================

/** 表单校验规则对象，以字段 prop 为键，规则数组为值。 */
const formRules = reactive({})

/**
 * 根据 columns 配置初始化表单校验规则。
 *
 * 遍历 columns，为每个字段生成校验规则：
 * - 如果配置了自定义 rules，则直接使用
 * - 如果 required 为 true 且未配置 rules，则自动生成必填校验规则：
 *   - 选择器/日期/级联类型提示 "请选择{label}"
 *   - 其他类型提示 "请输入{label}"
 *   - 触发时机默认为 'blur'，可通过 trigger 属性自定义
 */
function initRules() {
  props.columns.forEach((col) => {
    if (col.rules) {
      // 使用自定义校验规则
      formRules[col.prop] = col.rules
    } else if (col.required) {
      // 自动生成必填校验规则
      const msg = col.requiredMessage || `请${col.type === 'select' || col.type === 'date' || col.type === 'cascader' ? '选择' : '输入'}${col.label}`
      formRules[col.prop] = [{ required: true, message: msg, trigger: col.trigger || 'blur' }]
    }
  })
}

// ======================== 侦听器 ========================

/**
 * 监听外部 modelValue 变化，同步到内部 formData。
 * 初始化阶段跳过，避免与 formData 的 watch 互相触发。
 */
watch(() => props.modelValue, (val) => {
  if (isInitializing.value) return
  const normalized = normalizeFormData(val)
  Object.keys(normalized).forEach((key) => { formData[key] = normalized[key] })
}, { deep: true })

/**
 * 监听内部 formData 变化，同步到外部 modelValue（v-model 更新）。
 * 初始化阶段跳过，避免与 modelValue 的 watch 互相触发。
 */
watch(formData, (val) => {
  if (isInitializing.value) return
  emit('update:modelValue', { ...val })
}, { deep: true })

/**
 * 监听 columns 变化，重新初始化校验规则。
 * 当列配置中的 required、rules 等属性动态变化时，需要同步更新 formRules。
 */
watch(() => props.columns, () => {
  // 清空旧规则，避免已移除字段的残留规则
  Object.keys(formRules).forEach(key => delete formRules[key])
  initRules()
})

// ======================== 远程搜索 ========================

/**
 * 处理远程选择器的远程搜索请求。
 *
 * 调用 column 配置中的 remoteMethod 方法进行远程数据获取。
 * 在请求期间自动管理 item._loading 状态以显示加载动画。
 * 无论请求成功或失败，最终都会关闭 loading 状态。
 *
 * @param {ColumnItem} item - 远程选择器对应的列配置项，须包含 remoteMethod 方法。
 * @param {string} query - 用户输入的搜索关键字。
 * @returns {Promise<void>}
 */
async function handleRemoteSearch(item, query) {
  if (!item.remoteMethod) return
  item._loading = true
  try { await item.remoteMethod(query, item) } finally { item._loading = false }
}

// ======================== 多级联动 ========================

/**
 * 每级联动选择器的动态状态存储。
 * key 格式: "columnProp-levelProp"，value 为 { _options: [], _loading: false }。
 * 用于存储异步 loadOptions 加载的选项和加载状态。
 */
const cascadeLevelState = reactive({})

/**
 * 获取多级联动选择器中指定级别的加载状态。
 * @param {ColumnItem} item - cascade-select 类型的列配置项。
 * @param {CascadeLevelItem} level - 当前级别的配置项。
 * @returns {boolean}
 */
function getCascadeLevelLoading(item, level) {
  const stateKey = `${item.prop}-${level.prop}`
  return cascadeLevelState[stateKey]?._loading || false
}

/**
 * 获取多级联动选择器中指定级别的下拉选项。
 *
 * 按以下优先级获取选项：
 * 1. 如果 level 配置了 loadOptions 且已异步加载过，返回加载的选项
 * 2. 如果 level 配置了 optionsFn 函数，调用该函数动态获取选项
 * 3. 如果 level 配置了静态 options 数组，直接返回
 * 4. 如果 level 配置了 parentProp + optionsMap，根据父级字段值从 optionsMap 中查找对应选项
 * 5. 以上均不满足时返回空数组
 *
 * @param {ColumnItem} item - cascade-select 类型的列配置项。
 * @param {CascadeLevelItem} level - 当前级别的配置项。
 * @returns {Array<{label: string, value: *, disabled?: boolean}>} 该级别可用的选项数组。
 */
function getCascadeOptions(item, level) {
  const stateKey = `${item.prop}-${level.prop}`
  const state = cascadeLevelState[stateKey]
  // 优先级1: 如果有 loadOptions 且已加载过，返回动态加载的选项
  if (typeof level.loadOptions === 'function' && state?._options) {
    return state._options
  }
  // 优先级2: 如果有 optionsFn，调用它获取当前级别选项
  if (typeof level.optionsFn === 'function') {
    return level.optionsFn(formData, item.cascadeItems)
  }
  // 优先级3: 如果有静态 options 直接返回
  if (level.options) return level.options
  // 优先级4: 如果有 parentProp + optionsMap，根据父级值过滤
  if (level.parentProp && level.optionsMap) {
    const parentVal = formData[level.parentProp]
    return parentVal ? (level.optionsMap[parentVal] || []) : []
  }
  // 兜底: 返回空数组
  return []
}

/**
 * 处理多级联动选择器中某一级的选中值变化。
 *
 * 当某一级选择值变化时：
 * 1. 自动清空当前级别之后所有下级字段的值（保持联动一致性）
 * 2. 查找所有 parentProp 指向当前级的后续级别，并行调用 loadOptions 异步加载选项
 *    （支持多个子级依赖同一个父级，如库区和巷道都依赖仓库）
 * 3. 触发 change 事件，通知外部数据已变化
 *
 * @param {ColumnItem} item - cascade-select 类型的列配置项。
 * @param {CascadeLevelItem} level - 发生变化的级别配置项。
 * @param {*} val - 选中的新值。
 */
async function handleCascadeChange(item, level, val) {
  // 选中值变化后，清空所有下级字段的值和动态选项
  const items = item.cascadeItems
  const idx = items.indexOf(level)
  for (let i = idx + 1; i < items.length; i++) {
    formData[items[i].prop] = ''
    const stateKey = `${item.prop}-${items[i].prop}`
    if (cascadeLevelState[stateKey]) {
      cascadeLevelState[stateKey]._options = []
    }
  }

  // 加载所有 parentProp 指向当前级的后续级别的选项（支持多个子级依赖同一个父级）
  if (val) {
    const loadTasks = []
    for (let i = idx + 1; i < items.length; i++) {
      if (items[i].parentProp === level.prop && typeof items[i].loadOptions === 'function') {
        loadTasks.push(loadCascadeLevelOptions(item, items[i]))
      }
    }
    await Promise.all(loadTasks)
  }

  // 触发 change 事件
  handleChange(item, formData)
}

// ======================== 字段变更处理 ========================

/** 合法的 el-tag 类型常量 */
const validTagTypes = ['success', 'warning', 'danger', 'info', 'primary']

/**
 * 根据表单项配置获取某个值对应的标签颜色类型
 * @param {Object} item - 表单项配置（已解析字典引用）
 * @param {*} value - 选项值
 * @returns {string|undefined} 标签类型
 */
const getTagColorForValue = (item, value) => {
  if (item.tagTypeMap && item.tagTypeMap[value] !== undefined) return item.tagTypeMap[value] || undefined
  if (item.tagMap && item.tagMap[value] !== undefined) {
    const mapped = item.tagMap[value]
    return validTagTypes.includes(mapped) ? mapped : undefined
  }
  return undefined
}

/**
 * 判断表单项是否具有标签颜色映射
 * @param {Object} item - 表单项配置
 * @returns {boolean}
 */
const hasItemTagColors = (item) => !!(item.tagTypeMap || (item.tagMap && Object.values(item.tagMap).some((v) => validTagTypes.includes(v))))

/**
 * 处理表单字段值变化的通用方法。
 *
 * 当任何支持 change 事件的表单控件（select、date、radio、checkbox、cascader、cascade-select）值变化时调用。
 * 依次执行：
 * 1. 触发组件的 'change' 事件，将字段名、新值和完整表单数据传递给父组件
 * 2. 调用 column 配置项中可选的 onChange 回调函数（如果存在）
 *
 * @param {ColumnItem} item - 发生变化的列配置项。
 * @param {*} val - 字段的新值。
 */
function handleChange(item, val) {
  emit('change', item.prop, val, formData)
  item.onChange?.(val, formData)
}

// ======================== 表单操作方法 ========================

/**
 * 手动触发表单校验。
 *
 * 调用 el-form 实例的 validate 方法进行全量校验。
 * 如果表单引用不存在（如尚未挂载），直接返回 true。
 *
 * @returns {Promise<boolean>} 校验通过返回 true，存在校验失败的字段返回 false。
 */
async function validate() {
  if (!formRef.value) return true
  try { await formRef.value.validate(); return true } catch { return false }
}

/**
 * 重置所有表单字段为初始值，并清除所有校验结果。
 * 内部调用 el-form 实例的 resetFields 方法。
 */
function resetFields() { formRef.value?.resetFields() }

/**
 * 清除指定字段或所有字段的校验结果（不重置字段值）。
 * 内部调用 el-form 实例的 clearValidate 方法。
 *
 * @param {string|Array<string>} [p] - 要清除校验的字段名（prop）或字段名数组。
 *   不传参数时清除所有字段的校验结果。
 */
function clearValidate(p) { formRef.value?.clearValidate(p) }

// ======================== 按钮事件处理 ========================

/**
 * 处理"查询"按钮点击事件。
 * 先执行表单校验，校验通过后触发 'search' 事件并传递当前表单数据的浅拷贝。
 */
function handleSearch() { validate().then((v) => { if (v) emit('search', { ...formData }) }) }

/**
 * 处理收起态快速搜索事件。
 * 在快速搜索输入框中按回车或点击搜索按钮时触发。
 * 直接触发 'search' 事件，不执行表单校验（收起态无法校验所有字段）。
 */
function handleQuickSearch() { emit('search', { ...formData }) }

/**
 * 处理"重置"按钮点击事件。
 * 手动把各字段重置为默认值（不使用 el-form resetFields：其 initialValue 在挂载时记录，
 * 而此时 formData 尚未初始化（onMounted 才 init），initialValue 为 undefined，
 * 会导致"重置清空后又回滚"。这里显式置默认值，确保稳定清空）。
 */
function handleReset() {
  props.columns.forEach((col) => {
    if (col.type === 'buttons' || col.type === 'slot') return
    if (col.defaultValue !== undefined) formData[col.prop] = col.defaultValue
    else if (col.type === 'checkbox' || col.multiple) formData[col.prop] = []
    else if (col.type === 'number') formData[col.prop] = null
    else formData[col.prop] = ''
  })
  clearValidate()
  emit('reset')
}

/**
 * 处理"确定"按钮点击事件。
 * 先执行表单校验，校验通过后触发 'submit' 事件并传递当前表单数据的浅拷贝。
 */
function handleSubmit() { validate().then((v) => { if (v) emit('submit', { ...formData }) }) }

/**
 * 处理"取消"按钮点击事件。
 * 直接触发 'cancel' 事件，不执行任何校验或重置操作。
 */
function handleCancel() { emit('cancel') }

/**
 * 异步加载指定级别联动选择器的选项。
 * @param {ColumnItem} item - cascade-select 类型的列配置项。
 * @param {CascadeLevelItem} level - 要加载选项的级别配置项。
 */
async function loadCascadeLevelOptions(item, level) {
  const stateKey = `${item.prop}-${level.prop}`
  if (!cascadeLevelState[stateKey]) {
    cascadeLevelState[stateKey] = { _options: [], _loading: false }
  }
  const state = cascadeLevelState[stateKey]
  state._loading = true
  state._options = []
  try {
    const parentValue = level.parentProp ? formData[level.parentProp] : undefined
    const options = await level.loadOptions(parentValue, formData, level)
    state._options = options || []
  } catch (e) {
    console.error(`[KhForm] cascade-select loadOptions error (${level.prop}):`, e)
    state._options = []
  } finally {
    state._loading = false
  }
}

/**
 * 初始化多级联动选择器：自动加载无父级（第一级）且配置了 loadOptions 的级别选项。
 * 如果 modelValue 中已有值，还会按链路逐级加载后续级别。
 */
function initCascadeSelects() {
  props.columns.forEach((col) => {
    if (col.type !== 'cascade-select' || !col.cascadeItems?.length) return
    // 找到需要初始加载的起始级别
    let startIdx = 0
    // 如果第一级有 parentProp，尝试从已有值逐级向后找到第一个需要加载的级别
    for (let i = 0; i < col.cascadeItems.length; i++) {
      const level = col.cascadeItems[i]
      if (level.parentProp && formData[level.parentProp]) {
        // 父级有值，可以继续向后加载
        if (typeof level.loadOptions === 'function') {
          startIdx = i
        }
      } else {
        break
      }
    }
    // 从起始级别开始，依次加载所有配置了 loadOptions 的级别
    const loadChain = async (idx) => {
      for (let i = idx; i < col.cascadeItems.length; i++) {
        const level = col.cascadeItems[i]
        if (typeof level.loadOptions === 'function') {
          await loadCascadeLevelOptions(col, level)
        }
        // 如果下一级的父级就是当前级且当前级无值，停止后续加载
        const next = col.cascadeItems[i + 1]
        if (next?.parentProp === level.prop && !formData[level.prop]) {
          break
        }
      }
    }
    loadChain(startIdx)
  })
}

// ======================== 生命周期 ========================

/**
 * 组件挂载完成后的初始化逻辑。
 *
 * 按顺序执行：
 * 1. initFormData() - 根据 columns 配置初始化表单数据，合并外部传入的 modelValue
 * 2. initRules() - 根据 columns 配置生成校验规则
 * 3. nextTick 后清除校验状态并标记初始化完成（开启 watch 同步）
 */
onMounted(async () => {
  // 先加载字典数据，确保 normalizeFormData 能拿到选项进行类型转换
  const dictTypes = collectDictTypes(props.columns)
  await Promise.all([...dictTypes].map(type => dictStore.getDict(type)))
  initFormData()
  initRules()
  // 初始化多级联动：加载第一级（及已有值的后级）选项
  initCascadeSelects()
  nextTick(() => {
    clearValidate()
    isInitializing.value = false
  })
})

// ======================== 暴露给父组件的方法和属性 ========================

defineExpose({
  /** el-form 组件实例引用，可用于直接调用 Element Plus 表单方法 */
  formRef,
  /** 表单数据的响应式对象（reactive），可直接读写各字段值 */
  formData,
  /** 手动触发表单全量校验，返回 Promise<boolean> */
  validate,
  /** 重置所有表单字段为初始值并清除校验结果 */
  resetFields,
  /** 清除指定字段或所有字段的校验结果（不重置值） */
  clearValidate,
  /** 根据当前 columns 配置重新初始化表单数据 */
  initFormData,
})
</script>

<style scoped>
.kh-form__quick {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  padding-bottom: 16px;
}

.kh-form__quick .el-input {
  max-width: 360px;
}

.kh-form__toggle-btn,
.kh-form__toggle-bar {
  margin-left: 8px;
}

.kh-form__toggle-bar {
  text-align: right;
  margin-right: 0;
  padding-bottom: 4px;
}

.kh-form__cascade-select {
  display: flex;
  gap: 8px;
  width: 100%;
}

.kh-form__cascade-select .el-select {
  flex: 1;
  min-width: 0;
}

/* 表单控件在 el-col 网格中撑满宽度 */
.kh-form :deep(.el-select),
.kh-form :deep(.el-date-editor),
.kh-form :deep(.el-cascader),
.kh-form :deep(.el-input-number) {
  width: 100%;
}

/* 日期范围选择器需要更多宽度 */
.kh-form :deep(.el-date-editor--daterange),
.kh-form :deep(.el-date-editor--datetimerange) {
  width: 100%;
}

/* 查询按钮区域 */
.kh-form :deep(.el-form-item--buttons) {
  margin-bottom: 16px;
}

.kh-form :deep(.el-form-item--buttons .el-form-item__label) {
  display: none;
}

.kh-form :deep(.el-form-item--buttons .el-form-item__content) {
  width: 100%;
  display: flex;
  justify-content: flex-end;
}

.kh-form :deep(.el-form-item--buttons .el-form-item__content .el-button + .el-button) {
  margin-left: 8px;
}

.kh-form__select-option {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
</style>

<style>
/* 颜色圆点 (el-select 下拉选项渲染在 body 下，需要非 scoped) */
.kh-form__color-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: var(--el-color-info);
  flex-shrink: 0;
}

.kh-form__color-dot.is-success {
  background-color: var(--el-color-success);
}

.kh-form__color-dot.is-warning {
  background-color: var(--el-color-warning);
}

.kh-form__color-dot.is-danger {
  background-color: var(--el-color-danger);
}

.kh-form__color-dot.is-info {
  background-color: var(--el-color-info);
}

.kh-form__color-dot.is-primary {
  background-color: var(--el-color-primary);
}
</style>
