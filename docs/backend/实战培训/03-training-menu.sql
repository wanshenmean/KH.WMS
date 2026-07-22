SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
  BEGIN TRANSACTION;
  DECLARE @ParentId bigint;
  SELECT @ParentId=Id FROM dbo.sys_permission WHERE PermissionCode=N'training';
  IF @ParentId IS NULL
  BEGIN
    INSERT dbo.sys_permission(PermissionCode,PermissionName,ParentId,MenuType,Path,Icon,SortNo,IsVisible,Status,IsExternal,IsCache,Remark,CreatedTime)
    VALUES(N'training',N'实战培训',0,0,N'/training',N'edit',90,1,1,0,0,N'独立培训功能',SYSDATETIME());
    SET @ParentId=SCOPE_IDENTITY();
  END;

  IF NOT EXISTS(SELECT 1 FROM dbo.sys_permission WHERE PermissionCode=N'training_carrier')
    INSERT dbo.sys_permission(PermissionCode,PermissionName,ParentId,MenuType,Path,Component,Icon,SortNo,IsVisible,Status,Buttons,IsExternal,IsCache,CreatedTime)
    VALUES(N'training_carrier',N'实战一：承运商',@ParentId,1,N'/training/carrier',N'training/carrier',N'connection',1,1,1,
      N'[{"buttonCode":"btn_add","buttonName":"新增","permKey":"training:carrier:add","sortNo":1,"status":1},{"buttonCode":"btn_edit","buttonName":"编辑","permKey":"training:carrier:edit","sortNo":2,"status":1},{"buttonCode":"btn_delete","buttonName":"删除","permKey":"training:carrier:delete","sortNo":3,"status":1},{"buttonCode":"btn_view","buttonName":"查看","permKey":"training:carrier:view","sortNo":4,"status":1},{"buttonCode":"btn_import","buttonName":"导入","permKey":"training:carrier:import","sortNo":5,"status":1},{"buttonCode":"btn_export","buttonName":"导出","permKey":"training:carrier:export","sortNo":6,"status":1},{"buttonCode":"btn_toggle","buttonName":"启停","permKey":"training:carrier:toggle","sortNo":7,"status":1}]',0,0,SYSDATETIME());

  IF NOT EXISTS(SELECT 1 FROM dbo.sys_permission WHERE PermissionCode=N'training_owner')
    INSERT dbo.sys_permission(PermissionCode,PermissionName,ParentId,MenuType,Path,Component,Icon,SortNo,IsVisible,Status,Buttons,IsExternal,IsCache,CreatedTime)
    VALUES(N'training_owner',N'实战二：货主 ExtData',@ParentId,1,N'/training/owner-profile',N'training/owner-profile',N'peoples',2,1,1,
      N'[{"buttonCode":"btn_add","buttonName":"新增","permKey":"training:owner:add","sortNo":1,"status":1},{"buttonCode":"btn_edit","buttonName":"编辑","permKey":"training:owner:edit","sortNo":2,"status":1},{"buttonCode":"btn_delete","buttonName":"删除","permKey":"training:owner:delete","sortNo":3,"status":1},{"buttonCode":"btn_view","buttonName":"查看","permKey":"training:owner:view","sortNo":4,"status":1},{"buttonCode":"btn_export","buttonName":"导出","permKey":"training:owner:export","sortNo":5,"status":1}]',0,0,SYSDATETIME());

  IF NOT EXISTS(SELECT 1 FROM dbo.sys_permission WHERE PermissionCode=N'training_appointment')
    INSERT dbo.sys_permission(PermissionCode,PermissionName,ParentId,MenuType,Path,Component,Icon,SortNo,IsVisible,Status,Buttons,IsExternal,IsCache,CreatedTime)
    VALUES(N'training_appointment',N'实战三：到货预约',@ParentId,1,N'/training/arrival-appointment',N'training/arrival-appointment',N'schedule',3,1,1,
      N'[{"buttonCode":"btn_add","buttonName":"新增","permKey":"training:appointment:add","sortNo":1,"status":1},{"buttonCode":"btn_edit","buttonName":"编辑","permKey":"training:appointment:edit","sortNo":2,"status":1},{"buttonCode":"btn_delete","buttonName":"删除","permKey":"training:appointment:delete","sortNo":3,"status":1},{"buttonCode":"btn_view","buttonName":"查看","permKey":"training:appointment:view","sortNo":4,"status":1}]',0,0,SYSDATETIME());
  COMMIT;
END TRY
BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK;
  THROW;
END CATCH;

SELECT PermissionCode,PermissionName,Path,Component FROM dbo.sys_permission
WHERE PermissionCode IN(N'training',N'training_carrier',N'training_owner',N'training_appointment') ORDER BY Id;
