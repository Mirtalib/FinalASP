namespace Projecttask.Models.ViewModels;

public class UserWithTagViewModel
{
	public ApplicationUser User { get; set; }
	public List<Tag> Tags { get; set; }
}
