/*
    KH.WMS 实战培训表（SQL Server）

    用途：为通用 CRUD、ExtData 和主从表实战准备隔离数据。
    特性：
      1. 可重复执行；已存在的表、索引和样例数据不会重复创建。
      2. 不创建数据库，请在目标培训库中执行。
      3. 不创建物理外键；主从保存与删除由 DetailSaveService 管理。
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.trn_carrier', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.trn_carrier
        (
            Id                 bigint IDENTITY(1,1) NOT NULL,
            CarrierCode        nvarchar(30) NOT NULL,
            CarrierName        nvarchar(100) NOT NULL,
            ContactName        nvarchar(50) NULL,
            ContactPhone       nvarchar(20) NULL,
            TransportMode      nvarchar(20) NULL,
            Status             tinyint NOT NULL CONSTRAINT DF_trn_carrier_Status DEFAULT (1),
            Remark             nvarchar(500) NULL,
            CreatedBy          nvarchar(50) NULL,
            CreatedByName      nvarchar(50) NULL,
            CreatedTime        datetime2(3) NOT NULL CONSTRAINT DF_trn_carrier_CreatedTime DEFAULT (SYSDATETIME()),
            LastModifiedBy     nvarchar(50) NULL,
            LastModifiedByName nvarchar(50) NULL,
            LastModifiedTime   datetime2(3) NULL,
            CONSTRAINT PK_trn_carrier PRIMARY KEY CLUSTERED (Id)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_carrier')
          AND name = N'uk_trn_carrier_code'
    )
        CREATE UNIQUE INDEX uk_trn_carrier_code ON dbo.trn_carrier(CarrierCode);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_carrier')
          AND name = N'idx_trn_carrier_status'
    )
        CREATE INDEX idx_trn_carrier_status ON dbo.trn_carrier(Status);

    IF OBJECT_ID(N'dbo.trn_owner_profile', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.trn_owner_profile
        (
            Id                 bigint IDENTITY(1,1) NOT NULL,
            OwnerCode          nvarchar(30) NOT NULL,
            OwnerName          nvarchar(100) NOT NULL,
            ContactName        nvarchar(50) NULL,
            ContactPhone       nvarchar(20) NULL,
            Address            nvarchar(500) NULL,
            Remark             nvarchar(500) NULL,
            ExtData            nvarchar(max) NULL,
            CreatedBy          nvarchar(50) NULL,
            CreatedByName      nvarchar(50) NULL,
            CreatedTime        datetime2(3) NOT NULL CONSTRAINT DF_trn_owner_profile_CreatedTime DEFAULT (SYSDATETIME()),
            LastModifiedBy     nvarchar(50) NULL,
            LastModifiedByName nvarchar(50) NULL,
            LastModifiedTime   datetime2(3) NULL,
            CONSTRAINT PK_trn_owner_profile PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT CK_trn_owner_profile_ExtData_Json
                CHECK (ExtData IS NULL OR ISJSON(ExtData) = 1)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_owner_profile')
          AND name = N'uk_trn_owner_profile_code'
    )
        CREATE UNIQUE INDEX uk_trn_owner_profile_code ON dbo.trn_owner_profile(OwnerCode);

    IF OBJECT_ID(N'dbo.trn_arrival_appointment', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.trn_arrival_appointment
        (
            Id                 bigint IDENTITY(1,1) NOT NULL,
            AppointmentNo      nvarchar(50) NOT NULL,
            CarrierId          bigint NULL,
            OwnerId            bigint NULL,
            WarehouseId        bigint NULL,
            AppointmentDate    date NOT NULL,
            AppointmentTimeSlot nvarchar(20) NULL,
            VehicleNo          nvarchar(30) NULL,
            DriverName         nvarchar(50) NULL,
            DriverPhone        nvarchar(20) NULL,
            Remark             nvarchar(500) NULL,
            CreatedBy          nvarchar(50) NULL,
            CreatedByName      nvarchar(50) NULL,
            CreatedTime        datetime2(3) NOT NULL CONSTRAINT DF_trn_arrival_appointment_CreatedTime DEFAULT (SYSDATETIME()),
            LastModifiedBy     nvarchar(50) NULL,
            LastModifiedByName nvarchar(50) NULL,
            LastModifiedTime   datetime2(3) NULL,
            CONSTRAINT PK_trn_arrival_appointment PRIMARY KEY CLUSTERED (Id)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment')
          AND name = N'uk_trn_arrival_appointment_no'
    )
        CREATE UNIQUE INDEX uk_trn_arrival_appointment_no
            ON dbo.trn_arrival_appointment(AppointmentNo);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment')
          AND name = N'idx_trn_arrival_appointment_date'
    )
        CREATE INDEX idx_trn_arrival_appointment_date
            ON dbo.trn_arrival_appointment(AppointmentDate);

    IF OBJECT_ID(N'dbo.trn_arrival_appointment_line', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.trn_arrival_appointment_line
        (
            Id                 bigint IDENTITY(1,1) NOT NULL,
            AppointmentId      bigint NOT NULL,
            [LineNo]           int NOT NULL,
            MaterialId         bigint NULL,
            MaterialCode       nvarchar(50) NOT NULL,
            MaterialName       nvarchar(200) NOT NULL,
            ExpectedQty        decimal(12,3) NOT NULL,
            UnitId             bigint NULL,
            BatchNo            nvarchar(50) NULL,
            Remark             nvarchar(500) NULL,
            CreatedBy          nvarchar(50) NULL,
            CreatedByName      nvarchar(50) NULL,
            CreatedTime        datetime2(3) NOT NULL CONSTRAINT DF_trn_arrival_appointment_line_CreatedTime DEFAULT (SYSDATETIME()),
            LastModifiedBy     nvarchar(50) NULL,
            LastModifiedByName nvarchar(50) NULL,
            LastModifiedTime   datetime2(3) NULL,
            CONSTRAINT PK_trn_arrival_appointment_line PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT CK_trn_arrival_appointment_line_LineNo CHECK ([LineNo] > 0),
            CONSTRAINT CK_trn_arrival_appointment_line_ExpectedQty CHECK (ExpectedQty > 0)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment_line')
          AND name = N'idx_trn_arrival_appointment_line_header'
    )
        CREATE INDEX idx_trn_arrival_appointment_line_header
            ON dbo.trn_arrival_appointment_line(AppointmentId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment_line')
          AND name = N'uk_trn_arrival_appointment_line_no'
    )
        CREATE UNIQUE INDEX uk_trn_arrival_appointment_line_no
            ON dbo.trn_arrival_appointment_line(AppointmentId, [LineNo]);

    /* 普通单表样例：同时覆盖启用和禁用状态 */
    INSERT INTO dbo.trn_carrier
        (CarrierCode, CarrierName, ContactName, ContactPhone, TransportMode, Status, Remark, CreatedBy, CreatedByName)
    SELECT v.CarrierCode, v.CarrierName, v.ContactName, v.ContactPhone,
           v.TransportMode, v.Status, v.Remark, N'training', N'培训数据'
    FROM
    (
        VALUES
            (N'TRN-CAR-001', N'远海物流', N'张帆', N'13800001001', N'ROAD', CAST(1 AS tinyint), N'公路运输样例'),
            (N'TRN-CAR-002', N'冷链速运', N'李雪', N'13800001002', N'COLD_CHAIN', CAST(1 AS tinyint), N'冷链运输样例'),
            (N'TRN-CAR-003', N'联运达',   N'王陆', N'13800001003', N'MULTIMODAL', CAST(0 AS tinyint), N'用于练习启用操作')
    ) v(CarrierCode, CarrierName, ContactName, ContactPhone, TransportMode, Status, Remark)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.trn_carrier t WHERE t.CarrierCode = v.CarrierCode
    );

    /* ExtData 单表样例：键名与 02-training-owner-form-config.json 一致 */
    INSERT INTO dbo.trn_owner_profile
        (OwnerCode, OwnerName, ContactName, ContactPhone, Address, Remark, ExtData, CreatedBy, CreatedByName)
    SELECT v.OwnerCode, v.OwnerName, v.ContactName, v.ContactPhone, v.Address,
           v.Remark, v.ExtData, N'training', N'培训数据'
    FROM
    (
        VALUES
            (N'TRN-OWN-001', N'鲜食一号', N'陈晨', N'13900002001', N'上海市浦东新区', N'完整扩展字段样例',
             N'{"customerLevel":"A","creditLimit":500000.00,"requiresColdChain":true,"contractExpiry":"2027-12-31"}'),
            (N'TRN-OWN-002', N'智造科技', N'赵磊', N'13900002002', N'苏州市工业园区', N'非冷链样例',
             N'{"customerLevel":"B","creditLimit":200000.00,"requiresColdChain":false,"contractExpiry":"2027-06-30"}'),
            (N'TRN-OWN-003', N'训练货主', N'周宁', NULL, NULL, N'扩展字段为空的对照数据', NULL)
    ) v(OwnerCode, OwnerName, ContactName, ContactPhone, Address, Remark, ExtData)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.trn_owner_profile t WHERE t.OwnerCode = v.OwnerCode
    );

    /* 主表样例：通过培训编码查找刚插入的承运商和货主 ID */
    INSERT INTO dbo.trn_arrival_appointment
        (AppointmentNo, CarrierId, OwnerId, WarehouseId, AppointmentDate,
         AppointmentTimeSlot, VehicleNo, DriverName, DriverPhone, Remark, CreatedBy, CreatedByName)
    SELECT v.AppointmentNo,
           c.Id,
           o.Id,
           NULL,
           v.AppointmentDate,
           v.AppointmentTimeSlot,
           v.VehicleNo,
           v.DriverName,
           v.DriverPhone,
           v.Remark,
           N'training',
           N'培训数据'
    FROM
    (
        VALUES
            (N'TRN-APT-20260801-001', N'TRN-CAR-001', N'TRN-OWN-001', CAST('2026-08-01' AS date), N'08:00-10:00', N'沪A·TRN01', N'孙海', N'13700003001', N'一主多从样例'),
            (N'TRN-APT-20260801-002', N'TRN-CAR-002', N'TRN-OWN-001', CAST('2026-08-01' AS date), N'13:00-15:00', N'苏E·TRN02', N'吴峰', N'13700003002', N'包含空批次的样例'),
            (N'TRN-APT-20260802-001', N'TRN-CAR-001', N'TRN-OWN-002', CAST('2026-08-02' AS date), N'10:00-12:00', N'沪B·TRN03', N'郑江', N'13700003003', N'单条明细样例')
    ) v(AppointmentNo, CarrierCode, OwnerCode, AppointmentDate, AppointmentTimeSlot,
        VehicleNo, DriverName, DriverPhone, Remark)
    INNER JOIN dbo.trn_carrier c ON c.CarrierCode = v.CarrierCode
    INNER JOIN dbo.trn_owner_profile o ON o.OwnerCode = v.OwnerCode
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.trn_arrival_appointment t
        WHERE t.AppointmentNo = v.AppointmentNo
    );

    /* 从表样例：MaterialId/UnitId 留空，不依赖正式业务数据 */
    INSERT INTO dbo.trn_arrival_appointment_line
        (AppointmentId, [LineNo], MaterialId, MaterialCode, MaterialName,
         ExpectedQty, UnitId, BatchNo, Remark, CreatedBy, CreatedByName)
    SELECT a.Id, v.[LineNo], NULL, v.MaterialCode, v.MaterialName,
           v.ExpectedQty, NULL, v.BatchNo, v.Remark, N'training', N'培训数据'
    FROM
    (
        VALUES
            (N'TRN-APT-20260801-001', 1, N'TRN-MAT-001', N'冷藏奶制品', CAST(120.000 AS decimal(12,3)), N'BATCH-COLD-01', N'有批次'),
            (N'TRN-APT-20260801-001', 2, N'TRN-MAT-002', N'常温包装盒', CAST(300.000 AS decimal(12,3)), NULL, N'空批次'),
            (N'TRN-APT-20260801-002', 1, N'TRN-MAT-003', N'速冻原料',   CAST(80.500 AS decimal(12,3)),  N'BATCH-FRZ-01', NULL),
            (N'TRN-APT-20260801-002', 2, N'TRN-MAT-004', N'保温箱',       CAST(40.000 AS decimal(12,3)),  NULL, NULL),
            (N'TRN-APT-20260802-001', 1, N'TRN-MAT-005', N'设备备件',     CAST(12.000 AS decimal(12,3)),  N'BATCH-SP-01', N'单明细数据')
    ) v(AppointmentNo, [LineNo], MaterialCode, MaterialName, ExpectedQty, BatchNo, Remark)
    INNER JOIN dbo.trn_arrival_appointment a ON a.AppointmentNo = v.AppointmentNo
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.trn_arrival_appointment_line t
        WHERE t.AppointmentId = a.Id AND t.[LineNo] = v.[LineNo]
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

/* 执行后自检：预期 4 张表，3/3/3/5 条培训数据 */
SELECT N'trn_carrier' AS TableName, COUNT_BIG(1) AS [RowCount] FROM dbo.trn_carrier
UNION ALL
SELECT N'trn_owner_profile', COUNT_BIG(1) FROM dbo.trn_owner_profile
UNION ALL
SELECT N'trn_arrival_appointment', COUNT_BIG(1) FROM dbo.trn_arrival_appointment
UNION ALL
SELECT N'trn_arrival_appointment_line', COUNT_BIG(1) FROM dbo.trn_arrival_appointment_line;
