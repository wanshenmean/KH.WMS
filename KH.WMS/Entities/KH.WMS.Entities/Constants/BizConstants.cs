namespace KH.WMS.Entities.Constants;

/// <summary>
/// 业务常量集合，定义系统中各业务模块使用的常量值。
/// 所有常量均为字符串类型，用于单据状态、任务类型、编码规则等业务场景。
/// </summary>
public static class BizConstants
{
    /// <summary>
    /// 任务类型，用于区分仓库内不同类型的作业任务。
    /// 任务创建时根据业务场景自动指定类型，决定任务的处理逻辑和执行路径。
    /// </summary>
    public static class TaskTypes
    {
        /// <summary>
        /// 上架任务。物料完成入库收货或组盘后，需要从暂存区搬运到指定库位进行存放时生成。
        /// 上架目标库位由上架策略（如分类分区、集中存放等）自动计算或手动指定。
        /// </summary>
        public const string PUTAWAY = "PUTAWAY";

        /// <summary>
        /// 拣货任务。出库波次释放后，根据拣货策略生成拣货任务，指导人员或设备从库位取出物料。
        /// 拣货顺序由拣货策略（如按库位路径最短、先进先出等）确定。
        /// </summary>
        public const string PICKING = "PICKING";

        /// <summary>
        /// 移库任务。将物料从一个库位转移到另一个库位，通常用于库位整理、区域调整、
        /// 库位容量优化等场景。可通过手工创建移库计划或系统自动触发。
        /// </summary>
        public const string TRANSFER = "TRANSFER";

        /// <summary>
        /// 盘点任务。根据盘点计划生成，指导人员到指定库位进行实物盘点，
        /// 盘点结果与系统账面库存进行比对，生成盘盈盘亏差异记录。
        /// </summary>
        public const string STOCKTAKE = "STOCKTAKE";
    }

    /// <summary>
    /// 单据类型，用于标识系统中不同业务单据的分类。
    /// 在编码规则、单据状态流转、单据字段配置等场景中使用。
    /// </summary>
    public static class DocTypes
    {
        /// <summary>
        /// 采购入库单。需要收货→组盘→上架的完整流程。
        /// </summary>
        public const string INBOUND_ORDER_PURCHASE = "INBOUND_ORDER_PURCHASE";

        /// <summary>
        /// 生产入库单。无需收货，可直接组盘→上架。
        /// </summary>
        public const string INBOUND_ORDER_PRODUCTION = "INBOUND_ORDER_PRODUCTION";

        /// <summary>
        /// 退货入库单。需要收货（含质检）→组盘→上架。
        /// </summary>
        public const string INBOUND_ORDER_RETURN = "INBOUND_ORDER_RETURN";

        /// <summary>
        /// 入库组盘单。记录入库物料与容器（托盘/料箱）的绑定关系，
        /// 一个入库单可对应多个组盘记录，每组盘可绑定多行物料明细。
        /// 组盘完成后生成上架任务。
        /// </summary>
        public const string INBOUND_CONTAINER_BIND = "INBOUND_CONTAINER_BIND";

        /// <summary>
        /// 出库单。记录出库业务的核心单据，包含客户、优先级、物料明细及需求数量等信息。
        /// 出库单审核释放后可生成出库波次。
        /// </summary>
        public const string OUTBOUND_ORDER = "OUTBOUND_ORDER";

        /// <summary>
        /// 出库波次。将多个出库单按一定规则（如客户、紧急程度、配送路线等）合并为一个波次，
        /// 统一进行库存分配和拣货任务生成，提高拣货效率。
        /// </summary>
        public const string OUT_WAVE = "OUT_WAVE";

        /// <summary>
        /// 移库计划。记录库位间物料转移的计划信息，包含源库位、目标库位、转移物料明细等。
        /// 移库计划审核后生成移库任务。
        /// </summary>
        public const string TRANSFER_PLAN = "TRANSFER_PLAN";

        /// <summary>
        /// 临时移库单。由临时移库操作自动生成的单据类型，不走标准移库计划流程。
        /// </summary>
        public const string ADHOC_TRANSFER = "ADHOC_TRANSFER";

        /// <summary>
        /// 临时入库单。由临时入库操作自动生成的单据类型，不走标准入库流程。
        /// </summary>
        public const string ADHOC_INBOUND = "ADHOC_INBOUND";

        /// <summary>
        /// 临时出库单。由临时出库操作自动生成的单据类型，不走标准出库流程。
        /// </summary>
        public const string ADHOC_OUTBOUND = "ADHOC_OUTBOUND";
    }

    /// <summary>
    /// 来源类型，用于标识任务或操作的来源，便于追溯任务是由哪个业务模块触发的。
    /// </summary>
    public static class SourceTypes
    {
        /// <summary>
        /// 来自入库业务。当来源类型为 INBOUND 时，表示该任务由入库流程触发，
        /// 例如入库组盘完成后自动生成的上架任务。
        /// </summary>
        public const string INBOUND = "INBOUND";

        /// <summary>
        /// 来自移库业务。当来源类型为 TRANSFER 时，表示该任务由移库计划触发，
        /// 例如移库计划审核后生成的移库搬运任务。
        /// </summary>
        public const string TRANSFER = "TRANSFER";

        /// <summary>
        /// 来自盘点业务。当来源类型为 STOCKTAKE 时，表示该任务由盘点计划触发，
        /// 例如盘点计划生成的盘点任务，用于指导人员到指定库位进行实物清点。
        /// </summary>
        public const string STOCKTAKE = "STOCKTAKE";

        /// <summary>
        /// 临时操作。由 AdhocService 触发的临时移库、入库、出库等操作。
        /// </summary>
        public const string ADHOC = "ADHOC";
    }

    /// <summary>
    /// 入库订单类型，用于区分不同入库业务场景。
    /// 不同类型的入库单在审核流程、质检要求、上架策略上可能有差异。
    /// </summary>
    public static class OrderTypes
    {
        /// <summary>
        /// 采购入库。由采购订单驱动的入库业务，物料来自供应商，
        /// 通常需要经过收货质检确认后方可入库。
        /// </summary>
        public const string PURCHASE_IN = "PURCHASE_IN";

        /// <summary>
        /// 退货入库。客户退回的物料入库，需要重新登记库存，
        /// 通常需要质检确认物料状态后再决定是否重新上架或报废。
        /// </summary>
        public const string RETURN_INBOUND = "RETURN_INBOUND";

        /// <summary>
        /// 生产入库。生产完成后的成品或半成品入库，物料来自生产线，
        /// 通常与生产工单关联。
        /// </summary>
        public const string PRODUCTION_IN = "PRODUCTION_IN";

        /// <summary>
        /// 其他入库。不属于以上类型的入库业务，如赠品入库、调拨入库、
        /// 初始化库存录入等特殊场景。
        /// </summary>
        public const string OTHER_INBOUND = "OTHER_INBOUND";

        /// <summary>
        /// 该入库类型是否需要先收货才能组盘。
        /// 采购入库需经收货质检确认后再组盘；生产/退货/其他入库可直接组盘（不经过收货环节）。
        /// </summary>
        public static bool IsReceiveRequired(string? orderType) => orderType == PURCHASE_IN;
    }

    /// <summary>
    /// 编码规则类型，对应 ICodeGeneratorService 的 ruleType 参数。
    /// 每种业务单据使用不同的编码规则生成唯一单号，确保单号可识别业务类型。
    /// </summary>
    public static class CodeRuleTypes
    {
        /// <summary>
        /// 入库单编码。用于生成入库单号，通常包含前缀（如 RK）、日期、流水号，
        /// 例如 RK202605260001。
        /// </summary>
        public const string INBOUND_DOC = "INBOUND_DOC";

        /// <summary>
        /// 出库单编码。用于生成出库单号，通常包含前缀（如 CK）、日期、流水号，
        /// 例如 CK202605260001。
        /// </summary>
        public const string OUTBOUND_DOC = "OUTBOUND_DOC";

        /// <summary>
        /// 上架任务编码。用于生成上架任务号，通常包含前缀（如 SJ）、日期、流水号，
        /// 以区分不同类型的任务单据。
        /// </summary>
        public const string TASK_PUTAWAY = "TASK_PUTAWAY";

        /// <summary>
        /// 拣货任务编码。用于生成拣货任务号，通常包含前缀（如 JH）、日期、流水号，
        /// 以区分不同类型的任务单据。
        /// </summary>
        public const string TASK_PICKING = "TASK_PICKING";

        /// <summary>
        /// 移库任务编码。用于生成移库任务号，通常包含前缀（如 YK）、日期、流水号，
        /// 以区分不同类型的任务单据。
        /// </summary>
        public const string TASK_TRANSFER = "TASK_TRANSFER";

        /// <summary>
        /// 任务编号。用于生成通用任务单号。
        /// </summary>
        public const string TASK_NO = "TASK_NO";
    }

    /// <summary>
    /// 确认方式，用于标识任务的确认模式。
    /// 决定任务是自动确认还是需要人工手动确认。
    /// </summary>
    public static class ConfirmTypes
    {
        /// <summary>
        /// 自动确认。任务执行完成后系统自动确认，无需人工干预。
        /// 适用于自动化设备（如堆垛机、AGV）完成的任务，设备反馈完成后自动更新任务状态。
        /// </summary>
        public const string AUTO = "AUTO";

        /// <summary>
        /// 手动确认。任务执行完成后需要操作人员在终端（PDA/PC）上手动确认，
        /// 适用于人工参与的作业场景，确保操作人员核实后才更新状态。
        /// </summary>
        public const string MANUAL = "MANUAL";
    }

    /// <summary>
    /// 通用状态，用于字符串类型的状态字段，描述单据在其生命周期中所处的阶段。
    /// 适用于多种单据类型（入库单、出库单、移库计划等）的通用状态流转。
    /// </summary>
    public static class Status
    {
        /// <summary>
        /// 草稿。单据刚创建尚未提交审核，此状态下可自由修改单据内容，
        /// 不影响实际库存和任务。
        /// </summary>
        public const string DRAFT = "DRAFT";

        /// <summary>
        /// 待处理。单据已提交但尚未开始执行，等待审核或等待资源分配。
        /// 此状态下部分字段可能已锁定不可修改。
        /// </summary>
        public const string PENDING = "PENDING";

        /// <summary>
        /// 已确认。单据已通过审核确认，可以进入后续执行环节。
        /// 例如入库单确认后可开始收货，出库单确认后可生成波次。
        /// </summary>
        public const string CONFIRMED = "CONFIRMED";

        /// <summary>
        /// 已完成。单据关联的所有业务操作均已结束，单据关闭不可再修改。
        /// 例如入库单完成表示所有物料已上架，出库单完成表示所有物料已发货。
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// 已取消。单据被主动取消，不再执行。取消前需检查是否已有关联任务在执行中，
        /// 已生成任务的需先取消任务才能取消单据。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 库位物理状态，仅反映库位上是否存在物料。
    /// <para>
    /// 库位是否可分配由三个条件共同决定：
    /// Status==EMPTY、LockStatus==NONE、IsDisabled==0
    /// </para>
    /// </summary>
    public static class LocationStatus
    {
        /// <summary>
        /// 空闲。库位当前无任何物料存放，可以被分配用于上架或移库的目标库位。
        /// </summary>
        public const string EMPTY = "EMPTY";

        /// <summary>
        /// 占用。库位上已存放物料，不能直接分配新的上架任务到此库位。
        /// 部分仓库支持同一库位存放多个容器（混放），此时占用不代表库位已满。
        /// </summary>
        public const string OCCUPIED = "OCCUPIED";
    }

    /// <summary>
    /// 库位锁定状态，标识库位在自动流程中为何被锁定。
    /// <para>LockStatus=0 表示未锁定，库位可正常参与分配。</para>
    /// <para>LockStatus≠0 时库位不参与分配，具体值由系统根据任务类型自动设置和释放。</para>
    /// </summary>
    public static class LocationLockStatus
    {
        /// <summary>
        /// 未锁定，库位可正常参与分配。
        /// </summary>
        public const byte NONE = 0;

        /// <summary>
        /// 入库预占。上架任务分配库位时自动设置，任务完成或取消后自动释放。
        /// </summary>
        public const byte INBOUND_RESERVED = 1;

        /// <summary>
        /// 出库锁定。拣货任务分配库位时自动设置，任务完成或取消后自动释放。
        /// </summary>
        public const byte OUTBOUND_LOCKED = 2;

        /// <summary>
        /// 盘点冻结。盘点任务开始时自动设置，盘点完成后自动释放。
        /// </summary>
        public const byte STOCKTAKE_FREEZE = 3;
    }

    /// <summary>
    /// 任务状态，描述一个仓库任务从创建到结束的完整生命周期。
    /// 任务状态由系统根据执行进度自动推进，也可在异常情况下手动干预。
    /// </summary>
    public static class TaskStatus
    {
        /// <summary>
        /// 待执行。任务已创建但尚未开始执行，等待被调度系统分配给人员或设备。
        /// 可按优先级排序，优先级高的任务先被执行。
        /// </summary>
        public const string PENDING = "PENDING";

        /// <summary>
        /// 执行中。任务已被领取或下发到执行端（PDA/AGV/堆垛机），正在执行搬运或操作。
        /// 执行中的任务不可取消，需等待完成或强制终止。
        /// </summary>
        public const string IN_PROGRESS = "IN_PROGRESS";

        /// <summary>
        /// 已完成。任务的所有操作步骤已执行完毕，系统已更新相关库存和库位状态。
        /// 例如上架任务完成后更新目标库位库存，拣货任务完成后扣减拣货位库存。
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// 失败。任务在执行过程中发生异常导致失败，如设备故障、库位异常、物料不匹配等。
        /// 失败的任务需排查原因后手动重试或取消。
        /// </summary>
        public const string FAILED = "FAILED";

        /// <summary>
        /// 已取消。任务被主动取消不再执行。取消时需回滚已预占的资源（如库位预留、库存锁定），
        /// 确保系统数据一致性。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 任务优先级，用于确定任务的执行顺序。当有多个待执行任务时，
    /// 调度系统按优先级从高到低排序，优先级高的任务优先被分配和执行。
    /// </summary>
    public static class TaskPriority
    {
        /// <summary>
        /// 低优先级。适用于常规的、不紧急的作业，如计划性移库、库存整理等。
        /// 当有更高优先级的任务时，低优先级任务会被延后执行。
        /// </summary>
        public const string LOW = "LOW";

        /// <summary>
        /// 普通优先级。大多数任务的默认优先级，如常规上架、标准拣货等。
        /// 按正常调度顺序执行。
        /// </summary>
        public const string NORMAL = "NORMAL";

        /// <summary>
        /// 高优先级。适用于需要优先处理的紧急业务，如紧急出库、客户加急订单等。
        /// 排在普通优先级任务之前执行。
        /// </summary>
        public const string HIGH = "HIGH";

        /// <summary>
        /// 特急。最高优先级，适用于极其紧急的场景，如生产线停线等待物料、
        /// 客户投诉紧急发货等。必须立即处理，排在所有任务之前。
        /// </summary>
        public const string URGENT = "URGENT";
    }

    /// <summary>
    /// 执行模式，用于标识任务是由自动化设备执行还是由人工执行。
    /// 不同执行模式对应不同的任务调度和确认逻辑。
    /// </summary>
    public static class ExecutionMode
    {
        /// <summary>
        /// 自动执行。任务由自动化设备（如堆垛机、AGV、输送线）执行，
        /// 系统将任务指令直接下发到设备控制系统（WCS），设备完成后自动反馈结果。
        /// </summary>
        public const string AUTO = "AUTO";

        /// <summary>
        /// 手动执行。任务由仓库操作人员通过 PDA 或 PC 终端手动执行，
        /// 人员到达指定库位完成操作后在终端上确认。
        /// </summary>
        public const string MANUAL = "MANUAL";
    }

    /// <summary>
    /// 容器状态，用于标识容器（托盘、料箱等）当前的使用情况。
    /// 容器是物料在仓库中流转的载体，状态管理确保容器被正确使用。
    /// </summary>
    public static class ContainerStatus
    {
        /// <summary>
        /// 空容器。容器内无任何物料，处于空闲状态，可以被分配用于新的入库组盘。
        /// 空容器通常存放在空容器存放区，需要时由系统调度搬运到收货区。
        /// </summary>
        public const string EMPTY = "EMPTY";

        /// <summary>
        /// 使用中。容器内已装入物料，正在仓库中流转或已放置在库位上。
        /// 使用中的容器不能被重新分配用于其他组盘操作。
        /// </summary>
        public const string IN_USE = "IN_USE";

        /// <summary>
        /// 锁定。容器被临时锁定，不允许进行任何操作。通常发生在盘点期间
        /// （正在盘点的容器需锁定防止盘点过程中物料变化）或质检期间
        /// （待检验的物料需锁定防止被出库）。
        /// </summary>
        public const string LOCKED = "LOCKED";

        /// <summary>
        /// 搬运中。容器正在被设备（AGV、堆垛机、输送线）搬运，从一个位置移动到另一个位置。
        /// 搬运过程中不能对容器内的物料进行操作，搬运完成后状态自动更新。
        /// </summary>
        public const string MOVING = "MOVING";
    }

    /// <summary>
    /// 入库单状态，描述入库单从创建到完成的完整生命周期。
    /// 状态流转：草稿 -> 收货中 -> 已收货 -> 已组盘 -> 已完成 / 已取消
    /// </summary>
    public static class InboundOrderStatus
    {
        /// <summary>
        /// 草稿。入库单刚创建尚未开始收货，可修改单据信息（物料、数量等）。
        /// 此状态不占用库存，不影响仓库作业。
        /// </summary>
        public const string DRAFT = "DRAFT";

        /// <summary>
        /// 收货中。已开始对入库单中的物料进行收货操作，部分物料已到货登记。
        /// 收货过程中逐行记录实收数量，可能与计划数量存在差异。
        /// </summary>
        public const string RECEIVING = "RECEIVING";

        /// <summary>
        /// 已收货。入库单中所有物料均已收货完成，实收数量已确认。
        /// 收货完成后可进入组盘环节，将物料与容器绑定。
        /// </summary>
        public const string RECEIVED = "RECEIVED";

        /// <summary>
        /// 已组盘。收货物料已全部完成组盘（绑定容器），生成了组盘记录。
        /// 组盘完成后系统自动生成上架任务，将物料从收货区搬运到存储库位。
        /// </summary>
        public const string BOUND = "BOUND";

        /// <summary>
        /// 已完成。入库单关联的所有上架任务已执行完毕，物料已放入指定库位，
        /// 系统库存已增加。入库单关闭，不可再修改。
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// 已取消。入库单被取消，尚未收货的物料不再接收。
        /// 已收货但未上架的物料需视情况处理（退货或特殊入库）。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 入库行状态，描述入库单中每一行物料的收货进度。
    /// 每行物料独立跟踪，允许部分行先完成收货。
    /// </summary>
    public static class InboundLineStatus
    {
        /// <summary>
        /// 待收货。该行物料尚未开始收货，等待物料到货后进行登记。
        /// </summary>
        public const string OPEN = "OPEN";

        /// <summary>
        /// 收货中。正在对该行物料进行收货操作，已部分收货但尚未达到计划数量。
        /// </summary>
        public const string RECEIVING = "RECEIVING";

        /// <summary>
        /// 已收货。该行物料的实收数量已确认（可能等于或小于计划数量），
        /// 可以进入后续的组盘环节。
        /// </summary>
        public const string RECEIVED = "RECEIVED";
    }

    /// <summary>
    /// 质量状态，用于标识物料的质检情况。
    /// 质检结果决定物料是否可以正常使用或需要特殊处理。
    /// </summary>
    public static class QualityStatus
    {
        /// <summary>
        /// 待检。物料已收货但尚未完成质检，处于等待质检的状态。
        /// 待检物料不可用于出库，需等待质检完成后根据结果更新状态。
        /// </summary>
        public const string PENDING = "PENDING";

        /// <summary>
        /// 合格。物料已通过质检确认合格，可以正常上架和使用。
        /// 合格物料的库存状态变为可用，参与正常的库存分配。
        /// </summary>
        public const string QUALIFIED = "QUALIFIED";

        /// <summary>
        /// 不合格。物料未通过质检，不可正常使用。
        /// 不合格物料通常需退回供应商或移至不良品区域，不参与正常出库分配。
        /// </summary>
        public const string UNQUALIFIED = "UNQUALIFIED";
    }

    /// <summary>
    /// 组盘状态，描述入库物料与容器绑定后的流转状态。
    /// 状态流转：已组盘 -> 上架中 -> 已上架 / 已取消
    /// </summary>
    public static class BindStatus
    {
        /// <summary>
        /// 已组盘。物料已绑定到容器上，组盘记录已创建，等待生成上架任务。
        /// 此状态下容器状态变为"使用中"。
        /// </summary>
        public const string BOUND = "BOUND";

        /// <summary>
        /// 上架中。已申请上架，任务已创建并分配了目标库位，
        /// 等待设备（堆垛机/AGV/输送线）将物料搬运到目标库位。
        /// 设备搬运完成确认后状态更新为已上架。
        /// </summary>
        public const string PUTTING_AWAY = "PUTTING_AWAY";

        /// <summary>
        /// 已上架。组盘对应的物料已通过上架任务放置到目标库位，
        /// 容器和物料已在库位上登记，系统库存已更新。
        /// </summary>
        public const string PUT_AWAY = "PUT_AWAY";

        /// <summary>
        /// 已取消。组盘操作被取消，容器与物料的绑定关系解除，
        /// 容器恢复为空容器状态，物料需重新组盘或退回。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 出库单状态，描述出库单从创建到完成的完整生命周期。
    /// 状态流转：草稿 -> 释放中 -> 已释放 -> 已完成 / 已取消
    /// </summary>
    public static class OutboundOrderStatus
    {
        /// <summary>
        /// 草稿。出库单刚创建尚未释放，可修改出库物料和数量。
        /// 此状态不锁定库存，不影响仓库现有库存。
        /// </summary>
        public const string DRAFT = "DRAFT";

        /// <summary>
        /// 已确认。出库单已通过审核确认，不可再修改或删除。
        /// 确认后可执行库存分配操作。
        /// </summary>
        public const string CONFIRMED = "CONFIRMED";

        /// <summary>
        /// 释放中。出库单正在进行库存分配（按策略计算从哪些库位拣货），
        /// 分配过程中系统会锁定对应库存，防止被其他出库单占用。
        /// </summary>
        public const string RELEASING = "RELEASING";

        /// <summary>
        /// 已释放。出库单已完成库存分配并生成了拣货任务（或波次），
        /// 等待仓库执行拣货操作。
        /// </summary>
        public const string RELEASED = "RELEASED";

        /// <summary>
        /// 已完成。出库单中所有物料已完成拣货并确认发货，
        /// 系统库存已扣减，出库单关闭不可修改。
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// 已取消。出库单被取消不再执行，需释放已锁定的库存。
        /// 已生成拣货任务的需先取消任务才能取消出库单。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 出库行状态，描述出库单中每一行物料的执行进度。
    /// 每行物料独立跟踪，从待分配到发货完成。
    /// </summary>
    public static class OutboundLineStatus
    {
        /// <summary>
        /// 待分配。该行物料尚未进行库存分配，等待出库单释放时按分配策略计算拣货来源。
        /// </summary>
        public const string OPEN = "OPEN";

        /// <summary>
        /// 已分配。系统已为该行物料分配了具体库位和批次的库存，
        /// 被分配的库存会被锁定，等待生成拣货任务。
        /// </summary>
        public const string ALLOCATED = "ALLOCATED";

        /// <summary>
        /// 已拣货。该行物料已完成从库位上取出的操作，物料在出库暂存区等待发货。
        /// </summary>
        public const string PICKED = "PICKED";

        /// <summary>
        /// 已发货。该行物料已确认出库发货，系统已扣减对应库存，
        /// 完成整个出库流程。
        /// </summary>
        public const string SHIPPED = "SHIPPED";
    }

    /// <summary>
    /// 波次状态，描述出库波次从创建到完成的完整生命周期。
    /// 波次是出库作业的调度单元，将多个出库单合并处理以提高拣货效率。
    /// 状态流转：草稿 -> 已释放 -> 已完成 / 已取消
    /// </summary>
    public static class WaveStatus
    {
        /// <summary>
        /// 草稿。波次已创建但尚未释放，可添加或移除出库单、调整波次参数。
        /// 此状态下不进行库存分配，不生成拣货任务。
        /// </summary>
        public const string DRAFT = "DRAFT";

        /// <summary>
        /// 已释放。波次已释放执行，系统完成了库存分配并生成了拣货任务，
        /// 仓库人员或设备开始按任务拣货。
        /// </summary>
        public const string RELEASED = "RELEASED";

        /// <summary>
        /// 已完成。波次下所有出库单的拣货任务均已执行完毕，
        /// 物料已完成拣货和发货确认。
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// 已取消。波次被取消，需释放所有已分配的库存并取消关联的拣货任务。
        /// 仅在波次下的任务尚未开始执行时才能取消。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 出库分配状态，描述出库库存分配单从创建到完成的完整生命周期。
    /// 分配单记录了出库单的每一行物料具体从哪个库位、哪个容器、哪个批次取出多少数量。
    /// 状态流转：已分配 -> 拣货中 -> 已拣货 -> 已发货 / 已取消
    /// </summary>
    public static class AllocationStatus
    {
        /// <summary>
        /// 已分配。系统已按出库策略（FIFO/FEFO等）为出库单分配了具体库位和批次的库存，
        /// 被分配的库存数量会被锁定（InvInventoryDetail.LockedQty 增加），
        /// 防止被其他出库单重复占用。此状态等待生成出库搬运任务。
        /// </summary>
        public const string ALLOCATED = "ALLOCATED";

        /// <summary>
        /// 拣货中。分配明细已生成出库搬运任务并下发给WCS，堆垛机正在将托盘从高位取出。
        /// 此状态下不可修改分配结果，不可取消分配。
        /// </summary>
        public const string PICKING = "PICKING";

        /// <summary>
        /// 已拣货。托盘已到达出库口，出库确认完成，物料已从库位上取出。
        /// 系统已扣减对应库存（减少 InvInventoryDetail.Qty 和 LockedQty），并记录库存流水（InvMovement）。
        /// </summary>
        public const string PICKED = "PICKED";

        /// <summary>
        /// 已取消。分配被取消，系统释放已锁定的库存（减少 LockedQty），
        /// 恢复库存可用状态。仅在拣货任务尚未开始执行时才能取消。
        /// </summary>
        public const string CANCELLED = "CANCELLED";
    }

    /// <summary>
    /// 库存状态，用于标识库存记录的可用性。
    /// 不同状态的库存参与不同的业务操作，确保库存使用的准确性和安全性。
    /// </summary>
    public static class InventoryStatus
    {
        /// <summary>
        /// 可用。库存处于正常可用状态，可以参与出库分配、移库、盘点等所有正常业务操作。
        /// 出库策略和补货策略在计算可用库存时仅统计此状态的库存。
        /// </summary>
        public const string AVAILABLE = "AVAILABLE";

        /// <summary>
        /// 锁定。库存被临时锁定，不可用于出库分配。通常发生在以下场景：
        /// 出库单已分配库存但尚未完成拣货时锁定对应数量；
        /// 盘点进行中锁定盘点范围内的库存。
        /// 锁定原因消除后自动或手动解锁恢复为可用状态。
        /// </summary>
        public const string LOCKED = "LOCKED";

        /// <summary>
        /// 质检中。物料正在进行质量检验，暂时不可使用。
        /// 入库收货后如需质检，库存先标记为质检状态，待质检合格后转为可用。
        /// 质检不合格的物料转为不合格品处理。
        /// </summary>
        public const string QC = "QC";

        /// <summary>
        /// 冻结。库存被冻结，所有业务操作均不可使用。通常由管理人员手动冻结，
        /// 原因包括：物料质量问题追溯、客户投诉调查、库存异常待核实等。
        /// 冻结库存需手动解冻后才能恢复正常使用。
        /// </summary>
        public const string FROZEN = "FROZEN";
    }

    /// <summary>
    /// 库存变动类型，用于 InvMovement 记录的 MovementType 字段。
    /// 每种库存数量或状态变化都必须记录对应的变动流水。
    /// </summary>
    public static class MovementTypes
    {
        /// <summary>
        /// 入库。上架完成时库存增加。
        /// </summary>
        public const string INBOUND = "INBOUND";

        /// <summary>
        /// 出库。拣货发货后库存减少。
        /// </summary>
        public const string OUTBOUND = "OUTBOUND";

        /// <summary>
        /// 移库。库位间物料转移。
        /// </summary>
        public const string TRANSFER = "TRANSFER";

        /// <summary>
        /// 调整。盘盈/盘亏/损耗/纠错等数量调整。
        /// </summary>
        public const string ADJUST = "ADJUST";

        /// <summary>
        /// 盘点。盘点操作时记录。
        /// </summary>
        public const string STOCKTAKE = "STOCKTAKE";

        /// <summary>
        /// 冻结。库存状态变更为冻结。
        /// </summary>
        public const string FREEZE = "FREEZE";

        /// <summary>
        /// 解冻。冻结库存恢复为可用。
        /// </summary>
        public const string UNFREEZE = "UNFREEZE";
    }

    /// <summary>
    /// 库存变动方向，用于 InvMovement 记录的 Direction 字段。
    /// </summary>
    public static class MovementDirections
    {
        /// <summary>
        /// 增加。库存数量增加。
        /// </summary>
        public const string INCREASE = "INCREASE";

        /// <summary>
        /// 减少。库存数量减少。
        /// </summary>
        public const string DECREASE = "DECREASE";

        /// <summary>
        /// 不变。移库等不引起数量增减的流水方向（仅在库位间转移，数量保持不变）。
        /// </summary>
        public const string UNCHANGED = "UNCHANGED";
    }

    /// <summary>
    /// 库存快照类型，用于 InvSnapshotHeader.SnapshotType 字段。
    /// </summary>
    public static class SnapshotTypes
    {
        /// <summary>手动快照。由用户在界面手动触发。</summary>
        public const string MANUAL = "MANUAL";

        /// <summary>每日定时快照。由后台服务 DailySnapshotBackgroundService 按日自动生成。</summary>
        public const string DAILY = "DAILY";

        /// <summary>盘点快照。盘点单创建时自动触发，作为盘点比对基准。</summary>
        public const string STOCKTAKE = "STOCKTAKE";
    }

    /// <summary>
    /// 布尔标志，用于 byte 类型的布尔字段，替代数据库中的 bit 类型。
    /// </summary>
    public static class BoolFlag
    {
        /// <summary>
        /// 是。表示启用、激活、允许等肯定含义。
        /// </summary>
        public const byte YES = 1;

        /// <summary>
        /// 否。表示禁用、停用、不允许等否定含义。
        /// </summary>
        public const byte NO = 0;
    }

    /// <summary>
    /// 单据操作，用于标识在当前单据状态下允许执行的操作。
    /// </summary>
    public static class DocActions
    {
        public const string EDIT = "EDIT";
        public const string DELETE = "DELETE";
        public const string CONFIRM = "CONFIRM";
        public const string ALLOCATE = "ALLOCATE";
        public const string RECEIVE = "RECEIVE";
        public const string BIND = "BIND";
    }

    /// <summary>
    /// 配置作用域级别，用于全局配置的层级覆盖机制。
    /// 级别从高到低：GLOBAL > DOC_TYPE > WAREHOUSE > ZONE，低级别配置覆盖高级别配置。
    /// </summary>
    public static class ConfigScopeLevels
    {
        /// <summary>
        /// 全局级别。适用于所有仓库、所有单据类型的通用配置。
        /// </summary>
        public const string GLOBAL = "GLOBAL";

        /// <summary>
        /// 仓库级别。针对特定仓库的配置，覆盖全局配置。
        /// </summary>
        public const string WAREHOUSE = "WAREHOUSE";

        /// <summary>
        /// 区域级别。针对仓库内特定区域的配置，覆盖仓库级配置。
        /// </summary>
        public const string ZONE = "ZONE";

        /// <summary>
        /// 单据类型级别。针对特定单据类型的配置，覆盖全局配置。
        /// </summary>
        public const string DOC_TYPE = "DOC_TYPE";
    }

    /// <summary>
    /// 任务确认来源，用于标识任务确认结果的触发方。
    /// </summary>
    public static class ConfirmSource
    {
        /// <summary>
        /// 来自WCS（仓库控制系统）。设备执行完成后自动反馈确认结果。
        /// </summary>
        public const string WCS = "WCS";
    }

    /// <summary>
    /// 冻结记录状态，描述库存冻结/解冻操作的执行结果。
    /// </summary>
    public static class FreezeRecordStatus
    {
        /// <summary>
        /// 已冻结。库存已被冻结，不可参与出库分配等业务操作。
        /// </summary>
        public const string FROZEN = "FROZEN";

        /// <summary>
        /// 已解冻。库存冻结已被解除，恢复正常可用状态。
        /// </summary>
        public const string UNFROZEN = "UNFROZEN";
    }

    /// <summary>
    /// 区域类型，用于标识仓库中不同功能区域的分类。
    /// 上架策略、拣货策略等会根据区域类型决定库位分配规则。
    /// </summary>
    public static class ZoneTypes
    {
        /// <summary>
        /// 存储区。用于长期存放物料的主要区域，通常为高位货架区。
        /// </summary>
        public const string STORAGE = "STORAGE";

        /// <summary>
        /// 收货区。用于接收入库物料、进行收货质检操作的暂存区域。
        /// </summary>
        public const string RECEIVING = "RECEIVING";

        /// <summary>
        /// 发货区。用于出库物料拣货后暂存、等待发货的区域。
        /// </summary>
        public const string SHIPPING = "SHIPPING";

        /// <summary>
        /// 拣货区。用于存放即将被拣出的物料，通常为低位货架或流利架。
        /// </summary>
        public const string PICKING = "PICKING";

        /// <summary>
        /// 备货区。用于特定场景下的物料备货存放区域。
        /// </summary>
        public const string RESERVING = "RESERVING";

        private static readonly HashSet<string> StorageZoneTypes = [STORAGE];

        /// <summary>
        /// 判断区域类型是否属于存储区域。
        /// </summary>
        public static bool IsStorageZone(string zoneType) => StorageZoneTypes.Contains(zoneType);
    }

    /// <summary>
    /// 接驳口类型（MdTransferPoint.PointType），与 cfg_transfer_point_type 配置类型同义。
    /// </summary>
    public static class TransferPointType
    {
        /// <summary>入库接驳口</summary>
        public const string INBOUND = "INBOUND";

        /// <summary>出库接驳口</summary>
        public const string OUTBOUND = "OUTBOUND";

        /// <summary>双向接驳口（入/出均可）</summary>
        public const string MIXED = "MIXED";
    }

    /// <summary>
    /// 系统参数编码，对应 sys_parameter 表中的 ParamCode。
    /// </summary>
    public static class ParamCodes
    {
        /// <summary>
        /// 默认密码。新增用户或重置密码时，未指定密码的情况下使用此参数值。
        /// </summary>
        public const string DEFAULT_PASSWORD = "SYS_DEFAULT_PASSWORD";

        /// <summary>
        /// 默认密码值。当 sys_parameter 中未配置 SYS_DEFAULT_PASSWORD 时的兜底密码。
        /// </summary>
        public const string DEFAULT_PASSWORD_VALUE = "123456";

        /// <summary>
        /// 是否允许多设备同时登录（1=允许 0=不允许）
        /// </summary>
        public const string ALLOW_MULTI_LOGIN = "SYS_ALLOW_MULTI_LOGIN";

        /// <summary>
        /// Token 过期时间（分钟）
        /// </summary>
        public const string TOKEN_EXPIRE_MIN = "SYS_TOKEN_EXPIRE_MIN";

        /// <summary>
        /// 库存锁定策略（1=锁定 0=不锁定）
        /// </summary>
        public const string LOCK_ON_INVENTORY = "WH_LOCK_ON_INVENTORY";

        /// <summary>
        /// 日志保留天数
        /// </summary>
        public const string LOG_RETAIN_DAYS = "LOG_RETAIN_DAYS";

        /// <summary>
        /// 所有系统内置参数定义（ParamCode → (ParamName, DefaultValue)）
        /// </summary>
        public static readonly Dictionary<string, (string Name, string Value)> RequiredParams = new()
        {
            { DEFAULT_PASSWORD, ("默认密码", DEFAULT_PASSWORD_VALUE) },
            { ALLOW_MULTI_LOGIN, ("允许多设备登录", "1") },
            { TOKEN_EXPIRE_MIN, ("Token过期时间(分钟)", "30") },
            { LOCK_ON_INVENTORY, ("库存锁定策略", "1") },
            { LOG_RETAIN_DAYS, ("日志保留天数", "90") },
        };
    }
}
