using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        join c in db.Classes on e.ClassId equals c.ClassId into rightSide
                        from j1 in rightSide.DefaultIfEmpty()
                        join co in db.Courses on j1.CId equals co.CId into nextJoin
                        from j2 in nextJoin.DefaultIfEmpty()
                        where e.Student == uid
                        select new
                        {
                            subject = j2.DeptId,
                            number = j2.Number,
                            name = j2.Name,
                            season = j1.SemesterSeason,
                            year = j1.SemesterYear,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from s in db.Submissions
                        join a in db.Assignments on s.AId equals a.AId into aSide
                        from assignJoin in aSide.DefaultIfEmpty()
                        join ac in db.AssignmentCategories on assignJoin.AcId equals ac.AcId into rightSide
                        from j1 in rightSide.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season
                        && j2.SemesterYear == year && s.Student == uid
                        select new
                        {
                            aname = assignJoin.Name,
                            cname = j1.Name,
                            due = assignJoin.Due,
                            score = assignJoin == null ? null : (uint?)assignJoin.Points,
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var aIDQuery = from a in db.Assignments
                           join aca in db.AssignmentCategories on a.AcId equals aca.AcId into assignCats
                           from ac in assignCats.DefaultIfEmpty()
                           join cl in db.Classes on ac.ClassId equals cl.ClassId into rightSide
                           from c in rightSide.DefaultIfEmpty()
                           join cou in db.Courses on c.ClassId equals cou.CId into join1
                           from co in join1.DefaultIfEmpty()
                           where co.DeptId == subject && co.Number == num && c.SemesterSeason == season && c.SemesterYear == year
                           && ac.Name == category && a.Name == asgname
                           select a.AId;

            //Is true if there is no assignment id found using the method parameters.
            if (aIDQuery.SingleOrDefault() == 0) {
                System.Diagnostics.Debug.WriteLine("There was a bad query");
                return Json(new { success = false });
            }

            var resubmissionQuery = from a in db.Assignments
                                    where a.AId == aIDQuery.SingleOrDefault()
                                    select a;

            Submission newSubmission = new Submission();
            newSubmission.Time = DateTime.Now;
            newSubmission.Contents = contents;

            if (resubmissionQuery.DefaultIfEmpty() == null) {
                db.Submissions.Update(newSubmission);
                db.SaveChanges();
                return Json(new { success = true });
            }

            newSubmission.Student = uid;
            newSubmission.Score = 0;
            newSubmission.AId = aIDQuery.SingleOrDefault();

            db.Submissions.Add(newSubmission);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from co in db.Courses
                        join c in db.Classes on co.CId equals c.CId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join e in db.Enrolleds on j1.ClassId equals e.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join s in db.Students on j2.Student equals s.UId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where co.DeptId == subject && co.Number == num && j1.SemesterSeason == season && j1.SemesterYear == year && j2.Student == uid
                        select j3;

            if (query.SingleOrDefault() != null)
            {
                return Json(new { success = false });
            }

            Enrolled enrolled = new Enrolled();
            enrolled.Student = uid;
            var classID = from co in db.Courses
                          join c in db.Classes on co.CId equals c.CId into join1
                          from j1 in join1.DefaultIfEmpty()
                          where co.DeptId == subject && co.Number == num && j1.SemesterSeason == season && j1.SemesterYear == year
                          select j1.ClassId;
            enrolled.ClassId = classID.SingleOrDefault();
            enrolled.Grade = "--";

            db.Enrolleds.Add(enrolled);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from co in db.Courses
                        join c in db.Classes on co.CId equals c.CId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join e in db.Enrolleds on j1.ClassId equals e.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        where j2.Student == uid
                        select new
                        {
                            gpa = j2.Grade
                        };
            switch (query.ToString())
            {
                case "A":
                    return Json(4.0);
            }
            return Json(query);
        }
                
        /*******End code to modify********/

        }
    }

