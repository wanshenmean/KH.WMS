<template>
  <el-dialog
    v-model="visible"
    :width="width"
    :top="top"
    :close-on-click-modal="closeOnClickModal"
    :close-on-press-escape="closeOnPressEscape"
    :destroy-on-close="destroyOnClose"
    :draggable="draggable"
    :fullscreen="fullscreen"
    :show-close="false"
    :append-to-body="appendToBody"
    :class="'kh-dialog'"
    :style="dialogStyle"
    v-bind="$attrs"
    @close="handleClose"
  >
    <!-- 头部区域：标题 + 描述 -->
    <template #header="{ close }">
      <div class="kh-dialog__header">
        <div class="kh-dialog__header-content">
          <div class="kh-dialog__title">{{ title }}</div>
          <div v-if="description" class="kh-dialog__description">{{ description }}</div>
        </div>
        <button v-if="showClose" class="kh-dialog__close" @click="close">
          <el-icon><Close /></el-icon>
        </button>
      </div>
    </template>

    <!-- 默认插槽 / 表单插槽 -->
    <div class="kh-dialog__body">
      <slot>
        <KhForm
          v-if="formColumns.length"
          ref="khFormRef"
          :columns="formColumns"
          v-model="formData"
          :label-width="formLabelWidth"
          :col-count="formColCount"
          :size="formSize"
          :disabled="formDisabled"
        >
          <template v-for="(_, name) in $slots" #[name]="slotData">
            <slot :name="name" v-bind="slotData || {}" />
          </template>
        </KhForm>
      </slot>
    </div>

    <!-- 底部按钮 -->
    <template v-if="showFooter" #footer>
      <div class="kh-dialog__footer">
        <slot name="footer">
          <el-button @click="handleCancel">{{ cancelText }}</el-button>
          <el-button type="primary" :loading="confirmLoading" @click="handleConfirm">
            {{ confirmText }}
          </el-button>
        </slot>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
/**
 * @file KhDialog 通用弹窗组件
 * @description 通用弹窗组件，支持内置 KhForm 表单、自定义内容、全屏、拖拽等功能。
 *              基于 Element Plus 的 el-dialog 进行封装，提供了 v-model 双向绑定、
 *              内置表单自动渲染与校验、底部确认/取消按钮等常用弹窗场景的标准化方案。
 *              可通过插槽完全自定义弹窗内容，也可通过 formColumns 属性快速生成表单弹窗。
 *
 * @component KhDialog
 *
 * @props {Boolean}   modelValue          - 控制弹窗的显示与隐藏，支持 v-model 双向绑定。默认值: false（隐藏）
 * @props {String}    title               - 弹窗标题文字，显示在弹窗顶部标题栏。默认值: ''（空字符串）
 * @props {String|Number} width           - 弹窗宽度，支持字符串（如 '600px'、'50%'）或数字（单位 px）。默认值: '600px'
 * @props {String}    top                 - 弹窗距离视口顶部的距离，CSS 长度值。默认值: '15vh'
 * @props {Boolean}   closeOnClickModal   - 是否可以通过点击遮罩层（弹窗外区域）关闭弹窗。默认值: false（不可关闭）
 * @props {Boolean}   closeOnPressEscape  - 是否可以通过按下 ESC 键关闭弹窗。默认值: true（可以关闭）
 * @props {Boolean}   destroyOnClose      - 关闭弹窗时是否销毁弹窗内的子元素（DOM 节点），适用于内容较重或需要每次重新初始化的场景。默认值: false（不销毁）
 * @props {Boolean}   draggable           - 是否启用弹窗拖拽功能，允许用户通过标题栏拖动弹窗位置。默认值: false（不可拖拽）
 * @props {Boolean}   fullscreen          - 是否以全屏模式展示弹窗。默认值: false（非全屏）
 * @props {Boolean}   showClose           - 是否显示弹窗右上角的关闭按钮。默认值: true（显示）
 * @props {Boolean}   appendToBody        - 是否将弹窗 DOM 节点插入到 document.body 下，可避免父级元素的 CSS 样式（如 overflow:hidden、transform 等）对弹窗定位的影响。默认值: true（插入到 body）
 * @props {Boolean}   showFooter          - 是否显示弹窗底部的操作按钮区域。默认值: true（显示）
 * @props {String}    confirmText         - 确认按钮的显示文字。默认值: '确 定'
 * @props {String}    cancelText          - 取消按钮的显示文字。默认值: '取 消'
 * @props {Boolean}   confirmLoading      - 确认按钮是否处于加载状态，为 true 时按钮显示 loading 动画且不可重复点击。默认值: false（非加载状态）
 * @props {Array}     formColumns         - 内置 KhForm 表单的列配置数组，传入非空数组后将自动渲染 KhForm 表单。数组中每一项对应一个表单字段配置。默认值: []（空数组，不渲染表单）
 * @props {Object}    formModel           - 内置 KhForm 表单的初始数据对象，弹窗每次打开时会用此对象初始化表单数据。默认值: {}（空对象）
 * @props {String}    formLabelWidth      - 内置 KhForm 表单中标签（label）的宽度。默认值: '100px'
 * @props {Number}    formColCount        - 内置 KhForm 表单每行显示的表单项列数。默认值: 1（单列布局）
 * @props {String}    formSize            - 内置 KhForm 表单的尺寸，可选值为 'large' / 'default' / 'small'。默认值: 'default'
 * @props {Boolean}   formDisabled        - 内置 KhForm 表单是否处于禁用状态。默认值: false（不禁用）
 *
 * @event {Function} update:modelValue - 当弹窗显示状态发生变化时触发，用于 v-model 双向绑定
 *   @param {Boolean} val - 弹窗当前的显示状态，true 表示已打开，false 表示已关闭
 *
 * @event {Function} open - 当弹窗打开后触发（在表单数据初始化和校验清除之后）
 *
 * @event {Function} close - 当弹窗关闭时触发（无论通过何种方式关闭）
 *
 * @event {Function} confirm - 当用户点击确认按钮且表单校验通过后触发
 *   @param {Object} [formData] - 当使用内置 KhForm 时，传递当前表单数据的深拷贝对象；未使用内置表单时不传参
 *
 * @event {Function} cancel - 当用户点击取消按钮时触发
 *
 * @expose {Ref<Object|null>} khFormRef - 内置 KhForm 组件的模板引用，可用于直接调用 KhForm 的方法（如 validate、clearValidate、resetFields 等）
 *
 * @expose {Ref<Object>} formData - 内置 KhForm 的响应式表单数据对象，可直接读写当前表单值
 *
 * @expose {Function} open - 编程式打开弹窗，并可同时传入表单初始数据
 *   @param {Object} [data] - 可选参数，传入后将作为表单初始数据填充到内置 KhForm 中
 *   @returns {void}
 *
 * @expose {Function} close - 编程式关闭弹窗，等价于将 visible 设为 false
 *   @returns {void}
 *
 * @slot default - 默认内容插槽，用于自定义弹窗主体内容。
 *                 若未传入自定义内容且 formColumns 非空，则自动渲染内置 KhForm 表单。
 *
 * @slot footer - 底部操作区域插槽，用于自定义底部按钮。
 *                若未传入自定义内容，则默认渲染"取消"和"确认"两个按钮。
 */

import KhForm from '@/components/KhForm/index.vue'

/**
 * 组件属性定义
 * @description 定义 KhDialog 组件的所有 props，包括弹窗控制、样式、交互行为以及内置表单相关配置
 */
const props = defineProps({
  /**
   * 控制弹窗显示/隐藏的 v-model 绑定值
   * @type {Boolean}
   * @default false - 默认隐藏弹窗
   */
  modelValue: {
    type: Boolean,
    default: false,
  },

  /**
   * 弹窗标题文字
   * @type {String}
   * @default '' - 默认无标题
   */
  title: {
    type: String,
    default: '',
  },

  /**
   * 弹窗头部标题下方的描述文字，用于补充说明弹窗用途
   * @type {String}
   * @default '' - 默认无描述
   */
  description: {
    type: String,
    default: '',
  },

  /**
   * 弹窗宽度，支持字符串（如 '720px'、'50%'）或数字（单位为 px）
   * @type {String|Number}
   * @default '720px'
   */
  width: {
    type: [String, Number],
    default: '720px',
  },

  /**
   * 弹窗内容区域高度，支持字符串（如 '500px'、'60vh'）或数字（单位 px）。
   * 设置后内容区域固定高度并内部滚动，不设置则自适应内容高度。
   * @type {String|Number}
   * @default '' - 自适应
   */
  height: {
    type: [String, Number],
    default: '',
  },

  /**
   * 弹窗距离视口顶部的距离
   * @type {String}
   * @default '15vh' - 距顶部 15% 视口高度
   */
  top: {
    type: String,
    default: '10vh',
  },

  /**
   * 是否允许通过点击遮罩层关闭弹窗
   * @type {Boolean}
   * @default false - 默认不允许，防止误操作
   */
  closeOnClickModal: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否允许通过按下 ESC 键关闭弹窗
   * @type {Boolean}
   * @default true - 默认允许
   */
  closeOnPressEscape: {
    type: Boolean,
    default: true,
  },

  /**
   * 关闭弹窗时是否销毁弹窗内的子元素
   * @type {Boolean}
   * @default false - 默认不销毁，保留 DOM 以提升再次打开时的性能
   */
  destroyOnClose: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否启用弹窗拖拽功能（通过标题栏拖动）
   * @type {Boolean}
   * @default false - 默认不可拖拽
   */
  draggable: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否以全屏模式展示弹窗
   * @type {Boolean}
   * @default false - 默认非全屏
   */
  fullscreen: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否显示弹窗右上角的关闭按钮
   * @type {Boolean}
   * @default true - 默认显示
   */
  showClose: {
    type: Boolean,
    default: true,
  },

  /**
   * 是否将弹窗 DOM 插入到 document.body 下
   * 设置为 true 可避免父级 CSS（如 overflow:hidden、transform）对弹窗定位的影响
   * @type {Boolean}
   * @default true - 默认插入到 body
   */
  appendToBody: {
    type: Boolean,
    default: true,
  },

  /**
   * 是否显示弹窗底部的操作按钮区域
   * @type {Boolean}
   * @default true - 默认显示底部按钮
   */
  showFooter: {
    type: Boolean,
    default: true,
  },

  /**
   * 确认按钮的显示文字
   * @type {String}
   * @default '确 定'
   */
  confirmText: {
    type: String,
    default: '确 定',
  },

  /**
   * 取消按钮的显示文字
   * @type {String}
   * @default '取 消'
   */
  cancelText: {
    type: String,
    default: '取 消',
  },

  /**
   * 确认按钮是否处于加载状态
   * 为 true 时按钮显示 loading 动画，防止重复提交
   * @type {Boolean}
   * @default false - 默认非加载状态
   */
  confirmLoading: {
    type: Boolean,
    default: false,
  },

  // ---- Form 透传属性 ----

  /**
   * 内置 KhForm 表单的列配置数组
   * 传入非空数组后将自动渲染 KhForm 表单组件
   * 数组中每一项对应一个表单字段的配置（如类型、校验规则、选项等）
   * @type {Array}
   * @default [] - 默认空数组，不渲染表单
   */
  formColumns: {
    type: Array,
    default: () => [],
  },

  /**
   * 内置 KhForm 表单的初始数据对象
   * 弹窗每次打开时会用此对象的浅拷贝来初始化表单数据
   * @type {Object}
   * @default {} - 默认空对象
   */
  formModel: {
    type: Object,
    default: () => ({}),
  },

  /**
   * 内置 KhForm 表单中标签（label）的宽度
   * @type {String}
   * @default '100px'
   */
  formLabelWidth: {
    type: String,
    default: '100px',
  },

  /**
   * 内置 KhForm 表单每行显示的表单项列数
   * 设置为 2 时表单将采用双列布局
   * @type {Number}
   * @default 1 - 默认单列布局
   */
  formColCount: {
    type: Number,
    default: 1,
  },

  /**
   * 内置 KhForm 表单的尺寸
   * 可选值: 'large'（大）、'default'（默认）、'small'（小）
   * @type {String}
   * @default 'default'
   */
  formSize: {
    type: String,
    default: 'default',
  },

  /**
   * 内置 KhForm 表单是否整体禁用
   * 禁用后表单中所有字段均不可编辑
   * @type {Boolean}
   * @default false - 默认不禁用
   */
  formDisabled: {
    type: Boolean,
    default: false,
  },
})

/**
 * 弹窗动态样式
 * @description 根据 height prop 计算出需要绑定的内联样式，控制弹窗整体高度
 */
const dialogStyle = computed(() => {
  if (!props.height) return {}
  const h = typeof props.height === 'number' ? `${props.height}px` : props.height
  return { height: h }
})

/**
 * 组件事件定义
 * @description 定义 KhDialog 组件触发的所有自定义事件
 */
const emit = defineEmits([
  /** v-model 双向绑定事件，当弹窗显示状态改变时触发
   *  @arg {Boolean} val - 当前弹窗的显示状态
   */
  'update:modelValue',

  /** 弹窗打开事件，在弹窗完全打开后触发（表单数据已初始化、校验已清除） */
  'open',

  /** 弹窗关闭事件，在弹窗关闭动画结束后触发 */
  'close',

  /** 确认事件，用户点击确认按钮且表单校验通过后触发
   *  @arg {Object} [formData] - 使用内置 KhForm 时传递表单数据的深拷贝
   */
  'confirm',

  /** 取消事件，用户点击取消按钮时触发 */
  'cancel',
])

/**
 * 弹窗内部的显示状态（响应式引用）
 * @description 与 props.modelValue 保持双向同步，作为 el-dialog 的 v-model 绑定值
 * @type {import('vue').Ref<Boolean>}
 */
const visible = ref(false)

/**
 * 内置 KhForm 组件的模板引用
 * @description 用于调用 KhForm 实例上的方法，如 validate（表单校验）、clearValidate（清除校验）等
 * @type {import('vue').Ref<Object|null>}
 */
const khFormRef = ref(null)

/**
 * 内置 KhForm 的响应式表单数据对象
 * @description 弹窗打开时从 props.formModel 浅拷贝初始化，用户在表单中的修改会实时反映在此对象上
 * @type {import('vue').Ref<Object>}
 */
const formData = ref({})

/**
 * 监听外部 modelValue 变化，同步更新内部 visible 状态
 * @description 确保父组件通过 v-model 修改值时，弹窗的显示状态能正确响应
 */
watch(
  () => props.modelValue,
  (val) => {
    visible.value = val
  }
)

/**
 * 监听内部 visible 状态变化，向父组件发射 update:modelValue 事件
 * @description 实现 v-model 的双向绑定：当弹窗内部关闭（如点击遮罩、ESC）时，同步通知父组件
 */
watch(visible, (val) => {
  emit('update:modelValue', val)
})

/**
 * 监听弹窗打开，执行初始化逻辑
 * @description 当弹窗打开时：
 *   1. 用 props.formModel 的浅拷贝重置表单数据
 *   2. 触发 open 事件通知父组件
 *   3. 在下一个 tick 中清除所有表单校验状态，确保打开时无残留校验错误提示
 */
watch(visible, (val) => {
  if (val) {
    formData.value = { ...props.formModel }
    emit('open')
    nextTick(() => {
      khFormRef.value?.clearValidate?.()
    })
  } else {
    // 关闭时清空表单数据，避免下次打开残留上次输入
    formData.value = {}
    khFormRef.value?.resetFields?.()
  }
})

/**
 * 处理确认按钮点击
 * @description 异步处理确认逻辑：
 *   - 若存在内置 KhForm（khFormRef 有值），先执行表单校验（validate），
 *     校验通过后才触发 confirm 事件并传递表单数据的深拷贝
 *   - 若不存在内置 KhForm（使用自定义插槽内容），直接触发 confirm 事件
 * @async
 * @returns {Promise<void>}
 */
async function handleConfirm() {
  // 如果有内置 Form，先校验
  if (khFormRef.value) {
    const valid = await khFormRef.value.validate()
    if (!valid) return
    emit('confirm', { ...formData.value })
  } else {
    emit('confirm')
  }
}

/**
 * 处理取消按钮点击
 * @description 关闭弹窗并将 visible 设为 false，同时触发 cancel 事件通知父组件
 * @returns {void}
 */
function handleCancel() {
  visible.value = false
  emit('cancel')
}

/**
 * 处理弹窗关闭回调
 * @description 当 el-dialog 触发 close 事件时调用，向父组件发射 close 事件
 * 注意：此事件在弹窗关闭动画结束后触发，无论通过何种方式关闭（按钮、遮罩、ESC 等）
 * @returns {void}
 */
function handleClose() {
  emit('close')
}

/**
 * 暴露给父组件通过 ref 调用的方法和属性
 * @description 提供对弹窗内部状态的访问和编程式控制能力
 */
defineExpose({
  /**
   * 获取内部 KhForm 组件实例的引用
   * 可用于调用 KhForm 的 validate、clearValidate、resetFields 等方法
   * @type {import('vue').Ref<Object|null>}
   */
  khFormRef,

  /**
   * 获取内置 KhForm 的响应式表单数据对象
   * @type {import('vue').Ref<Object>}
   */
  formData,

  /**
   * 编程式打开弹窗
   * @description 将弹窗的 visible 状态设为 true，并可选地传入表单初始数据
   * @param {Object} [data] - 可选的表单初始数据对象，传入后将覆盖当前表单数据
   * @returns {void}
   */
  open: (data) => {
    if (data) formData.value = { ...data }
    visible.value = true
  },

  /**
   * 编程式关闭弹窗
   * @description 将弹窗的 visible 状态设为 false，触发关闭动画和事件
   * @returns {void}
   */
  close: () => {
    visible.value = false
  },
})
</script>

<style scoped>
/* 头部区域 */
.kh-dialog__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 16px 20px;
  background: #fafbfc;
  border-bottom: 1px solid #e4e7ed;
  flex-shrink: 0;
}

.kh-dialog__header-content {
  flex: 1;
  min-width: 0;
}

.kh-dialog__title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  line-height: 1.4;
}

.kh-dialog__description {
  font-size: 13px;
  color: #909399;
  line-height: 1.5;
  margin-top: 4px;
}

.kh-dialog__close {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  background: none;
  border-radius: 4px;
  color: #909399;
  cursor: pointer;
  flex-shrink: 0;
  margin-top: -2px;
  transition: all 0.2s;
}

.kh-dialog__close:hover {
  background: #e8eaed;
  color: #303133;
}

/* 内容区域 */
.kh-dialog__body {
  flex: 1;
  padding: 20px 24px;
  overflow-y: auto;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

/* 底部按钮区域 */
.kh-dialog__footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  padding: 12px 20px;
  background: #fafbfc;
  border-top: 1px solid #e4e7ed;
  flex-shrink: 0;
}
</style>

<!-- 非 scoped 样式：覆盖 el-dialog 默认布局 -->
<style>
.el-dialog.kh-dialog {
  display: flex;
  flex-direction: column;
  max-height: 85vh;
}

.el-dialog.kh-dialog .el-dialog__header {
  padding: 0;
  margin: 0;
}

.el-dialog.kh-dialog .el-dialog__body {
  padding: 0;
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.el-dialog.kh-dialog .el-dialog__footer {
  padding: 0;
  flex-shrink: 0;
}
</style>
