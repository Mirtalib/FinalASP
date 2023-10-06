using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Projecttask.Data;
using Projecttask.Models;
using Projecttask.Models.ViewModels;
using Azure.Core;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Projecttask.Controllers;

[Authorize(Roles = "Worker")]
public class WorkerController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public WorkerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ApplicationUser getUser = await _userManager.GetUserAsync(User);
        var orders = _context.Orders.Where(o => o.WorkerId == getUser.Id).Include(o => o.Employer).Include(o => o.Worker).ToList();
		return View(orders);
    }

    public async Task<IActionResult> Profile()
    {
        ApplicationUser getUser = await _userManager.GetUserAsync(User);
        ViewData["Username"] = getUser;
        ViewData["City"] = getUser.City;
        ViewData["isEditing"] = getUser.isEditing;
        ViewData["sentOfferCount"] = getUser.sentOfferCount;
        ViewData["deletedOfferCount"] = getUser.deletedOfferCount;
        ViewData["Tags"] = _context.UserTag.Include(ut => ut.Tag).Where(ut => ut.UserId == getUser.Id).Select(ut => ut.Tag).ToList();
        ViewData["DefaultTags"] = _context.Tag.ToList();
        ViewData["About"] = getUser.About;
        return View();
    }

    private void SetProfileViewData(ApplicationUser user)
    {
        ViewData["Username"] = user;
        ViewData["City"] = user.City;
        ViewData["isEditing"] = user.isEditing;
        ViewData["Tags"] = _context.UserTag.Include(ut => ut.Tag).Where(ut => ut.UserId == user.Id).Select(ut => ut.Tag).ToList();
        ViewData["DefaultTags"] = _context.Tag.ToList();
        ViewData["About"] = user.About;
    }

    public async Task<JsonResult> EditPage([FromBody] EditPageViewModel editPageData)
    {
        ApplicationUser getUser = await _userManager.GetUserAsync(User);
        var dbUser = _context.Users.FirstOrDefault(user => user.Id == getUser.Id);
        if (dbUser != null)
        {
            dbUser.isEditing = !dbUser.isEditing;
            _context.Entry(dbUser).State = EntityState.Modified;
            _context.SaveChanges();
            SetProfileViewData(dbUser);
            return Json(new { isEditing = !dbUser.isEditing });
        }
        return Json(new { isEditing = false });
    }

    public async Task<JsonResult> SavePage([FromBody] EditPageViewModel editPageData)
    {
        if (!string.IsNullOrWhiteSpace(editPageData.UserName))
        {
            ApplicationUser getUser = await _userManager.GetUserAsync(User);
            var dbUser = _context.Users.FirstOrDefault(user => user.Id == getUser.Id);
            var canChangeUsername = await _userManager.FindByNameAsync(editPageData.UserName);
            if (dbUser != null && canChangeUsername == null || dbUser.UserName == editPageData.UserName)
            {
                var newNormalizedUserName = _userManager.NormalizeName(editPageData.UserName);
                dbUser.isEditing = !dbUser.isEditing;
                dbUser.UserName = editPageData.UserName;
                dbUser.NormalizedUserName = newNormalizedUserName;
                dbUser.City = editPageData.City;
                dbUser.About = editPageData.About;
                _context.Entry(dbUser).State = EntityState.Modified;
                _context.SaveChanges();
                SetProfileViewData(dbUser);
                Log.Information($"User Edited Profile. Email: {getUser.Email}");
                return Json(new { isEditing = !dbUser.isEditing });
            }
        }
        return Json(new { isEditing = false }) ;
    }

    public async Task<IActionResult> DeleteOffer(int offerId)
    {
		var order = await _context.Orders.FindAsync(offerId);

		if (order == null)
		{
			return NotFound("Order not found.");
		}

		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Unauthorized("User not found.");
		}

		if (order.EmployerId != user.Id && order.WorkerId != user.Id)
		{
			return Forbid("User is not authorized to delete this order.");
		}

		_context.Orders.Remove(order);
		await _context.SaveChangesAsync();

        user.deletedOfferCount += 1;

        await _userManager.UpdateAsync(user);
        Log.Information($"User Deleted Offer. EmployerID: {order.EmployerId} Message: {order.Message} Price: {order.OfferPrice}");
        var orders = _context.Orders.Where(o => o.WorkerId == user.Id).Include(o => o.Employer).Include(o => o.Worker).ToList();
		return View("Index", orders);
    }

	[HttpPost]
    public async Task<IActionResult> AddTag([FromBody] TagRequestViewModel request)
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);
        var tag = _context.Tag.FirstOrDefault(tag => tag.Name == request.TagName);

        if (user == null || tag == null)
        {
            return NotFound();
        }

        var existingUserTag = await _context.UserTag
            .FirstOrDefaultAsync(ut => ut.UserId == user.Id && ut.TagId == tag.Id);

        if (existingUserTag != null)
        {
            return BadRequest("User already has this tag.");
        }

        var newUserTag = new UserTag
        {
            UserId = user.Id,
            TagId = tag.Id
        };

        _context.UserTag.Add(newUserTag);
        await _context.SaveChangesAsync();

        return Ok("Tag added to the user.");

    }

    [HttpPost]
    public async Task<IActionResult> RemoveTag([FromBody] TagRequestViewModel request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TagName))
        {
            return BadRequest("Invalid request.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var tag = await _context.Tag.FirstOrDefaultAsync(t => t.Name == request.TagName);

        if (tag == null)
        {
            return NotFound($"Tag '{request.TagName}' not found.");
        }

        var userTag = await _context.UserTag.FirstOrDefaultAsync(ut => ut.UserId == user.Id && ut.TagId == tag.Id);

        if (userTag == null)
        {
            return NotFound($"Tag '{request.TagName}' not associated with the user.");
        }

        _context.UserTag.Remove(userTag);
        await _context.SaveChangesAsync();
        return Ok($"Tag '{request.TagName}' removed successfully.");
    }

}
