using DataRepository;
using PCS.DataRepository.DataContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace PCS.DataRepository.Students
{
    public static class StudentMenu
    {
        public static StudentMenuFull MenuGet()
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                List<StudentMenuItem> first = (from it in con.menus
                                               where it.level == 1
                                               select new StudentMenuItem
                                               {
                                                   href = it.href,
                                                   level = it.level.ToString(),
                                                   text = it.Text,
                                                   title = it.title
                                               }).ToList();
                List<StudentMenuItem> second = (from it in con.menus
                                                where it.level == 2
                                                select new StudentMenuItem
                                                {
                                                    href = it.href,
                                                    level = it.level.ToString(),
                                                    text = it.Text,
                                                    title = it.title
                                                }).ToList();
                StudentMenuFull menu = new StudentMenuFull();
                menu.first = first;
                menu.second = second;
                return menu;
            }
        }

        public static string BackgroundGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.background).FirstOrDefault();
                return data;
            }
        }

        public static int BackgroundSave(string background, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.background = background;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string ObservationsGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.observations).FirstOrDefault();
                return data;
            }
        }

        public static int ObservationsSave(string observations, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.observations = observations;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string TestGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.interpretations).FirstOrDefault();
                return data;
            }
        }

        public static int TestSave(string interpretations, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.interpretations = interpretations;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string GoalsGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.goals).FirstOrDefault();
                return data;
            }
        }

        public static int GoalsSave(string goals, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.goals = goals;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string ProgressGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.progress).FirstOrDefault();
                return data;
            }
        }

        public static int ProgressSave(string progress, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.progress = progress;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string RecomendationsGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.recommendations).FirstOrDefault();
                return data;
            }
        }

        public static int RecomendationsSave(string recommendations, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.recommendations = recommendations;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static string SummaryGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.summary).FirstOrDefault();
                return data;
            }
        }

        public static string StatusGet(string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it.status).FirstOrDefault();
                return data;
            }
        }

        public static int SummarySave(string summary, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.summary = summary;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        public static int StatusSave(string status, string studentId)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int id = int.Parse(studentId);

                var data = (from it in con.studentSupportReports
                            where it.studentId == id
                            select it).FirstOrDefault();
                data.status = status;
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }

        /*public static List<TimeRecordFrom> TimeRecordsGet(string user, string student)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int studentId = int.Parse(student);
                int userId = int.Parse(user);

                var data = (from it in con.userTimeRecords
                            where it.studentId == studentId && it.userId == userId
                            select it).ToList();
                List<TimeRecordFrom> result = new List<TimeRecordFrom>();
                TimeRecordFrom record;
                foreach (var item in data)
                {
                    record = new TimeRecordFrom();
                    record.code = item.code;
                    record.date = item.date.ToString("yyyy-MM-dd");
                    record.id = item.id.ToString();
                    record.notes = item.notes;
                    record.time = item.time.ToString();
                    result.Add(record);
                }
                return result;
            }
        }*/

        /*public static TimeRecordFrom TimeRecordOneGet(string id)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int recordId = int.Parse(id);

                var data = (from it in con.userTimeRecords
                            where it.id == recordId
                            select it).FirstOrDefault();
                TimeRecordFrom form = new TimeRecordFrom
                                          {
                                              code = data.code,
                                              date = data.date.ToString("yyyy-MM-dd"),
                                              id = data.id.ToString(),
                                              notes = data.notes,
                                              time = data.time.ToString()
                                          };
                return form;
            }
        }*/

        /*public static int TimeRecordsSave(TimeRecordFrom form, string user, string student)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int studentId = int.Parse(student);
                int userId = int.Parse(user);
                int id = int.Parse(form.id);
                int row;
                if (id == -1)
                {
                    userTimeRecord record = new userTimeRecord();
                    record.code = form.code;
                    record.date = DateTime.ParseExact(form.date, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                    record.notes = form.notes;
                    record.studentId = int.Parse(student);
                    record.time = int.Parse(form.time);
                    record.userId = int.Parse(user);
                    con.userTimeRecords.InsertOnSubmit(record);
                    row = 1;
                }
                else
                {
                    var record = (from item in con.userTimeRecords
                                  where item.id == id
                                  select item).FirstOrDefault();
                    record.code = form.code;
                    record.date = DateTime.ParseExact(form.date, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                    record.notes = form.notes;
                    record.time = int.Parse(form.time);
                    row = con.GetChangeSet().Updates.Count();
                }
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }*/

        /*public static int TimeRecordsDelete(string id)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int recordId = int.Parse(id);
                var record = (from item in con.userTimeRecords
                              where item.id == recordId
                              select item).FirstOrDefault();
                con.userTimeRecords.DeleteOnSubmit(record);
                int row = con.GetChangeSet().Updates.Count();
                con.studentSupportReports.Context.SubmitChanges();
                return row;
            }
        }*/

        /*public static List<StudentSearchForm> StudentSearchGet(string user)
        {
            using (DataClassesPCSDataContext con = new DataClassesPCSDataContext())
            {
                int userId = int.Parse(user);
                var data = (from item in con.students
                            join req in con.studentSupportRequests on item.id equals req.studentId
                            join assign in con.studentSupportRequestAssignments on req.id equals assign.studentSupportRequestId
                            join sch in con.schools on item.schoolId equals sch.id
                            where assign.specialistId == userId
                            select new StudentSearchForm
                            {
                                id = item.id.ToString(),
                                grade = item.grade,
                                name = (item.lastName + ", " + item.firstName),
                                number = item.studentNumber,
                                school = sch.schoolName,
                                district = sch.organization.districtName,
                                dob = item.dob.ToShortDateString()
                            }).ToList();
                return data;
            }
        }*/

        /*public static List<StudentSearchForm> StudentSearchGetParams(string filters, string user)
        {
            List<StudentSearchForm> students = StudentSearchGet(user);
            filters = filters.ToLower();
            string[] filterSplitted = filters.Split('|');
            string field;
            string val;
            string[] buf;
            int numFilter;
            if (filterSplitted.Count() == 1 && int.TryParse(filters, out numFilter))
            {
                students = students.Where(p => p.grade == filters
                    || p.number == filters
                    || p.dob.Contains(filters)).ToList();
            }
            else
            {
                if (filterSplitted.Count() == 1 && filters.Split('=').Count() == 1)
                {
                    students = students.Where(p => p.district.ToLower().Contains(filters)
                        || p.name.ToLower().Contains(filters)
                        || p.school.ToLower().Contains(filters)).ToList();
                }
                else
                {
                    foreach (string filter in filterSplitted)
                    {
                        buf = filter.Split('=');
                        if (buf.Count() != 2)
                        {
                            return new List<StudentSearchForm>();
                        }
                        else
                        {
                            field = buf[0];
                            val = buf[1];
                            switch (field)
                            {
                                case "name":
                                    students = students.Where(p => p.name.ToLower().Contains(val)).ToList();
                                    break;
                                case "dob":
                                    students = students.Where(p => p.dob.ToLower().Contains(val)).ToList();
                                    break;
                                case "number":
                                    students = students.Where(p => p.number.ToLower() == val).ToList();
                                    break;
                                case "grade":
                                    students = students.Where(p => p.grade.ToLower() == val).ToList();
                                    break;
                                case "district":
                                    students = students.Where(p => p.district.ToLower().Contains(val)).ToList();
                                    break;
                                case "school":
                                    students = students.Where(p => p.school.ToLower().Contains(val)).ToList();
                                    break;
                                default:
                                    return new List<StudentSearchForm>();
                                    break;
                            }
                        }
                    }
                }
            }
            return students;
        }*/
    }
}