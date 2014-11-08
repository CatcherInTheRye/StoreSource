-- 0 : successfully ; 1: bad password ; 2: bad user or wrong email; 3: user login is disabled
CREATE PROCEDURE [dbo].[serviceValidateUser]
    @logon_id VARCHAR(40), -- can be user name or email
    @password VARCHAR(40), --  hashed text
	@attempt_timeout INT
AS
BEGIN	
	DECLARE @check_attempt BIT;
	DECLARE @user_id INT;
	--@UserName is either user name or email address
	SET NOCOUNT ON
	IF EXISTS (SELECT id
			   FROM users
			   WHERE ([userName] = @logon_id
				   OR email = @logon_id)
				   AND pwd = @password
				   AND active = 1)
	BEGIN
		SELECT @user_id = id
			   FROM users
			   WHERE ([userName] = @logon_id OR email = @logon_id) AND active = 1
		EXEC @check_attempt = serviceCheckAuthAttempt @user_id, @attempt_timeout;
		IF @check_attempt = 0 
			SELECT 4 AS "result"
		ELSE
			SELECT 0 AS "result" -- successfully
	END
	ELSE 
	IF EXISTS (SELECT id
			   FROM users
			   WHERE ([userName] = @logon_id
				   OR email = @logon_id)
				   AND active = 1)
	BEGIN
		SELECT @user_id = id
			   FROM users
			   WHERE ([userName] = @logon_id OR email = @logon_id) AND active = 1
		EXEC @check_attempt = serviceCheckAuthAttempt @user_id, @attempt_timeout;
		IF @check_attempt = 0 
			SELECT 4 AS "result";
		ELSE
			SELECT 1 AS "result" -- return bad password too
	END
	ELSE
	IF EXISTS (SELECT id
			   FROM users
			   WHERE ([userName] = @logon_id
				   OR email = @logon_id)
				   AND active = 0)
		SELECT 3 AS "result" -- user login is disabled
	ELSE
	BEGIN		
		EXEC @check_attempt = serviceCheckAuthAttempt -1, @attempt_timeout;
		IF @check_attempt = 0 
			SELECT 4 AS "result";
		ELSE
			SELECT 1 AS "result" -- return bad password too
	END
END
