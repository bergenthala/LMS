using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from s in db.Students
                        join e in db.Enrolleds on s.UId equals e.Student into join1
                        from j1 in join1.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        join d in db.Departments on j3.DeptId equals d.Subject into join4
                        from j4 in join4.DefaultIfEmpty()
                        where j4.Subject == subject && j3.Number == num && j2.SemesterSeason == season && j2.SemesterYear == year
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = j1.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var query = from a in db.Assignments
                        join ac in db.AssignmentCategories on a.AcId equals ac.AcId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season && j2.SemesterYear == year && category == null ? true : j1.Name == category
                        select new
                        {
                            aname = a.Name,
                            cname = j1.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from ac in db.AssignmentCategories
                        join c in db.Classes on ac.ClassId equals c.ClassId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join co in db.Courses on j1.CId equals co.CId into join2
                        from j2 in join2.DefaultIfEmpty()
                        where j2.DeptId == subject && j2.Number == num && j1.SemesterSeason == season && j1.SemesterYear == year
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from ac in db.AssignmentCategories
                        join c in db.Classes on ac.ClassId equals c.ClassId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join co in db.Courses on j1.CId equals co.CId into join2
                        from j2 in join2.DefaultIfEmpty()
                        where j2.DeptId == subject && j2.Number == num && j1.SemesterSeason == season && j1.SemesterYear == year && ac.Name == category
                        select j1;

            if(query.SingleOrDefault() != null)
            {
                return Json(new { success = false });
            }

            var classIDQuery = from c in db.Classes
                           join co in db.Courses on c.CId equals co.CId into join1
                           from j1 in join1.DefaultIfEmpty()
                           where j1.Number == num && c.SemesterSeason == season && c.SemesterYear == year && j1.DeptId == subject
                           select c.ClassId;

            AssignmentCategory assignmentCategory = new AssignmentCategory();
            assignmentCategory.ClassId = classIDQuery.SingleOrDefault();
            assignmentCategory.Weight = (byte)catweight;
            assignmentCategory.Name = category;

            db.AssignmentCategories.Add(assignmentCategory);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from a in db.Assignments
                        join ac in db.AssignmentCategories on a.AcId equals ac.AcId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season && j2.SemesterYear == year && j1.Name == category && a.Name == asgname
                        select a;

            if(query.SingleOrDefault() != null)
            {
                return Json(new { success = false });
            }

            var acid = from asc in db.AssignmentCategories
                       join c in db.Classes on asc.ClassId equals c.ClassId into join1
                       from j1 in join1.DefaultIfEmpty()
                       join co in db.Courses on j1.CId equals co.CId into join2
                       from j2 in join2.DefaultIfEmpty()
                       where asc.Name == category && j1.SemesterSeason == season && j1.SemesterYear == year && j2.Number == num && j2.DeptId == subject
                       select asc.AcId;

            Assignment assignment = new Assignment();
            assignment.AcId = acid.SingleOrDefault();
            assignment.Points = (uint)asgpoints;
            assignment.Name = asgname;
            assignment.Due = asgdue;
            assignment.Contents = asgcontents;

            db.Assignments.Add(assignment);
            db.SaveChanges();


            //Updates the grades of all students in the class
            var studentsInClassQuery = from co in db.Courses
                                       join cl in db.Classes on co.CId equals cl.CId into join1
                                       from c in join1.DefaultIfEmpty()
                                       join en in db.Enrolleds on c.ClassId equals en.ClassId into join2
                                       from e in join2.DefaultIfEmpty()
                                       where co.Number == num && co.DeptId == subject && c.SemesterSeason == season && c.SemesterYear == year
                                       select e;

            var classID = from c in db.Classes
                          join co in db.Courses on c.CId equals co.CId into join1
                          from j1 in join1.DefaultIfEmpty()
                          where j1.Number == num && c.SemesterSeason == season && c.SemesterYear == year && j1.DeptId == subject
                          select c;

            foreach (var student in studentsInClassQuery.ToArray()) {
                UpdateStudentGrade(student.Student, classID.Single().ClassId);
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from s in db.Submissions
                        join a in db.Assignments on s.AId equals a.AId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join ac in db.AssignmentCategories on j1.AcId equals ac.AcId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join c in db.Classes on j2.ClassId equals c.ClassId into join3
                        from j3 in join3.DefaultIfEmpty()
                        join co in db.Courses on j3.CId equals co.CId into join4
                        from j4 in join4.DefaultIfEmpty()
                        join st in db.Students on s.Student equals st.UId into join6
                        from j6 in join6.DefaultIfEmpty()
                        where j3.SemesterSeason == season && j3.SemesterYear == year && j4.DeptId == subject && j2.Name == category && j1.Name == asgname && j4.Number == num
                        select new
                        {
                            fname = j6.FName,
                            lname = j6.LName,
                            uid = j6.UId,
                            time = s.Time,
                            score = s.Score
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from s in db.Submissions
                        join a in db.Assignments on s.AId equals a.AId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join ac in db.AssignmentCategories on j1.AcId equals ac.AcId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join c in db.Classes on j2.ClassId equals c.ClassId into join3
                        from j3 in join3.DefaultIfEmpty()
                        join co in db.Courses on j3.CId equals co.CId into join4
                        from j4 in join4.DefaultIfEmpty()
                        join st in db.Students on s.Student equals st.UId into join6
                        from j6 in join6.DefaultIfEmpty()
                        where j3.SemesterSeason == season && j3.SemesterYear == year && j4.DeptId == subject && j2.Name == category && j1.Name == asgname 
                        && j4.Number == num && s.Student == uid
                        select s;

            //Checks if there was an assignment found to be graded
            if (query.SingleOrDefault() != null)
            {
                Submission submission = query.Single();
                submission.Score = (uint)score;
                
                db.Submissions.Update(submission);
                db.SaveChanges();

                var classID = from c in db.Classes
                              join co in db.Courses on c.CId equals co.CId into join1
                              from j1 in join1.DefaultIfEmpty()
                              where j1.Number == num && c.SemesterSeason == season && c.SemesterYear == year && j1.DeptId == subject
                              select c;

                UpdateStudentGrade(uid, classID.Single().ClassId);

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classQuery = from c in db.Classes
                        join co in db.Courses on c.CId equals co.CId into rightSide
                        from j1 in rightSide.DefaultIfEmpty()
                        where c.Teacher == uid
                        select new {
                            subject = j1.DeptId,
                            number = j1.Number,
                            name = j1.Name,
                            season = c.SemesterSeason,
                            year = c.SemesterYear
                        };

            return Json(classQuery.ToArray());
        }

        private void UpdateStudentGrade(string uid, uint classID)
        {
            var asgcat = from ac in db.AssignmentCategories
                         where ac.ClassId == classID
                         select ac;


            uint totalWeight = 0;
            uint totalScore = 0;
            foreach (var category in asgcat.ToArray()) {
                uint totalPoints = 0;
                uint maxPoints = 0;

                var sQuery = from s in db.Submissions
                             join a in db.Assignments on s.AId equals a.AId into join1
                             from j1 in join1.DefaultIfEmpty()
                             join ac in db.AssignmentCategories on j1.AcId equals ac.AcId into join2
                             from j2 in join2.DefaultIfEmpty()
                             join c in db.Classes on j2.ClassId equals c.ClassId into join3
                             from j3 in join3.DefaultIfEmpty()
                             join enr in db.Enrolleds on j3.ClassId equals enr.ClassId into join4
                             from j4 in join4.DefaultIfEmpty()
                             where j4.Student == uid && j4.ClassId == classID && j2.AcId == category.AcId
                             select s;

               

                //Calculate totalPoints and MaxPoints
                foreach (var submission in sQuery.ToArray())
                {
                    totalPoints += submission.Score;
                }

                var asgs = from a in db.Assignments
                           join ac in db.AssignmentCategories on a.AcId equals ac.AcId into join1
                           from j1 in join1.DefaultIfEmpty()
                           where j1.ClassId == classID && j1.AcId == category.AcId
                           select a;

                foreach (var asg in asgs.ToArray())
                {
                    maxPoints += asg.Points;
                }
                if(maxPoints == 0)
                {
                    continue;
                }

                uint newGrade = totalPoints / maxPoints;

                // Calculate category weight
                newGrade *= category.Weight;

                totalScore += newGrade;
                totalWeight += category.Weight;
            }

            // Calculate scaling factor
            uint scalingFactor = 100 / totalWeight;

            // Calculate the percentage the student earned in the class
            totalScore *= scalingFactor;

            Dictionary<double, string> valueToGrade = new Dictionary<double, string>
            {
                {4.0, "A"},
                {3.7, "A-"},
                {3.3, "B+"},
                {3.0, "B"},
                {2.7, "B-"},
                {2.3, "C+"},
                {2.0, "C"},
                {1.7, "C-"},
                {1.3, "D+"},
                {1.0, "D"},
                {0.0, "E"}
            };

            string finalGrade = "";
            foreach(KeyValuePair<double, string> grade in valueToGrade)
            {
                if(totalScore >= grade.Key)
                {
                    finalGrade = grade.Value;
                    break;
                }
            }

            var enrolledClass = from en in db.Enrolleds
                                where en.ClassId == classID && en.Student == uid
                                select en;
            Enrolled e = enrolledClass.Single();
            e.Grade = finalGrade;

            db.Enrolleds.Update(e);
            db.SaveChanges();

        }

        /*******End code to modify********/
    }
}

