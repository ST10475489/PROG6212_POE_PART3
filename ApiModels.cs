namespace RaceDay.Web.Models
{
    // These classes match the JSON the API sends back

    public class LoginModel
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
    }

    public class IdModel
    {
        public int EventId { get; set; }
    }

    public class UserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfilePictureUrl { get; set; }
    }

    public class EventModel
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; }
        public string BannerUrl { get; set; }
    }

    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public decimal Distance { get; set; }
        public int Participants { get; set; }
    }

    public class EnrolmentModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; }
    }

    public class ResultModel
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; }
        public string FinishTime { get; set; }
        public int Position { get; set; }
        public int TotalFinishers { get; set; }
    }
}
