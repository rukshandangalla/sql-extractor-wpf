DECLARE @VersionNo NVARCHAR(100) = '##RELEASE_NUMBER##',
@ScriptNo NVARCHAR(100) = '##SCRIPT_NUMBER## Escript'

IF NOT EXISTS(SELECT 1 FROM dbo.VersionScripts vs WHERE vs.Version = @VersionNo AND vs.ScriptName = @ScriptNo)
BEGIN
	INSERT INTO[dbo].[VersionScripts]
		([ScriptName], [Version], [Author], [ExecuteUser], [ExecuteDateTime], [Description])
	VALUES
		(@ScriptNo, @VersionNo, '##AUTHOR_NAME##', SUSER_SNAME(), GETDATE(), '##STORY_NAME##')
END
GO
DROP PROCEDURE IF EXISTS ##SP_NAME##  
GO

------------------------------------------------------------------------------------------------------------------------
