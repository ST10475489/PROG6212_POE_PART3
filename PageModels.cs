namespace RaceDay.Web.Models
{
    // These classes hold everything one page needs

    public class EventDetailsModel
    {
        public EventModel Event { get; set; }
        public List<CategoryModel> Categories { get; set; }
    }

    public class DashboardRow
    {
        public EventModel Event { get; set; }
        public int TotalEnrolments { get; set; }
    }

    public class DashboardModel
    {
        public List<DashboardRow> Rows { get; set; }
    }

    public class CategoriesPageModel
    {
        public EventModel Event { get; set; }
        public List<CategoryModel> Categories { get; set; }
    }

    public class EnrolmentsPageModel
    {
        public EventModel Event { get; set; }
        public List<EnrolmentModel> Enrolments { get; set; }
    }

    public class ResultsPageModel
    {
        public EventModel Event { get; set; }
        public List<EnrolmentModel> Enrolments { get; set; }
        public List<ResultModel> Results { get; set; }
    }
}
