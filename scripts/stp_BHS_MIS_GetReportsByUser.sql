GO
USE [ReportServer1];
GO


ALTER PROCEDURE dbo.stp_BHS_MIS_GetReportsByUser
		  @UserName varchar(100),
		  @Folder varchar(50)
AS
BEGIN

	IF UPPER(@Folder) = 'ALL'
	BEGIN
		SELECT	DISTINCT rc.PolicyID,U.UserType,U.AuthType,U.UserName,RC.Name,RC.Path
		FROM	Catalog RC,PolicyUserRole pur, Users U
		WHERE	rc.PolicyID=pur.PolicyID
			AND pur.UserID=u.UserID
			AND U.UserName=@USERNAME
			AND RC.Type=2
		ORDER BY RC.Name
	END
	ELSE
	BEGIN
		SELECT	DISTINCT rc.PolicyID,U.UserType,U.AuthType,U.UserName,RC.Name,RC.Path
		FROM	Catalog RC,PolicyUserRole pur, Users U
		WHERE	rc.PolicyID=pur.PolicyID
			AND pur.UserID=u.UserID
			AND U.UserName=@USERNAME
			AND RC.Type=2
			AND RC.Path LIKE '/' + @Folder + '/%'
		ORDER BY RC.Name
	END
	

END
