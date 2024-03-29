﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.Name,
                            subject = d.Subject,
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from d in db.Departments
                        select new {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = from co in d.Courses
                            select new {
                                number = co.Number,
                                cname = co.Name
                            }
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from cl in db.Classes
                        join p in db.Professors on cl.Teacher equals p.UId into join1
                        from j1 in join1.DefaultIfEmpty()
                        join co in db.Courses on cl.CId equals co.CId into join2
                        from j2 in join2.DefaultIfEmpty()
                        where j2.DeptId == subject && j2.Number == number
                        select new {
                            season = cl.SemesterSeason,
                            year = cl.SemesterYear,
                            location = cl.Loc,
                            start = cl.Start, 
                            end = cl.End,
                            fname = j1.FName,
                            lname = j1.LName
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from a in db.Assignments
                        join ac in db.AssignmentCategories on a.AcId equals ac.AcId into rightSide
                        from j1 in rightSide.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season && j2.SemesterYear == year
                        && j1.Name == category && a.Name == asgname
                        select a.Contents;

            return Content(query.SingleOrDefault()!);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = from s in db.Submissions
                        join a in db.Assignments on s.AId equals a.AId
                        join ac in db.AssignmentCategories on a.AcId equals ac.AcId into rightSide
                        from j1 in rightSide.DefaultIfEmpty()
                        join c in db.Classes on j1.ClassId equals c.ClassId into join2
                        from j2 in join2.DefaultIfEmpty()
                        join co in db.Courses on j2.CId equals co.CId into join3
                        from j3 in join3.DefaultIfEmpty()
                        where j3.DeptId == subject && j3.Number == num && j2.SemesterSeason == season && j2.SemesterYear == year
                        && j1.Name == category && a.Name == asgname && s.Student == uid
                        select s.Contents;

            if(query.Any())
            {
                return Content(query.Single());
            }

            return Content("");
        }

        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var adminQuery = from a in db.Administrators
                             where a.UId == uid
                             select new {
                                 fname = a.FName,
                                 lname = a.LName,
                                 uid = a.UId
                             };
            if (adminQuery.SingleOrDefault() != null) return Json(adminQuery.Single());

            var professorQuery = from p in db.Professors
                             where p.UId == uid
                             select new {
                                 fname = p.FName,
                                 lname = p.LName,
                                 uid = p.UId,
                                 department = p.WorksIn
                             };
            if (professorQuery.SingleOrDefault() != null) return Json(professorQuery.Single());

            var studentQuery = from s in db.Students
                                 where s.UId == uid
                                 select new {
                                     fname = s.FName,
                                     lname = s.LName,
                                     uid = s.UId,
                                     department = s.Major
                                 };
            if (studentQuery.SingleOrDefault() != null) return Json(studentQuery.Single());

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

