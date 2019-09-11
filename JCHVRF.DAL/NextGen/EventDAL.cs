using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF.DALFactory;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Dapper;
using JCHVRF_New.Model;

namespace JCHVRF.DAL.NextGen
{
    public interface IEventDAL
    {
        bool InsertEventData(string EventTitle, string EventLocation, DateTime EventStartDate, DateTime EventEndDate, string EventNotes);
        bool UpdateEventData(int? EventId, string EventTitle, string EventLocation, DateTime EventStartDate, DateTime EventEndDate, string EventNotes);
        IList<Event> GetEventList(DateTime ClickedDate);

        List<Tuple<DateTime, DateTime>> GetEvent();

        Event EditEvent(int? EventId);

        bool DeleteEventClick(int? eventid);
    }

    public class EventDAL: IEventDAL
    {
        DAOAccess _dao = new DAOAccess();
        string VRFDB = ConfigurationManager.ConnectionStrings["VRFDB"].ConnectionString;
        public bool InsertEventData(string EventTitle, string EventLocation, DateTime EventStartDate, DateTime EventEndDate, string EventNotes)
        {


            using (IDbConnection conn = new OleDbConnection(VRFDB))
            {
                int newEventID = 0;
                var availableProjects = conn.Query<int>("SELECT EVENT_ID from JCHVRF_EVENT");
                // var availableProjects = conn.Query<int>("DELETE from JCHVRF_EVENT");
                if (availableProjects.Count() < 1)
                {

                    newEventID = 1;
                }
                else
                {
                    newEventID = conn.Query<int>("SELECT MAX(EVENT_ID) from JCHVRF_EVENT").Single() + 1;
                }

                string query = "Insert into JCHVRF_EVENT (EVENT_ID, EVENT_TITLE , EVENT_LOCATION , EVENT_STARTDATE , EVENT_ENDDATE , EVENT_NOTES) values" +
                                "(" + newEventID + ", '" + EventTitle + "','" + EventLocation + "','" + EventStartDate + "','" + EventEndDate + "','" + EventNotes + "')";
                string con = _dao.GetConnString();
                //DataTable insertNameprefixRecord = new DataTable();
                int result = _dao.ExecuteSql(query, con);
                if (result > 0)
                    return true;
                else
                    return false;

            }
        }

        public bool UpdateEventData(int? EventId, string EventTitle, string EventLocation, DateTime EventStartDate, DateTime EventEndDate, string EventNotes)
        {
            using (IDbConnection conn = new OleDbConnection(VRFDB))
            {
                //int newEventID = 0;
                //var availableProjects = conn.Query<int>("SELECT EVENT_ID from JCHVRF_EVENT");
                //// var availableProjects = conn.Query<int>("DELETE from JCHVRF_EVENT");
                //if (availableProjects.Count() < 1)
                //{

                //    newEventID = 1;
                //}
                //else
                //{
                //    newEventID = conn.Query<int>("SELECT MAX(EVENT_ID) from JCHVRF_EVENT").Single() + 1;
                //}

                string query = "Update JCHVRF_EVENT set EVENT_TITLE = '" + EventTitle + "', EVENT_LOCATION = '" + EventLocation + "', EVENT_STARTDATE = '" + EventStartDate + "', EVENT_ENDDATE = '" + EventEndDate + "', EVENT_NOTES = '" + EventNotes + "' where EVENT_ID ="+ EventId + "";
                string con = _dao.GetConnString();
                //DataTable insertNameprefixRecord = new DataTable();
                int result = _dao.ExecuteSql(query, con);
                if (result > 0)
                    return true;
                else
                    return false;

            }
        }
        public IList<Event> GetEventList(DateTime ClickedDate)
        {
            List<Event> Listevt = new List<Event>();
            try
            {
                using (IDbConnection db = new OleDbConnection(VRFDB))
                {
                    /// TODO Bind From Database.
                    //List
                    //Listevt = db.Query<Event>
                    // ("SELECT * FROM JCHVRF_EVENT WHERE CONVERT(VARCHAR(10), EVENT_STARTDATE, 111) <= CONVERT(VARCHAR(10), " + ClickedDate.Date + ", 111) OR CONVERT(VARCHAR(10), EVENT_ENDDATE, 111)>= CONVERT(VARCHAR(10), " + ClickedDate.Date + ", 111)").ToList();
                    //Listevt = db.Query<Event>
                    //("SELECT EVENT_ID as EventId,EVENT_TITLE as EventTitle,EVENT_LOCATION as EventLocation,EVENT_STARTDATE as StartDate,EVENT_ENDDATE as EndDate,EVENT_NOTES as Notes FROM JCHVRF_EVENT WHERE EVENT_STARTDATE<=#" + ClickedDate.Date + "# AND EVENT_ENDDATE>=#" + ClickedDate.Date + "#").ToList();
                    Listevt = db.Query<Event>
                   ("SELECT EVENT_ID as EventId,EVENT_TITLE as EventTitle,EVENT_LOCATION as EventLocation,EVENT_STARTDATE as StartDate,EVENT_ENDDATE as EndDate,EVENT_NOTES as Notes FROM JCHVRF_EVENT WHERE EVENT_STARTDATE<=#" + ClickedDate.Date + "# AND EVENT_ENDDATE>=#" + ClickedDate.Date + "#").ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return Listevt;
        }
        public List<Tuple<DateTime, DateTime>> GetEvent()
        {
            List<Tuple<DateTime, DateTime>> listevent = new List<Tuple<DateTime, DateTime>>();
            try
            {
                using (IDbConnection db = new OleDbConnection(VRFDB))
                {
                    /// TODO Bind From Database.
                    //List
                    var Listevt = db.Query<Event>
                      ("Select EVENT_STARTDATE,EVENT_ENDDATE From JCHVRF_EVENT").ToList();
                    Listevt.ForEach((item) =>
                    {
                        listevent.Add(Tuple.Create(item.EVENT_STARTDATE, item.EVENT_ENDDATE));
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return listevent;
        }

        public Event EditEvent(int? EventId)
        {
            List<Event> Listevt = new List<Event>();
            try
            {
                using (IDbConnection db = new OleDbConnection(VRFDB))
                {
                   
                    Listevt = db.Query<Event>
                    ("SELECT EVENT_ID as EventId,EVENT_TITLE as EventTitle,EVENT_LOCATION as EventLocation,EVENT_STARTDATE as StartDate,EVENT_ENDDATE as EndDate,EVENT_NOTES as Notes FROM JCHVRF_EVENT WHERE EVENT_ID=" + EventId + "").ToList();
                    if (Listevt.Count > 0)
                        return Listevt[0];
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public bool DeleteEventClick(int? eventid)
        {
            using (IDbConnection conn = new OleDbConnection(VRFDB))
            {
                string query = "Delete from JCHVRF_EVENT WHERE EVENT_ID=" + eventid;
                string con = _dao.GetConnString();
                int result = _dao.ExecuteSql(query, con);
                if (result > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
