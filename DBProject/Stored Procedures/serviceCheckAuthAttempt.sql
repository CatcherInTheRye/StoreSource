
CREATE PROCEDURE [dbo].[serviceCheckAuthAttempt]
    @user_id INT,
	@attempt_timeout INT
AS
BEGIN
	IF NOT EXISTS (SELECT dos_user_id FROM dos_auth_attempts WHERE dos_user_id = @user_id)
	BEGIN
		INSERT INTO dos_auth_attempts (dos_user_id, last_access) VALUES (@user_id, SYSDATETIME());
		RETURN CONVERT(BIT, 1);
	END
	DECLARE @last_access DATETIME = (SELECT last_access FROM dos_auth_attempts WHERE dos_user_id = @user_id);
	IF DATEADD(ms, @attempt_timeout, @last_access) > SYSDATETIME()
		RETURN CONVERT(BIT, 0);
	UPDATE dos_auth_attempts SET last_access = SYSDATETIME() WHERE dos_user_id = @user_id;
	RETURN CONVERT(BIT, 1);
END
