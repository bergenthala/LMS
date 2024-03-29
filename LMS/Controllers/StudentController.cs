﻿using System;
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
            var assignmentsQuery = from a in db.Assignments
                                   join ac in db.AssignmentCategories on a.AcId equals ac.AcId into rightSide
                                   from j1 in rightSide.DefaultIfEmpty()
                                   join c in db.Classes on j1.ClassId equals c.ClassId into join2
                                   from j2 in join2.DefaultIfEmpty()
                                   join co in db.Courses on j2.CId equals co.CId into join3
                                   from j3 in join3.DefaultIfEmpty()
                                   where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season
                                   && j2.SemesterYear == year
                                   select new {
                                       aname = a.Name,
                                       cname = j1.Name,
                                       due = a.Due,
                                       score = (from s in db.Submissions
                                                where s.Student == uid && s.AId == a.AId
                                                select s.Score).SingleOrDefault()
                                   };

            return Json(assignmentsQuery.ToArray());
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
                           join ac in db.AssignmentCategories on a.AcId equals ac.AcId into rightSide
                           from j1 in rightSide.DefaultIfEmpty()
                           join c in db.Classes on j1.ClassId equals c.ClassId into join2
                           from j2 in join2.DefaultIfEmpty()
                           join co in db.Courses on j2.CId equals co.CId into join3
                           from j3 in join3.DefaultIfEmpty()
                           where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season
                           && j2.SemesterYear == year && a.Name == asgname && j1.Name == category
                           select a.AId;

            var resubmissionQuery = from s in db.Submissions
                                    where s.AId == aIDQuery.SingleOrDefault() && s.Student == uid
                                    select s;

            Submission newSubmission = new Submission();
            newSubmission.Student = uid;
            newSubmission.Contents = contents;
            newSubmission.AId = aIDQuery.SingleOrDefault();

            if (resubmissionQuery.SingleOrDefault() != null) {
                newSubmission.Score = resubmissionQuery.Single().Score;
                newSubmission.Time = resubmissionQuery.Single().Time;

                db.Submissions.Remove(resubmissionQuery.Single());
            } else {
                newSubmission.Score = 0;
                newSubmission.Time = DateTime.Now;
            }

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
            Dictionary<string, double> gradeToValue = new Dictionary<string, double>
            {
                {"A", 4.0},
                {"A-", 3.7},
                {"B+", 3.3},
                {"B", 3.0},
                {"B-", 2.7},
                {"C+", 2.3},
                {"C", 2.0},
                {"C-", 1.7},
                {"D+", 1.3},
                {"D", 1.0},
                {"E", 0.0}
            };
            var query = from co in db.Courses
                        join c in db.Classes on co.CId equals c.CId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join e in db.Enrolleds on j1.ClassId equals e.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        where j2.Student == uid && j2.Grade != "--"
                        select new
                        {
                            grade = j2.Grade
                        };

            if (query.Any())
            {
                var grades = query.ToList();
                double avgGrade = grades.Average(x => gradeToValue[x.grade]);
                return Json(new { gpa = avgGrade });
            }
            return Json(new { gpa = 0.0 });
        }


        /*******End code to modify********/

    }
}

