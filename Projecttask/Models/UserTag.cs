namespace Projecttask.Models
{
    public class UserTag
    {
        public int Id { get; set; }

        // Foreign key to the user
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Foreign key to the tag
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
