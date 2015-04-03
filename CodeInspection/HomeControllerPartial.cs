using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using On_Call_Assistant.DAL;
using On_Call_Assistant.Models;


namespace On_Call_Assistant.Controllers
{
    public partial class HomeController : Controller
    {
        private OnCallContext db = new OnCallContext();
        public ActionResult RotationData(string start, string end, int ID = -1)
        {
            if (start == null || end == null)
            {
                start = end = DateTime.Today.ToString("u");
            }
            List<CalendarObject> rotationList = getRotations(start, end);
            if (ID != -1)
            {
                rotationList = rotationList.Where(rot => rot.id == ID).ToList();
            }
            return Json(rotationList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RotationsWithoutURL(string start, string end, int ID = -1)
        {
            if (start == null || end == null)
            {
                start = end = DateTime.Today.ToString("u");
            }
            List<CalendarObject> rotationList = getRotations(start, end);
            if (ID != -1)
            {
                rotationList = rotationList.Where(rot => rot.id == ID).ToList();
            }
            foreach (var rot in rotationList)
            {
                rot.url = null;
            }
            return Json(rotationList, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AbsenceData(string start, string end)
        {
            if (start == null || end == null)
            {
                start = end = DateTime.Today.ToString("u");
            }
            DateTime beginDate = DateTime.Parse(start);
            DateTime endDate = DateTime.Parse(end);
            IList<CalendarObject> absenceList = new List<CalendarObject>();
            var absences = db.outOfOffice.Include(o => o.employeeOut);
            var filteredAbsences = filterAbsences(absences, beginDate, endDate);
            foreach (var absence in filteredAbsences)
            {
                absenceList.Add(new CalendarObject
                {
                    title = absence.employeeOut.firstName + " " + absence.employeeOut.lastName,
                    start = absence.startDate.ToString("u"),
                    end = absence.startDate.AddDays(absence.numHours / 8).ToString("u"),
                    color = "yellow",
                    url = String.Format("OutOfOffices/Details/{0}", absence.ID),
                    allDay = "false"
                });
            }
            return Json(absenceList, JsonRequestBehavior.AllowGet);
        }
        private System.Collections.Hashtable getApplicationColors()
        {
            System.Collections.Hashtable colors = new System.Collections.Hashtable();
            colors.Add(3, "MediumSeaGreen");
            colors.Add(5, "Goldenrod");
            colors.Add(8, "Cyan");
            return colors;
        }
        private List<OutOfOffice> filterAbsences(IQueryable<OutOfOffice> absences, DateTime begin, DateTime end)
        {
            List<OutOfOffice> results = new List<OutOfOffice>();
            foreach (var abs in absences)
            {
                if ((abs.startDate >= begin && abs.startDate <= end) || (abs.startDate.AddDays(abs.numHours / 8) >= begin && abs.startDate.AddDays(abs.numHours / 8) <= end))
                {
                    results.Add(abs);
                }
            }
            return results;
        }

        private List<CalendarObject> getRotations(string start, string end)
        {
            DateTime beginDate = DateTime.Parse(start);
            DateTime endDate = DateTime.Parse(end);
            List<CalendarObject> rotationList = new List<CalendarObject>();
            var onCallRotations = db.onCallRotations.Include(o => o.employee);
            onCallRotations = onCallRotations.Where(rot => (rot.startDate >= beginDate && rot.startDate <= endDate) || (rot.endDate >= beginDate && rot.endDate <= endDate));
            System.Collections.Hashtable applicationColors = getApplicationColors();

            foreach (var rotation in onCallRotations)
            {
                CalendarObject temp = new CalendarObject();
                temp.id = rotation.employee.Application;
                temp.title = rotation.employee.firstName + " " + rotation.employee.lastName;
                if (!rotation.isPrimary)
                    temp.title = temp.title + " as Secondary";
                temp.start = rotation.startDate.ToString("u");
                temp.end = rotation.endDate.AddDays(1).ToString("u");
                temp.color = applicationColors[rotation.employee.Application].ToString();
                temp.url = String.Format("OnCallRotations/Details/{0}", rotation.rotationID);
                temp.allDay = "true";
                rotationList.Add(temp);
            }

            return rotationList;
        }

    }

    public class CalendarObject
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string color { get; set; }
        public string url { get; set; }
        public string allDay { get; set; }

    }

}