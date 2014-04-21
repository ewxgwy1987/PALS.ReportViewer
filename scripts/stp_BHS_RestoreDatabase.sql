GO
USE [BHSDB_CLT];
GO

alter PROCEDURE dbo.stp_BHS_RestoreDatabase
		  @DATABASE_NAME VARCHAR(50),
		  @BACKUP_FILEPATH VARCHAR(1000),
		  @MDF_FILEPATH VARCHAR(1000),
		  @LDF_FILEPATH VARCHAR(1000),
		  @STP_RESULT VARCHAR(50) OUT
AS
BEGIN

	SET @STP_RESULT='';

	DECLARE @RESULT INT;

	--1. Check the database restored exists or not
	IF EXISTS(SELECT name FROM master.sys.sysdatabases WHERE name=@DATABASE_NAME)
	BEGIN
		SET @STP_RESULT = 'The specified database name has already existed.'
		RETURN
	END

	--2. CHECK BACKUP FILE EXISTS
	EXEC master.sys.xp_fileexist @BACKUP_FILEPATH,@RESULT out;
	IF @RESULT=0
	BEGIN
		SET @STP_RESULT = 'Backup file does not exist.';
		RETURN
	END

	--Check @MDF_FILEPATH and @LDF_FILEPATH
	DECLARE @FILE_EXISTS BIT;
	DECLARE @IS_DIR BIT;
	DECLARE @PARENT_EXISTS BIT;
	CREATE TABLE #PATHEXISTS
	(
		FILE_EXISTS BIT,
		IS_DIRECTORY BIT,
		PARENT_DIRECTORY_EXISTS BIT
	)

	--3. CHECK PARENT DIRECTORY OF MDF FILE EXISTS
	DELETE FROM #PATHEXISTS;
	INSERT INTO #PATHEXISTS
	EXEC master.sys.xp_fileexist @MDF_FILEPATH
	
	SELECT @FILE_EXISTS=FILE_EXISTS, @IS_DIR=IS_DIRECTORY, @PARENT_EXISTS=PARENT_DIRECTORY_EXISTS FROM #PATHEXISTS;
	IF @PARENT_EXISTS=0
	BEGIN
		SET @STP_RESULT = 'The path of MDF file can not be found.';
		RETURN
	END
	ELSE IF @FILE_EXISTS = 1
	BEGIN
		SET @STP_RESULT = 'There is already a MDF file with same name in the specified location.';
		RETURN
	END

	--4. CHECK PARENT DIRECTORY OF LDF FILE EXISTS
	DELETE FROM #PATHEXISTS;
	INSERT INTO #PATHEXISTS
	EXEC master.sys.xp_fileexist @LDF_FILEPATH

	SELECT @FILE_EXISTS=FILE_EXISTS, @IS_DIR=IS_DIRECTORY, @PARENT_EXISTS=PARENT_DIRECTORY_EXISTS FROM #PATHEXISTS;
	IF @PARENT_EXISTS=0
	BEGIN
		SET @STP_RESULT = 'The path of LDF file can not be found.';
		RETURN
	END
	ELSE IF @FILE_EXISTS = 1
	BEGIN
		SET @STP_RESULT = 'There is already a LDF file with same name in the specified location.';
		RETURN
	END

	BEGIN TRY
		--5. Find logical files of data and log
		DECLARE @LOGICAL_FILE_DATA VARCHAR(100);
		DECLARE @LOGICAL_FILE_LOG VARCHAR(100);

		DECLARE @FileListTable table
		(
			LogicalName          nvarchar(128),
			PhysicalName         nvarchar(260),
			[Type]               char(1),
			FileGroupName        nvarchar(128),
			Size                 numeric(20,0),
			MaxSize              numeric(20,0),
			FileID               bigint,
			CreateLSN            numeric(25,0),
			DropLSN              numeric(25,0),
			UniqueID             uniqueidentifier,
			ReadOnlyLSN          numeric(25,0),
			ReadWriteLSN         numeric(25,0),
			BackupSizeInBytes    bigint,
			SourceBlockSize      int,
			FileGroupID          int,
			LogGroupGUID         uniqueidentifier,
			DifferentialBaseLSN  numeric(25,0),
			DifferentialBaseGUID uniqueidentifier,
			IsReadOnl            bit,
			IsPresent            bit,
			TDEThumbprint        varbinary(32) -- remove this column if using SQL 2005
		)

		INSERT INTO @FileListTable
		EXEC('RESTORE FILELISTONLY FROM DISK=''' + @BACKUP_FILEPATH + '''' )
		SELECT @LOGICAL_FILE_DATA=LogicalName FROM @FileListTable WHERE [Type]='D';
		SELECT @LOGICAL_FILE_LOG=LogicalName FROM @FileListTable WHERE [Type]='L';

		--6. Restore database to the specified path
		RESTORE DATABASE @DATABASE_NAME
		FROM DISK = @BACKUP_FILEPATH
			WITH RECOVERY,
			MOVE @LOGICAL_FILE_DATA TO @MDF_FILEPATH, 
			MOVE @LOGICAL_FILE_LOG TO @LDF_FILEPATH
	END TRY
	BEGIN CATCH
		SELECT @STP_RESULT=ERROR_MESSAGE();
	END CATCH;

	DROP TABLE #PATHEXISTS;

END

