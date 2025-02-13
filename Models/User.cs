using Microsoft.AspNetCore.Identity;

namespace MovieReservation.Models;

public class User : IdentityUser
{
    public string? FullName { get; set; }
}