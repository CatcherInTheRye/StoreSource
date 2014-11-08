-- create login id ,record login info
CREATE PROCEDURE [dbo].[serviceLogin]
    @user_name VARCHAR(40),
    @user_ip VARCHAR(20)
AS
BEGIN
	DECLARE @user_id INT
	DECLARE @login_id BIGINT
	SELECT @login_id = ISNULL(MAX(id), 0) + 1
	FROM [login] tablockx

	SELECT @user_id = id
	FROM users
	WHERE [userName] = @user_name
		OR email = @user_name

	INSERT INTO [login] (id
						 , [userid]
						 , ipaddress
						 , logintime
						 , loginactivetime
						 , service)
	VALUES (@login_id
		  , @user_id
		  , @user_ip
		  , GETDATE()
		  , GETDATE()
		  , 1)

	SELECT @login_id
END
