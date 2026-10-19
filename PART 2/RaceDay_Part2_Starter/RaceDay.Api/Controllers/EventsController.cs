using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public static class UserRoles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}

public class User
{
    public int UserId { get; set; }
    [Required, MaxLength(80)] public string FirstName { get; set; } = "";
    [Required, MaxLength(80)] public string LastName { get; set; } = "";
    [Required, MaxLength(256), EmailAddress] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, MaxLength(20)] public string Role { get; set; } = UserRoles.Participant;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    [Required, MaxLength(150)] public string EventName { get; set; } = "";
    [MaxLength(2000)] public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Venue { get; set; } = "";
    [Required, MaxLength(100)] public string Province { get; set; } = "";
    [Required, MaxLength(20)] public string Status { get; set; } = "Scheduled";
    [Required, MaxLength(10)] public string EventType { get; set; } = "Run"; // Added to meet Part 2 requirement.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Route> Routes { get; set; } = new List<Route>();
}

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    [Required, MaxLength(100)] public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
    public Event? Event { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Route
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    [Required, MaxLength(150)] public string RouteName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    [MaxLength(200)] public string StartLocation { get; set; } = "";
    [MaxLength(200)] public string FinishLocation { get; set; } = "";
    [MaxLength(2000)] public string RouteDescription { get; set; } = "";
    [MaxLength(500)] public string? MapUrl { get; set; }
    public Event? Event { get; set; }
}

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int ParticipantId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(20)] public string Status { get; set; } = "Enrolled";
    [MaxLength(20)] public string? BibNumber { get; set; }
    public User? Participant { get; set; }
    public Category? Category { get; set; }
    public Result? Result { get; set; }
}

public class Result
{
    public int ResultId { get; set; }
    public int EnrollmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public decimal PaceMinutesPerKm { get; set; }
    public DateTime ResultDate { get; set; } = DateTime.UtcNow;
    public Enrollment? Enrollment { get; set; }
}
