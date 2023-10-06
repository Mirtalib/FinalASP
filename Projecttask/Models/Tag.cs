using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Projecttask.Models;

public class Tag
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
}
