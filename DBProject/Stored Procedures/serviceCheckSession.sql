
CREATE PROCEDURE [dbo].[serviceCheckSession]
    @login_id BIGINT
AS
BEGIN
	DECLARE @active SMALLDATETIME

	IF NOT EXISTS (SELECT id
				   FROM [login]
				   WHERE id = @login_id)
	BEGIN
		SELECT -1

	END
	ELSE
	BEGIN
		SELECT DATEDIFF(n, loginactivetime, CONVERT(SMALLDATETIME, GETDATE()))
		FROM [login]
		WHERE id = @login_id

		UPDATE [login]
		SET loginactivetime = GETDATE()
		WHERE id = @login_id

	END
END
