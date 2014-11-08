CREATE FUNCTION [Plsd].[GetAttendance]
(
    @dos_stu_id INT,
	@startDate smalldatetime,
	@endDate smalldatetime,
	@type varchar(20)
)
RETURNS decimal(10,1)
AS
BEGIN
	--declare @result int
	DECLARE @result decimal(10,1)
	
	select @result = count(AttendanceDate)
			from Plsd.SyncAttendances att inner join 
			dos_students_sync dss on dss.sis_id = att.StudentId inner join 
			dos_students ds on ds.dos_stu_id = dss.dos_stu_id
			where AttendanceDate between @startDate and @endDate and  AttendanceType like @type 
			and ds.dos_stu_id = @dos_stu_id

	RETURN @result/(case when @type='Absent' then 2 else 1 end)
END