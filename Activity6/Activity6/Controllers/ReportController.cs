﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using Activity6.Models;
using System.Web.Http.Cors;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using System.Web.Hosting;
using System.Net.Http.Headers;
using Activity6.Report;
using System.Data;
using System.Dynamic;

namespace Activity6.Controllers
{
    [EnableCors(origins:"*", headers:"*",methods:"*")]
    public class ReportController : ApiController
    {
        [System.Web.Mvc.Route("api/Report/getReportData")]
        [HttpGet]
        public dynamic getReportData(int courseSelection)
        {
            SchoolEntities db = new SchoolEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<StudentGrade> grades;
          
            if (courseSelection == 1)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnsiteCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();
            }
            else if (courseSelection == 2)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnlineCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();
            }
            else
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).ToList();
            }
            return getExpandoReport(grades);
        }
        private dynamic getExpandoReport(List<StudentGrade> grades)
        {
            dynamic outObject = new ExpandoObject();
            var depList = grades.GroupBy(gg => gg.Course.Department.Name);
            List<dynamic> deps = new List<dynamic>();
            foreach (var group in depList)
            {
                dynamic department = new ExpandoObject();
                department.Name = group.Key;
                department.Average = group.Average(gg => gg.Grade);
                deps.Add(department);
            }
            outObject.Departments = deps;
            var courseList = grades.GroupBy(gg => gg.Course.Title);
            List<dynamic> courseGroups = new List<dynamic>();
            foreach (var group in courseList)
            {
                dynamic course = new ExpandoObject();
                course.Title = group.Key;
                course.Average = group.Average(gg => gg.Grade);
                List<dynamic> flexiGrades = new List<dynamic>();
                foreach (var item in group)
                {
                    dynamic gradeObj = new ExpandoObject();
                    gradeObj.Student = item.Person.FirstName + " " + item.Person.LastName;
                    gradeObj.Course = item.Course.Title;
                    gradeObj.Grade = item.Grade;
                    flexiGrades.Add(gradeObj);
                }
                course.StudentGrades = flexiGrades;
                courseGroups.Add(course);
            }
            outObject.Courses = courseGroups;
            return outObject;
            }

        [System.Web.Mvc.Route("api/Report/downloadReport")]
        [HttpGet]
        public HttpResponseMessage downloadReport(int courseSelection, int type)
        {
            HttpResponseMessage httpresponsemessage = new HttpResponseMessage();
            SchoolEntities db = new SchoolEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<StudentGrade> grades;
            if (courseSelection == 1)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnsiteCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();
            }
            else if (courseSelection == 2)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnlineCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();
            }
            else
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).ToList();
            }
            return getGradeReportFile(grades,type);
        }
        private HttpResponseMessage getGradeReportFile(List<StudentGrade> grades, int FileType)
        {
            ReportDocument report = new ReportDocument();
            report.Load(Path.Combine(HostingEnvironment.MapPath("~/Report/GradesReport.rpt")));
            GradesReportModel modle = new GradesReportModel();
            foreach (StudentGrade grade in grades)
            {
                DataRow row = modle.Grades.NewRow();
                row["Student"] = grade.Person.FirstName + " " + grade.Person.LastName;
                row["Grade"] = grade.Grade;
                row["Course"] = grade.Course.Title;
                row["Department"] = grade.Course.Department.Name;
                modle.Grades.Rows.Add(row);
            }

            report.SetDataSource(modle);

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            if (FileType == 1)
            {
                Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.Add("x-filename", "Report.pdf");
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = "GradesReport.pdf";
            }
            else
            {
                Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
                stream.Seek(0, SeekOrigin.Begin);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.Add("x-filename", "Report.doc");
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/msword");
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = "GradesReport.doc";
            }

            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            return httpResponseMessage;
        }


        }
    }

