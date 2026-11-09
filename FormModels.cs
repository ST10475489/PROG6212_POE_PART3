using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    // These classes hold what the user types into the forms

    public class LoginForm
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterForm
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please choose a role.")]
        public string Role { get; set; }
    }

    public class ProfileForm
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }

        public string ProfilePictureUrl { get; set; }
    }

    public class EventForm
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        public string Location { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; }

        public IFormFile Banner { get; set; }
        public string ExistingBannerUrl { get; set; }
    }

    public class CategoryForm
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        public string CategoryName { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        public decimal DistanceKm { get; set; }

        [Range(1, 100000, ErrorMessage = "Max participants must be at least 1.")]
        public int MaxParticipants { get; set; }
    }

    public class ResultForm
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }

        [Required]
        public string FinishTime { get; set; }

        [Range(1, 100000)]
        public int Position { get; set; }
    }
}
