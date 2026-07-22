/*
    实战二：货主档案动态扩展字段（SQLite 配置库）

    执行目标：KH.WMS.Server/bin/Debug/net8.0/Data/kh-wms-config.db
    可重复执行，不会重复创建实体类型或字段。
*/

PRAGMA foreign_keys = ON;
BEGIN IMMEDIATE;

INSERT INTO cfg_ext_field_type
(
    Id, EntityCode, EntityName, EntityCategory, HasLineLevel, IsActive,
    SortOrder, Remark, CreatedBy, CreatedByName, CreatedTime
)
SELECT
    (SELECT COALESCE(MAX(Id), 0) + 1 FROM cfg_ext_field_type),
    'TRN_OWNER_PROFILE',
    '培训货主档案',
    'BASEDATA',
    0,
    1,
    99,
    '实战二 ExtData 动态字段',
    'training',
    '培训数据',
    strftime('%Y-%m-%d %H:%M:%f', 'now')
WHERE NOT EXISTS
(
    SELECT 1
    FROM cfg_ext_field_type
    WHERE EntityCode = 'TRN_OWNER_PROFILE'
);

UPDATE cfg_ext_field_type
SET EntityName = '培训货主档案',
    EntityCategory = 'BASEDATA',
    HasLineLevel = 0,
    IsActive = 1,
    SortOrder = 99,
    Remark = '实战二 ExtData 动态字段'
WHERE EntityCode = 'TRN_OWNER_PROFILE';

INSERT INTO cfg_ext_field
(
    Id, EntityTypeId, FieldKey, FieldName, FieldType,
    IsProcessable, IsRequired, DefaultValue, FieldLevel,
    SortOrder, Remark, CreatedBy, CreatedByName, CreatedTime
)
SELECT
    (SELECT COALESCE(MAX(Id), 0) + 1 FROM cfg_ext_field),
    (SELECT Id FROM cfg_ext_field_type WHERE EntityCode = 'TRN_OWNER_PROFILE'),
    'customerLevel',
    '客户等级',
    'STRING',
    0,
    1,
    'NORMAL',
    'HEADER',
    1,
    '实战二扩展字段',
    'training',
    '培训数据',
    strftime('%Y-%m-%d %H:%M:%f', 'now')
WHERE NOT EXISTS
(
    SELECT 1
    FROM cfg_ext_field f
    JOIN cfg_ext_field_type t ON t.Id = f.EntityTypeId
    WHERE t.EntityCode = 'TRN_OWNER_PROFILE'
      AND f.FieldKey = 'customerLevel'
      AND f.FieldLevel = 'HEADER'
);

INSERT INTO cfg_ext_field
(
    Id, EntityTypeId, FieldKey, FieldName, FieldType,
    IsProcessable, IsRequired, DefaultValue, FieldLevel,
    SortOrder, Remark, CreatedBy, CreatedByName, CreatedTime
)
SELECT
    (SELECT COALESCE(MAX(Id), 0) + 1 FROM cfg_ext_field),
    (SELECT Id FROM cfg_ext_field_type WHERE EntityCode = 'TRN_OWNER_PROFILE'),
    'creditLimit',
    '信用额度',
    'DECIMAL',
    0,
    0,
    NULL,
    'HEADER',
    2,
    '实战二扩展字段',
    'training',
    '培训数据',
    strftime('%Y-%m-%d %H:%M:%f', 'now')
WHERE NOT EXISTS
(
    SELECT 1
    FROM cfg_ext_field f
    JOIN cfg_ext_field_type t ON t.Id = f.EntityTypeId
    WHERE t.EntityCode = 'TRN_OWNER_PROFILE'
      AND f.FieldKey = 'creditLimit'
      AND f.FieldLevel = 'HEADER'
);

INSERT INTO cfg_ext_field
(
    Id, EntityTypeId, FieldKey, FieldName, FieldType,
    IsProcessable, IsRequired, DefaultValue, FieldLevel,
    SortOrder, Remark, CreatedBy, CreatedByName, CreatedTime
)
SELECT
    (SELECT COALESCE(MAX(Id), 0) + 1 FROM cfg_ext_field),
    (SELECT Id FROM cfg_ext_field_type WHERE EntityCode = 'TRN_OWNER_PROFILE'),
    'requiresColdChain',
    '是否要求冷链',
    'BOOLEAN',
    0,
    0,
    NULL,
    'HEADER',
    3,
    '实战二扩展字段',
    'training',
    '培训数据',
    strftime('%Y-%m-%d %H:%M:%f', 'now')
WHERE NOT EXISTS
(
    SELECT 1
    FROM cfg_ext_field f
    JOIN cfg_ext_field_type t ON t.Id = f.EntityTypeId
    WHERE t.EntityCode = 'TRN_OWNER_PROFILE'
      AND f.FieldKey = 'requiresColdChain'
      AND f.FieldLevel = 'HEADER'
);

INSERT INTO cfg_ext_field
(
    Id, EntityTypeId, FieldKey, FieldName, FieldType,
    IsProcessable, IsRequired, DefaultValue, FieldLevel,
    SortOrder, Remark, CreatedBy, CreatedByName, CreatedTime
)
SELECT
    (SELECT COALESCE(MAX(Id), 0) + 1 FROM cfg_ext_field),
    (SELECT Id FROM cfg_ext_field_type WHERE EntityCode = 'TRN_OWNER_PROFILE'),
    'contractExpiry',
    '合同到期日',
    'DATETIME',
    0,
    0,
    NULL,
    'HEADER',
    4,
    '实战二扩展字段',
    'training',
    '培训数据',
    strftime('%Y-%m-%d %H:%M:%f', 'now')
WHERE NOT EXISTS
(
    SELECT 1
    FROM cfg_ext_field f
    JOIN cfg_ext_field_type t ON t.Id = f.EntityTypeId
    WHERE t.EntityCode = 'TRN_OWNER_PROFILE'
      AND f.FieldKey = 'contractExpiry'
      AND f.FieldLevel = 'HEADER'
);

UPDATE cfg_ext_field
SET FieldName = CASE FieldKey
        WHEN 'customerLevel' THEN '客户等级'
        WHEN 'creditLimit' THEN '信用额度'
        WHEN 'requiresColdChain' THEN '是否要求冷链'
        WHEN 'contractExpiry' THEN '合同到期日'
    END,
    FieldType = CASE FieldKey
        WHEN 'customerLevel' THEN 'STRING'
        WHEN 'creditLimit' THEN 'DECIMAL'
        WHEN 'requiresColdChain' THEN 'BOOLEAN'
        WHEN 'contractExpiry' THEN 'DATETIME'
    END,
    IsProcessable = 0,
    IsRequired = CASE WHEN FieldKey = 'customerLevel' THEN 1 ELSE 0 END,
    DefaultValue = CASE WHEN FieldKey = 'customerLevel' THEN 'NORMAL' ELSE NULL END,
    SortOrder = CASE FieldKey
        WHEN 'customerLevel' THEN 1
        WHEN 'creditLimit' THEN 2
        WHEN 'requiresColdChain' THEN 3
        WHEN 'contractExpiry' THEN 4
    END,
    Remark = '实战二扩展字段'
WHERE EntityTypeId =
      (SELECT Id FROM cfg_ext_field_type WHERE EntityCode = 'TRN_OWNER_PROFILE')
  AND FieldLevel = 'HEADER'
  AND FieldKey IN
      ('customerLevel', 'creditLimit', 'requiresColdChain', 'contractExpiry');

COMMIT;

SELECT
    t.EntityCode,
    f.FieldKey,
    f.FieldName,
    f.FieldType,
    f.IsRequired,
    f.DefaultValue,
    f.FieldLevel,
    f.SortOrder
FROM cfg_ext_field f
JOIN cfg_ext_field_type t ON t.Id = f.EntityTypeId
WHERE t.EntityCode = 'TRN_OWNER_PROFILE'
ORDER BY f.SortOrder;
