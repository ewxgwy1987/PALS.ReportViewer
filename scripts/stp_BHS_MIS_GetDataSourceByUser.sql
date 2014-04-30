GO
USE [ReportServer1];
GO


ALTER PROCEDURE dbo.stp_BHS_MIS_GetDataSourceByUser
		  @UserName varchar(100)
AS
BEGIN

	SELECT	DISTINCT rc.PolicyID,U.UserType,U.AuthType,U.UserName,RC.Name,RC.Path
	FROM	Catalog RC,PolicyUserRole pur, Users U, Roles r
	WHERE	rc.PolicyID=pur.PolicyID
		AND pur.UserID=u.UserID
		AND pur.RoleID=r.RoleID
		AND r.RoleName='Content Manager'
		AND U.UserName=@USERNAME
		AND RC.Type=5
	ORDER BY RC.Name

END
