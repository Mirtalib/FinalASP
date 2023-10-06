namespace Projecttask.Models;

public class Orders
{
	public int Id { get; set; }
	public string EmployerId { get; set; }
	public string WorkerId { get; set; }
	public ApplicationUser Employer { get; set; }
	public ApplicationUser Worker { get; set; }
	public string Message { get; set; }
	public decimal OfferPrice { get; set; }
}