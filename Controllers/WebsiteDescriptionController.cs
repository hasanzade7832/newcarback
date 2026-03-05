using CarAds.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ✅ این خط را اضافه کنید

[ApiController]
[Route("api/admin/website-description")]
public class WebsiteDescriptionController : ControllerBase
{
    private readonly AppDbContext _context;

    public WebsiteDescriptionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<WebsiteDescription>> GetDescription()
    {
        var desc = await _context.WebsiteDescriptions.FirstOrDefaultAsync();
        return desc ?? new WebsiteDescription { Description = "هنوز هیچ توضیحی ثبت نشده" };
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDescription([FromBody] WebsiteDescription desc)
    {
        var existing = await _context.WebsiteDescriptions.FirstOrDefaultAsync();
        if (existing != null)
        {
            existing.Description = desc.Description;
            _context.WebsiteDescriptions.Update(existing);
        }
        else
        {
            _context.WebsiteDescriptions.Add(desc);
        }
        await _context.SaveChangesAsync();
        return NoContent();
    }
}