using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record RegisterRequest(
    [Required, MaxLength(80)] string FirstName,
    [Required, MaxLength(80)] string LastName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string Role,
    string? OrganiserCode);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record ProfileUpdateRequest(
    [Required, MaxLength(80)] string FirstName,
    [Required, MaxLength(80)] string LastName,
    [Required, EmailAddress, MaxLength(256)] string Email);

public record EventRequest(
    [Required, MaxLength(150)] string EventName,
    [MaxLength(2000)] string Description,
    DateTime EventDate,
    [Required, MaxLength(200)] string Venue,
    [Required, MaxLength(100)] string Province,
    [Required] string EventType,
    [Range(0.1, 500)] decimal DistanceKm,
    [MaxLength(200)] string? StartLocation,
    [MaxLength(200)] string? FinishLocation,
    [MaxLength(2000)] string? RouteDescription,
    [MaxLength(500)] string? MapUrl);

public record CategoryRequest(
    [Required, MaxLength(100)] string CategoryName,
    [Range(0.1, 500)] decimal DistanceKm,
    [Range(1, 100000)] int MaxParticipants,
    [Range(0, 100000)] decimal EntryFee);

public record EnrollmentRequest([Range(1, int.MaxValue)] int CategoryId);

public record ResultRequest(
    [Range(1, int.MaxValue)] int Position,
    TimeSpan FinishTime,
    [Range(0.01, 1000)] decimal PaceMinutesPerKm);

public record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, DateTime CreatedAt);
