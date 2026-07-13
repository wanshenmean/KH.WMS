<template>
  <div class="kh-upload">
    <el-upload
      ref="uploadRef"
      :action="action"
      :headers="uploadHeaders"
      :data="data"
      :name="name"
      :accept="accept"
      :multiple="multiple"
      :drag="drag"
      :disabled="disabled"
      :list-type="listType"
      :auto-upload="autoUpload"
      :file-list="fileList"
      :limit="limit"
      :on-exceed="handleExceed"
      :before-upload="handleBeforeUpload"
      :on-progress="handleProgress"
      :on-success="handleSuccess"
      :on-error="handleError"
      :on-remove="handleRemove"
      :on-preview="handlePreview"
      :on-change="handleChange"
      :http-request="customRequest ? handleCustomRequest : undefined"
      :show-file-list="showFileList"
      v-bind="$attrs"
    >
      <!-- 拖拽区域 -->
      <template v-if="drag">
        <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
        <div class="el-upload__text">
          将文件拖到此处，或 <em>点击上传</em>
        </div>
      </template>

      <!-- 图片列表模式 -->
      <template v-else-if="listType === 'picture-card'">
        <el-icon><Plus /></el-icon>
      </template>

      <!-- 默认按钮模式 -->
      <template v-else>
        <el-button :type="buttonType" :icon="UploadIcon" :disabled="disabled">
          {{ buttonText }}
        </el-button>
      </template>

      <!-- 提示文字 -->
      <template v-if="$slots.tip || tip" #tip>
        <slot name="tip">
          <div class="el-upload__tip">{{ tip }}</div>
        </slot>
      </template>
    </el-upload>

    <!-- 图片预览弹窗 -->
    <el-dialog v-model="dialogVisible" title="图片预览" append-to-body>
      <img :src="dialogImageUrl" alt="预览" style="width: 100%" />
    </el-dialog>
  </div>
</template>

<script setup>
/**
 * @file KhUpload 文件上传组件
 * @component KhUpload
 * @description 文件上传组件，支持按钮上传、拖拽上传、图片墙上传，自动携带 Token，支持文件大小校验和自定义上传。
 *              基于 Element Plus 的 el-upload 组件进行二次封装，提供统一的鉴权头注入、文件大小限制、
 *              以及灵活的自定义上传能力，适用于系统内各类文件上传场景。
 *
 * @props {string}   action        - 上传请求的接口地址，默认为 '/api/file/upload'
 * @props {Object}   headers       - 额外的自定义请求头，会与自动注入的 Authorization 头合并，默认为空对象 {}
 * @props {Object}   data          - 上传时附带的额外 FormData 参数，默认为空对象 {}
 * @props {string}   name          - 上传文件在 FormData 中的字段名，默认为 'file'
 * @props {string}   accept        - 接受的文件类型（MIME 类型），如 '.jpg,.png' 或 'image/*'，默认为空字符串（不限制）
 * @props {boolean}  multiple      - 是否允许同时选择多个文件进行上传，默认为 false
 * @props {boolean}  drag          - 是否启用拖拽上传模式，默认为 false（使用按钮模式）
 * @props {boolean}  disabled      - 是否禁用上传功能，默认为 false
 * @props {string}   listType      - 文件列表的展示类型，可选值：'text'（文字列表）、'picture'（图片列表）、'picture-card'（图片墙），默认为 'text'
 * @props {boolean}  autoUpload    - 是否在选取文件后立即自动上传，默认为 true；设为 false 时需手动调用 submit 方法触发上传
 * @props {Array}    fileList      - 文件列表（受控模式），传入后由外部控制文件列表的展示，默认为空数组 []
 * @props {number}   limit         - 最大允许上传的文件数量，默认为 0（不限制）
 * @props {number}   maxSize        - 单个文件的最大允许大小，单位为 MB，超过此大小将被拦截并提示，默认为 10
 * @props {boolean}  showFileList  - 是否在组件下方显示已上传的文件列表，默认为 true
 * @props {Function} customRequest  - 自定义上传方法，传入后将替代默认的上传行为，接收 el-upload 的 options 参数（含 file、onProgress、onSuccess、onError），默认为 null
 * @props {Function} beforeUpload  - 上传前的自定义校验钩子函数，接收原始 File 对象，返回 false 可阻止上传，默认为 null
 * @props {string}   buttonText    - 按钮模式下上传按钮上显示的文字内容，默认为 '点击上传'
 * @props {string}   buttonType    - 按钮模式下上传按钮的类型样式，对应 el-button 的 type 属性，默认为 'primary'
 * @props {string}   tip           - 上传区域下方显示的提示文字，默认为空字符串（不显示）
 *
 * @events success          - 文件上传成功时触发
 *   @param {Object} response - 服务端返回的响应数据
 *   @param {Object} file     - 上传成功的文件对象
 *   @param {Array}  fileList - 当前完整的文件列表
 *
 * @events error            - 文件上传失败时触发
 *   @param {Error}  error    - 上传过程中的错误对象
 *   @param {Object} file     - 上传失败的文件对象
 *   @param {Array}  fileList - 当前完整的文件列表
 *
 * @events remove           - 文件被移除时触发
 *   @param {Object} file     - 被移除的文件对象
 *   @param {Array}  fileList - 移除后的文件列表
 *
 * @events change           - 文件状态发生变化时触发（选取、上传中、上传成功/失败等）
 *   @param {Object} file     - 当前发生变化的文件对象
 *   @param {Array}  fileList - 当前完整的文件列表
 *
 * @events progress         - 文件上传过程中进度变化时触发
 *   @param {Object} event    - 包含 percent（进度百分比）等信息的进度事件对象
 *   @param {Object} file     - 正在上传的文件对象
 *
 * @events exceed           - 当文件数量超过 limit 限制时触发
 *   @param {Array}  files    - 本次尝试上传的超限文件列表
 *
 * @events preview          - 点击文件列表中的文件进行预览时触发
 *   @param {Object} file     - 被预览的文件对象
 *
 * @events file-list-change - 文件列表发生变化时触发
 *   @param {Array}  fileList - 变化后的完整文件列表
 *
 * @slots tip - 自定义上传提示信息区域，传入后将替代默认的 tip 文字渲染
 *
 * @exposed uploadRef - 获取内部 el-upload 组件实例的引用
 * @exposed submit    - 手动触发上传，适用于 autoUpload 为 false 的场景
 * @exposed clearFiles - 清空已上传的文件列表
 * @exposed abort     - 中止指定文件的上传任务
 */


// 图标引用（供模板 :icon 绑定使用）
const UploadIcon = Upload

/**
 * 组件属性定义
 * @description 定义 KhUpload 组件的全部可配置属性，基于 Element Plus el-upload 进行功能扩展，
 *              新增了 maxSize（文件大小校验）、customRequest（自定义上传）、beforeUpload（上传前钩子）等能力。
 */
const props = defineProps({
  /**
   * 上传请求的接口地址
   * @description 指定文件上传的目标后端接口 URL，该地址会作为 HTTP POST 请求的 action。
   *              默认值为 '/api/file/upload'，适用于系统内通用的文件上传接口。
   * @type {string}
   * @default '/api/file/upload'
   */
  action: {
    type: String,
    default: '/api/file/upload',
  },

  /**
   * 额外的自定义请求头
   * @description 允许外部传入额外的 HTTP 请求头，这些头信息会与组件自动注入的 Authorization（Bearer Token）头合并。
   *              合并时，同名外部头信息会覆盖内部自动注入的头。
   * @type {Object}
   * @default () => ({})
   */
  headers: {
    type: Object,
    default: () => ({}),
  },

  /**
   * 上传附带的额外参数
   * @description 上传文件时会将这些参数以 FormData 的形式一并提交到后端。
   *              适用于需要传递业务关联信息（如业务类型、关联 ID 等）的场景。
   * @type {Object}
   * @default () => ({})
   */
  data: {
    type: Object,
    default: () => ({}),
  },

  /**
   * 上传文件在 FormData 中的字段名
   * @description 指定文件在 FormData 中对应的 key 名称，后端需使用相同的字段名来接收文件。
   *              默认值为 'file'，需与后端接口的参数名保持一致。
   * @type {string}
   * @default 'file'
   */
  name: {
    type: String,
    default: 'file',
  },

  /**
   * 接受的文件类型
   * @description 限制用户可以选择的文件类型，值应为有效的 MIME 类型或文件扩展名，多个类型用逗号分隔。
   *              例如：'.jpg,.png' 或 'image/*' 或 '.pdf,.doc,.docx'。
   *              默认为空字符串，表示不限制文件类型。
   * @type {string}
   * @default ''
   */
  accept: {
    type: String,
    default: '',
  },

  /**
   * 是否允许多选文件
   * @description 控制用户是否可以同时选择多个文件进行上传。
   *              设为 true 时，文件选择对话框将允许选中多个文件。
   *              默认为 false，即每次只能选择一个文件。
   * @type {boolean}
   * @default false
   */
  multiple: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否启用拖拽上传模式
   * @description 设为 true 时，上传区域将变为可拖拽区域，用户可以直接将文件拖放到区域内完成上传。
   *              拖拽模式下，上传区域会显示图标和引导文字。
   *              默认为 false，使用普通按钮模式。
   * @type {boolean}
   * @default false
   */
  drag: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否禁用上传功能
   * @description 设为 true 后，上传按钮和拖拽区域都将被禁用，用户无法选择或上传文件。
   *              适用于某些业务条件下需要暂时禁止上传的场景。
   *              默认为 false。
   * @type {boolean}
   * @default false
   */
  disabled: {
    type: Boolean,
    default: false,
  },

  /**
   * 文件列表的展示类型
   * @description 控制已上传文件列表的视觉展示方式：
   *              - 'text'：默认值，以文字列表形式展示文件名、大小等信息；
   *              - 'picture'：以缩略图列表形式展示图片文件；
   *              - 'picture-card'：以图片墙（卡片网格）形式展示，每张图片为一个卡片，支持预览和删除。
   * @type {'text' | 'picture' | 'picture-card'}
   * @default 'text'
   */
  listType: {
    type: String,
    default: 'text',
  },

  /**
   * 是否在选取文件后立即自动上传
   * @description 设为 true（默认）时，用户选择文件后将自动发起上传请求；
   *              设为 false 时，需由外部调用 exposed 的 submit() 方法来手动触发上传。
   *              适用于需要在上传前执行额外操作（如填写表单）的场景。
   * @type {boolean}
   * @default true
   */
  autoUpload: {
    type: Boolean,
    default: true,
  },

  /**
   * 文件列表（受控模式）
   * @description 外部传入的文件列表数组，用于在受控模式下管理文件列表的展示。
   *              数组元素通常为包含 name、url、status 等字段的对象。
   *              默认为空数组。
   * @type {Array}
   * @default () => []
   */
  fileList: {
    type: Array,
    default: () => [],
  },

  /**
   * 最大允许上传的文件数量
   * @description 限制组件可上传的文件总数。当已上传文件数达到此限制后，将阻止新的上传并触发 exceed 事件。
   *              默认为 0，表示不限制文件数量。
   * @type {number}
   * @default 0
   */
  limit: {
    type: Number,
    default: 0,
  },

  /**
   * 单个文件的最大允许大小
   * @description 限制单个上传文件的大小，单位为兆字节（MB）。超过此大小的文件将在上传前被拦截，
   *              并向用户显示错误提示消息。内部通过 file.size / 1024 / 1024 进行换算。
   *              默认为 10MB。
   * @type {number}
   * @default 10
   */
  maxSize: {
    type: Number,
    default: 10,
  },

  /**
   * 是否显示已上传的文件列表
   * @description 控制是否在组件下方展示已上传（或待上传）的文件列表。
   *              设为 false 可隐藏文件列表，适用于仅需触发上传而不需要展示文件状态的场景。
   *              默认为 true。
   * @type {boolean}
   * @default true
   */
  showFileList: {
    type: Boolean,
    default: true,
  },

  /**
   * 自定义上传方法
   * @description 传入一个函数以替代默认的 HTTP 上传行为。函数接收 el-upload 提供的 options 对象，
   *              包含 file（原始文件对象）、onProgress（进度回调）、onSuccess（成功回调）、onError（失败回调）。
   *              适用于需要对接 OSS、七牛云等第三方对象存储，或需要特殊上传逻辑的场景。
   *              默认为 null，表示使用默认的 HTTP 上传方式。
   * @type {Function | null}
   * @default null
   */
  customRequest: {
    type: Function,
    default: null,
  },

  /**
   * 上传前的自定义校验钩子函数
   * @description 在文件上传前执行的校验函数，接收原始 File 对象作为参数。
   *              返回 false 可以阻止本次上传。可用于实现文件类型、内容等业务层面的自定义校验。
   *              注意：此钩子会在组件内置的文件大小校验之前执行。
   *              默认为 null，表示不做额外校验。
   * @type {Function | null}
   * @default null
   */
  beforeUpload: {
    type: Function,
    default: null,
  },

  /**
   * 按钮模式下上传按钮的文字内容
   * @description 仅在非拖拽模式且非 picture-card 模式下生效，显示在按钮上。
   *              默认为 '点击上传'。
   * @type {string}
   * @default '点击上传'
   */
  buttonText: {
    type: String,
    default: '点击上传',
  },

  /**
   * 按钮模式下上传按钮的类型样式
   * @description 对应 Element Plus el-button 组件的 type 属性，决定按钮的颜色主题。
   *              可选值包括 'primary'、'success'、'warning'、'danger'、'info' 等。
   *              默认为 'primary'（主要按钮，蓝色）。
   * @type {string}
   * @default 'primary'
   */
  buttonType: {
    type: String,
    default: 'primary',
  },

  /**
   * 上传区域下方的提示文字
   * @description 在上传按钮或拖拽区域下方显示的辅助说明文字，用于提示用户上传的限制条件或注意事项。
   *              默认为空字符串，表示不显示提示。
   *              也可通过具名插槽 tip 完全自定义提示区域的内容。
   * @type {string}
   * @default ''
   */
  tip: {
    type: String,
    default: '',
  },
})

/**
 * 组件事件定义
 * @description 定义 KhUpload 组件对外暴露的所有事件，涵盖上传生命周期中的各个关键节点：
 *              文件选择、校验、进度、成功、失败、移除、预览、超限等。
 */
const emit = defineEmits([
  /** @event success - 文件上传成功时触发，携带服务端响应数据、文件对象和文件列表 */
  'success',
  /** @event error - 文件上传失败时触发，携带错误对象、文件对象和文件列表 */
  'error',
  /** @event remove - 文件从列表中移除时触发，携带被移除的文件对象和剩余文件列表 */
  'remove',
  /** @event change - 文件状态发生变化时触发（包括选取、上传中、成功、失败），携带当前文件对象和文件列表 */
  'change',
  /** @event progress - 文件上传进度更新时触发，携带进度事件对象和当前文件对象 */
  'progress',
  /** @event exceed - 文件数量超出 limit 限制时触发，携带本次尝试上传的超限文件数组 */
  'exceed',
  /** @event preview - 点击预览文件时触发，携带被预览的文件对象 */
  'preview',
  /** @event file-list-change - 文件列表内容发生变化时触发，携带变化后的完整文件列表 */
  'file-list-change',
])

/**
 * el-upload 组件实例引用
 * @description 通过 template ref 获取 Element Plus el-upload 组件的内部实例，
 *              用于调用其暴露的 submit、clearFiles、abort 等方法。
 * @type {import('vue').Ref<import('element-plus').UploadInstance | null>}
 */
const uploadRef = ref(null)

/**
 * 图片预览弹窗的显示状态
 * @description 控制图片预览对话框（el-dialog）的打开/关闭状态。
 *              当用户在 picture-card 或 picture 模式下点击文件预览时，此值会被设为 true。
 * @type {import('vue').Ref<boolean>}
 */
const dialogVisible = ref(false)

/**
 * 当前预览图片的 URL 地址
 * @description 存储用户点击预览的图片文件的 URL，用于在预览弹窗中展示图片。
 * @type {import('vue').Ref<string>}
 */
const dialogImageUrl = ref('')

/**
 * 计算属性：上传请求头（自动携带 Token）
 * @description 自动从 localStorage 中读取 token，并构造包含 Bearer Token 的 Authorization 请求头。
 *              外部通过 headers 属性传入的额外请求头会被展开合并到最终结果中（后者覆盖前者）。
 *              确保所有上传请求都自动携带用户身份凭证，无需手动维护鉴权信息。
 * @returns {Object} 合并后的请求头对象，包含 Authorization 和自定义 headers
 */
const uploadHeaders = computed(() => {
  const token = localStorage.getItem('token')
  return {
    Authorization: token ? `Bearer ${token}` : '',
    ...props.headers,
  }
})

/**
 * 处理文件数量超出限制
 * @description 当用户尝试上传的文件数量超过 limit 属性设定的最大值时，由 el-upload 内部调用此回调。
 *              函数会向外部触发 exceed 事件，并使用 ElMessage 显示警告提示。
 * @param {Array<File>} files - 本次尝试上传的超出限制的文件数组
 */
function handleExceed(files) {
  emit('exceed', files)
  KhMessageFn.warning(`最多只能上传 ${props.limit} 个文件`)
}

/**
 * 上传前的校验处理
 * @description 在文件正式上传之前执行校验逻辑，包含两个阶段：
 *              1. 外部自定义校验：若 props.beforeUpload 存在，先调用外部校验函数，若返回 false 则直接阻止上传；
 *              2. 文件大小校验：检查文件大小是否超过 props.maxSize（单位 MB），超限时显示错误提示并阻止上传。
 *              两个校验均通过后才允许上传。
 * @param {File} file - 待上传的原始文件对象，包含 name、size、type 等属性
 * @returns {boolean} 返回 true 表示校验通过允许上传，返回 false 表示校验未通过阻止上传
 */
function handleBeforeUpload(file) {
  // 外部自定义校验
  if (props.beforeUpload) {
    const result = props.beforeUpload(file)
    if (result === false) return false
  }
  // 文件大小校验
  const isOverSize = file.size / 1024 / 1024 > props.maxSize
  if (isOverSize) {
    KhMessageFn.error(`文件大小不能超过 ${props.maxSize}MB`)
    return false
  }
  return true
}

/**
 * 处理上传进度变化
 * @description 在文件上传过程中，当上传进度发生变化时由 el-upload 内部调用此回调。
 *              将进度信息透传给外部，方便父组件展示自定义进度条或处理进度相关逻辑。
 * @param {Object} event - 上传进度事件对象，其中 event.percent 为当前上传进度百分比（0-100）
 * @param {Object} file  - 当前正在上传的文件对象
 */
function handleProgress(event, file) {
  emit('progress', event, file)
}

/**
 * 处理文件上传成功
 * @description 当文件上传请求成功完成时由 el-upload 内部调用此回调。
 *              函数会先使用 ElMessage 显示"上传成功"提示，然后将成功信息（响应数据、文件对象、文件列表）
 *              透传给外部，方便父组件处理业务逻辑（如更新数据、刷新列表等）。
 * @param {Object} response - 服务端返回的响应数据对象
 * @param {Object} file     - 上传成功的文件对象
 * @param {Array}  fileList - 当前完整的文件列表（包含本次上传成功的文件）
 */
function handleSuccess(response, file, fileList) {
  KhMessageFn.success('上传成功')
  emit('success', response, file, fileList)
}

/**
 * 处理文件上传失败
 * @description 当文件上传请求失败（网络错误、服务端异常等）时由 el-upload 内部调用此回调。
 *              函数会先使用 ElMessage 显示"上传失败"提示，然后将错误信息透传给外部，
 *              方便父组件进行错误处理或重试逻辑。
 * @param {Error}  error    - 上传过程中的错误对象，包含错误信息和堆栈
 * @param {Object} file     - 上传失败的文件对象
 * @param {Array}  fileList - 当前完整的文件列表
 */
function handleError(error, file, fileList) {
  KhMessageFn.error('上传失败')
  emit('error', error, file, fileList)
}

/**
 * 处理文件移除
 * @description 当用户从文件列表中移除某个文件时由 el-upload 内部调用此回调。
 *              将被移除的文件对象和更新后的文件列表透传给外部，
 *              方便父组件同步更新业务数据（如删除后端已上传的文件记录）。
 * @param {Object} file     - 被移除的文件对象
 * @param {Array}  fileList - 移除后的文件列表
 */
function handleRemove(file, fileList) {
  emit('remove', file, fileList)
}

/**
 * 处理文件预览
 * @description 当用户点击文件列表中的文件进行预览时由 el-upload 内部调用此回调。
 *              如果当前列表类型为 picture-card 或 picture 模式，会打开内置的图片预览弹窗，
 *              展示该文件的缩略图。同时将预览事件透传给外部，方便父组件实现自定义预览逻辑。
 * @param {Object} file - 被点击预览的文件对象，其 url 属性为文件的访问地址
 */
function handlePreview(file) {
  if (props.listType === 'picture-card' || props.listType === 'picture') {
    dialogImageUrl.value = file.url
    dialogVisible.value = true
  }
  emit('preview', file)
}

/**
 * 处理文件状态变化
 * @description 当文件列表中的某个文件状态发生变化时由 el-upload 内部调用此回调。
 *              文件状态变化包括：文件被选中、开始上传、上传中、上传成功、上传失败等。
 *              将变化信息透传给外部，方便父组件实时响应文件状态更新。
 * @param {Object} file     - 当前状态发生变化的文件对象
 * @param {Array}  fileList - 当前完整的文件列表
 */
function handleChange(file, fileList) {
  emit('change', file, fileList)
}

/**
 * 自定义上传处理
 * @description 当外部通过 customRequest 属性传入了自定义上传函数时，由 el-upload 调用此回调。
 *              函数将 el-upload 提供的 options 对象（包含 file、onProgress、onSuccess、onError）
 *              直接传递给外部自定义上传函数，由外部完全控制上传过程。
 *              适用于对接第三方云存储（如阿里云 OSS、腾讯云 COS、七牛云等）或需要特殊上传协议的场景。
 * @param {Object} options - el-upload 提供的上传选项对象
 * @param {File}   options.file       - 待上传的原始文件对象
 * @param {Function} options.onProgress - 上传进度回调函数，接收包含 percent 属性的事件对象
 * @param {Function} options.onSuccess  - 上传成功回调函数，接收服务端响应数据
 * @param {Function} options.onError    - 上传失败回调函数，接收错误对象
 */
function handleCustomRequest(options) {
  if (props.customRequest) {
    props.customRequest(options)
  }
}

/**
 * 暴露给父组件的方法和属性
 * @description 通过 defineExpose 将组件内部的 key 方法和引用暴露给父组件，
 *              父组件可通过 template ref 调用这些方法来控制上传行为。
 */
defineExpose({
  /**
   * el-upload 组件实例引用
   * @description 提供对内部 el-upload 组件实例的直接访问，可用于调用 el-upload 暴露的任何方法
   *              或访问其内部状态。通常建议优先使用下方封装好的方法。
   * @type {import('vue').Ref<import('element-plus').UploadInstance | null>}
   */
  uploadRef,

  /**
   * 手动触发上传
   * @description 在 autoUpload 设为 false 的情况下，调用此方法可手动触发所有待上传文件的上传。
   *              内部委托给 el-upload 实例的 submit 方法执行。
   * @returns {void}
   */
  submit: () => uploadRef.value?.submit(),

  /**
   * 清空已上传的文件列表
   * @description 清空组件内部的文件列表，移除所有已上传和待上传的文件。
   *              内部委托给 el-upload 实例的 clearFiles 方法执行。
   * @returns {void}
   */
  clearFiles: () => uploadRef.value?.clearFiles(),

  /**
   * 中止指定文件的上传任务
   * @description 中止正在上传中的指定文件的上传请求。
   *              内部委托给 el-upload 实例的 abort 方法执行。
   * @param {Object} [file] - 需要中止上传的文件对象。如果不传，则中止所有正在上传的文件。
   * @returns {void}
   */
  abort: (file) => uploadRef.value?.abort(file),
})
</script>
