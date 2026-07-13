<template>
  <div ref="triggerRef" class="kh-icon-picker">
    <!-- 触发器 -->
    <div
      class="kh-icon-picker__input"
      :class="{ 'is-focus': visible, 'is-disabled': disabled }"
      @click.stop="toggle"
    >
      <span v-if="modelValue" class="kh-icon-picker__preview">
        <el-icon :size="16"><component :is="modelValue" /></el-icon>
      </span>
      <span class="kh-icon-picker__text" :class="{ 'is-placeholder': !modelValue }">
        {{ modelValue || placeholder || '请选择图标' }}
      </span>
      <span v-if="modelValue && !disabled" class="kh-icon-picker__clear" @click.stop="handleClear">
        <el-icon :size="14"><CircleClose /></el-icon>
      </span>
      <span class="kh-icon-picker__arrow">
        <el-icon :size="14"><ArrowDown /></el-icon>
      </span>
    </div>

    <!-- 下拉面板（Teleport 到 body 避免 overflow 裁剪） -->
    <Teleport to="body">
      <Transition name="kh-fade">
        <div
          v-if="visible"
          ref="panelRef"
          class="kh-icon-picker__panel"
          :style="panelStyle"
          @mousedown.stop
        >
          <div class="kh-icon-picker__search">
            <el-input
              v-model="keyword"
              placeholder="搜索图标..."
              clearable
              size="small"
              :prefix-icon="SearchIcon"
            />
          </div>
          <div class="kh-icon-picker__list">
            <div
              v-for="icon in filteredIcons"
              :key="icon"
              class="kh-icon-picker__item"
              :class="{ 'is-active': modelValue === icon }"
              :title="icon"
              @click="handleSelect(icon)"
            >
              <el-icon :size="18">
                <component :is="icon" />
              </el-icon>
            </div>
            <div v-if="filteredIcons.length === 0" class="kh-icon-picker__empty">
              无匹配图标
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
// 图标引用（供模板 :prefix-icon 绑定使用）
const SearchIcon = Search

const props = defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  disabled: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'change'])

const triggerRef = ref(null)
const panelRef = ref(null)
const visible = ref(false)
const keyword = ref('')

const iconList = [
  'AddLocation', 'Aim', 'AlarmClock', 'Apple', 'ArrowDown', 'ArrowDownBold', 'ArrowLeft',
  'ArrowLeftBold', 'ArrowRight', 'ArrowRightBold', 'ArrowUp', 'ArrowUpBold',
  'Avatars', 'Back', 'Baseball', 'Basketball', 'Bell', 'BellFilled',
  'Box', 'Bicycle', 'Briefcase', 'Brush', 'Burger', 'Calendar', 'Camera',
  'CameraFilled', 'CaretBottom', 'CaretLeft', 'CaretRight', 'CaretTop',
  'Cellphone', 'ChatDotRound', 'ChatDotSquare', 'ChatLineRound', 'ChatLineSquare',
  'ChatRound', 'ChatSquare', 'Check', 'Checked', 'Cherry', 'Chicken',
  'CircleCheck', 'CircleCheckFilled', 'CircleClose', 'CircleCloseFilled',
  'CirclePlus', 'CirclePlusFilled', 'Clock', 'Close', 'CloseBold',
  'Cloudy', 'Coffee', 'CoffeeCup', 'Coin', 'ColdDrink', 'Collection',
  'CollectionTag', 'Comment', 'Compass', 'Connection', 'Coordinate',
  'CopyDocument', 'Cpu', 'CreditCard', 'Crop', 'DArrowLeft', 'DArrowRight',
  'DCaret', 'Delete', 'DeleteFilled', 'DeleteLocation', 'Discount',
  'Document', 'DocumentAdd', 'DocumentChecked', 'DocumentCopy', 'DocumentDelete',
  'DocumentRemove', 'Download', 'Drizzle', 'Edit', 'EditPen',
  'Eleme', 'ElemeFilled', 'Element', 'ElementPlus', 'Expand', 'Failed',
  'Female', 'Files', 'Film', 'Filter', 'Finished', 'Flag', 'Fold',
  'Folder', 'FolderAdd', 'FolderChecked', 'FolderDelete', 'FolderOpened',
  'FolderRemove', 'Food', 'Football', 'ForkSpoon', 'Fries', 'FullScreen',
  'Goods', 'GoodsFilled', 'Grape', 'Grid', 'Guide', 'Handbag', 'Headset',
  'Help', 'HelpFilled', 'Hide', 'Histogram', 'HomeFilled', 'HotWater',
  'House', 'IceCream', 'IceCreamRound', 'IceCreamSquare', 'IceDrink',
  'InfoFilled', 'Iphone', 'Key', 'KnifeFork', 'Lollipop',
  'Lock', 'Location', 'LocationFilled', 'LocationInformation', 'MagicStick',
  'Male', 'Management', 'MapLocation', 'Memo', 'Menu', 'Message',
  'Microphone', 'MilkTea', 'Minus', 'Monitor', 'Moon', 'MoonNight',
  'More', 'MoreFilled', 'MostlyCloudy', 'Mouse', 'Mug', 'MuteNotification',
  'NoSmoking', 'Notebook', 'Notification', 'Odometer', 'OfficeBuilding',
  'Open', 'Operation', 'Opportunity', 'Orange', 'Paperclip', 'PartlyCloudy',
  'Pear', 'Phone', 'PhoneFilled', 'Picture', 'PictureFilled', 'PictureRounded',
  'PieChart', 'Place', 'Platform', 'Plus', 'Pointer', 'Position',
  'Postcard', 'Pouring', 'Present', 'PriceTag', 'Printer', 'Promotion',
  'QuestionFilled', 'Rank', 'Reading', 'RefreshLeft', 'RefreshRight',
  'Refrigerator', 'Remove', 'RemoveFilled', 'Right', 'ScaleToOriginal',
  'School', 'Scissor', 'Search', 'Select', 'Sell', 'Semaphore',
  'Service', 'SetUp', 'Setting', 'Share', 'Ship', 'Shop',
  'ShoppingBag', 'ShoppingCart', 'ShoppingCartFull', 'ShoppingTrolley',
  'Smoking', 'Snowy', 'Soccer', 'SoldOut', 'Sort', 'SortDown', 'SortUp',
  'Stamp', 'Star', 'StarFilled', 'Stopwatch', 'SuccessFilled', 'Suitcase',
  'Sunny', 'Sunrise', 'Switch', 'SwitchButton', 'SwitchFilled',
  'TakeawayBox', 'Ticket', 'Tickets', 'Timer', 'Toast', 'Tofu',
  'TopLeft', 'TopRight', 'TrendCharts', 'Trophy', 'TrophyBase', 'TurnOff',
  'Umbrella', 'Unlock', 'Upload', 'UploadFilled', 'User', 'UserFilled',
  'Van', 'VideoCamera', 'VideoCameraFilled', 'VideoPause', 'VideoPlay',
  'View', 'Wallet', 'WarnTriangleFilled', 'Warning', 'WarningFilled',
  'Watch', 'Watermelon', 'WindPower', 'Window', 'ZoomIn', 'ZoomOut',
]

const filteredIcons = computed(() => {
  if (!keyword.value) return iconList
  const kw = keyword.value.toLowerCase()
  return iconList.filter(name => name.toLowerCase().includes(kw))
})

const panelStyle = computed(() => {
  if (!triggerRef.value) return { display: 'none' }
  const width = triggerRef.value.getBoundingClientRect().width
  return {
    position: 'fixed',
    top: `${panelTop.value}px`,
    left: `${panelLeft.value}px`,
    zIndex: 9999,
    minWidth: `${Math.max(width, 360)}px`,
  }
})

// 面板可见时，跟随触发器位置重新计算（响应式依赖 triggerRef 布局变化）
const panelTop = ref(0)
const panelLeft = ref(0)
const updatePanelPosition = () => {
  if (!triggerRef.value) return
  const rect = triggerRef.value.getBoundingClientRect()
  panelTop.value = rect.bottom + 6
  panelLeft.value = rect.left
}

function toggle() {
  if (props.disabled) return
  keyword.value = ''
  if (!visible.value) updatePanelPosition()
  visible.value = !visible.value
}

function close() {
  visible.value = false
}

function handleSelect(icon) {
  emit('update:modelValue', icon)
  emit('change', icon)
  close()
}

function handleClear() {
  emit('update:modelValue', '')
  emit('change', '')
}

// 点击外部关闭
function handleClickOutside(e) {
  if (!visible.value) return
  const trigger = triggerRef.value
  const panel = panelRef.value
  if (trigger?.contains(e.target) || panel?.contains(e.target)) return
  close()
}

// 滚动时重新定位面板（而非直接关闭），避免弹窗内滚动导致面板消失
function handleScroll() {
  if (visible.value) updatePanelPosition()
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  window.addEventListener('scroll', handleScroll, true)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  window.removeEventListener('scroll', handleScroll, true)
})
</script>

<style scoped>
.kh-icon-picker {
  width: 100%;
}

.kh-icon-picker__input {
  display: flex;
  align-items: center;
  height: 32px;
  padding: 0 30px 0 11px;
  border: 1px solid var(--el-border-color);
  border-radius: var(--el-border-radius-base);
  background-color: var(--el-fill-color-blank);
  transition: border-color 0.2s;
  position: relative;
  box-sizing: border-box;
  cursor: pointer;
}

.kh-icon-picker__input:hover {
  border-color: var(--el-border-color-hover);
}

.kh-icon-picker__input.is-focus {
  border-color: var(--el-color-primary);
}

.kh-icon-picker__input.is-disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.kh-icon-picker__preview {
  display: inline-flex;
  align-items: center;
  margin-right: 6px;
  color: var(--el-text-color-regular);
  flex-shrink: 0;
}

.kh-icon-picker__text {
  flex: 1;
  font-size: 14px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 32px;
}

.kh-icon-picker__text.is-placeholder {
  color: var(--el-text-color-placeholder);
}

.kh-icon-picker__clear {
  position: absolute;
  right: 25px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--el-text-color-placeholder);
  cursor: pointer;
  display: flex;
  align-items: center;
}

.kh-icon-picker__clear:hover {
  color: var(--el-text-color-secondary);
}

.kh-icon-picker__arrow {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--el-text-color-placeholder);
  display: flex;
  align-items: center;
}

.kh-icon-picker__panel {
  background: var(--el-bg-color-overlay);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: var(--el-border-radius-base);
  box-shadow: var(--el-box-shadow-light);
  padding: 14px;
  max-height: 380px;
  display: flex;
  flex-direction: column;
}

.kh-icon-picker__search {
  padding-bottom: 10px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  margin-bottom: 8px;
}

.kh-icon-picker__list {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 4px;
  max-height: 300px;
  overflow-y: auto;
}

.kh-icon-picker__item {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
  color: var(--el-text-color-regular);
}

.kh-icon-picker__item:hover {
  background-color: var(--el-fill-color-light);
  color: var(--el-color-primary);
}

.kh-icon-picker__item.is-active {
  background-color: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
  border: 1px solid var(--el-color-primary-light-5);
}

.kh-icon-picker__empty {
  grid-column: 1 / -1;
  text-align: center;
  color: var(--el-text-color-placeholder);
  padding: 20px 0;
  font-size: 13px;
}
</style>
