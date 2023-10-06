using Microsoft.AspNetCore.Identity;

namespace Projecttask.Models;

public class ApplicationUser : IdentityUser
{
    public string? ProfilePhoto { get; set; }
    public string? City { get; set; }
    public string? About { get; set; }
    public bool? isEditing { get; set; }
    public int sentOfferCount { get; set; }
    public int deletedOfferCount { get; set; }
    public ICollection<UserTag> Tags { get; set; }
}
