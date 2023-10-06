using System.ComponentModel.DataAnnotations;

namespace Projecttask.Models.ViewModels;

public class AddCategoryViewModel
{
    [Required(ErrorMessage = "Category Name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Category Name must be between 2 and 50 characters.")]
    [Display(Name = "Tag Name")]
    public string TagName { get; set; }
}
