using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projecttask.Data;
using Projecttask.Models;
using Projecttask.Models.ViewModels;
using Serilog;

namespace Projecttask.Controllers;

[Authorize(Roles = "Employer")]
public class EmployerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public EmployerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var workers = await _userManager.GetUsersInRoleAsync("Worker");
        var usersWithTags = new List<UserWithTagViewModel>();

        foreach (var worker in workers)
        {
            var tags = _context.UserTag.Where(ut => ut.UserId == worker.Id).Select(ut => ut.Tag).ToList();
            var userWithTagViewModel = new UserWithTagViewModel
            {
                User = worker,
                Tags = tags
            };

            usersWithTags.Add(userWithTagViewModel);
        }

        return View(usersWithTags);
    }

    public async Task<IActionResult> Profile(string? userid)
    {
        if (string.IsNullOrEmpty(userid))
        {
            return BadRequest("Invalid User ID");
        }

        var user = await _userManager.FindByIdAsync(userid);

        if (user == null)
        {
            return NotFound("User not found...");
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> MakeOffer([FromBody] Orders order)
    {
        var user = await _userManager.GetUserAsync(User);
        var worker = await _context.Users.FindAsync(order.WorkerId);
        if (user == null || worker == null)
        {
            return Unauthorized("User not found...");
        }

        if (!await _userManager.IsInRoleAsync(user, "Employer"))
        {
            return Forbid("User is not authorized to create an order.");
        }

        worker.sentOfferCount += 1;

        await _userManager.UpdateAsync(worker);


        order.EmployerId = user.Id;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        Log.Information($"Succesfully Offer Created. EmployerID: {order.EmployerId} WorkerID: {order.WorkerId}");
        return Ok("Succesfully Offer Created.");
    }
}
