using HotelBooking.Api.Contracts;
using HotelBooking.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class UploadsController(IWebHostEnvironment environment) : ControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".jfif",
        ".png",
        ".webp",
        ".gif",
        ".bmp",
        ".avif"
    };

    [Authorize]
    [HttpPost("uploads/images")]
    [HttpPost("admin/uploads/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType<AdminImageUploadResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminImageUploadResult>> UploadImage([FromForm] IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Image file is required." });
        }

        if (file.Length > MaxUploadBytes)
        {
            return BadRequest(new { error = "Image file must be 20 MB or smaller." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = "Only jpg, jpeg, jfif, png, webp, gif, bmp, and avif images are supported." });
        }

        var directory = ImageDirectory();
        Directory.CreateDirectory(directory);

        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(directory, fileName);

        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }

        return Created($"/api/uploads/images/{fileName}", new AdminImageUploadResult($"/api/uploads/images/{fileName}"));
    }

    [AllowAnonymous]
    [HttpGet("uploads/images/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetImage(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var path = Path.Combine(ImageDirectory(), safeFileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(path, ContentTypeFor(path));
    }

    private string ImageDirectory()
    {
        return Path.Combine(environment.ContentRootPath, "uploads", "images");
    }

    private static string ContentTypeFor(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".jfif" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".avif" => "image/avif",
            _ => "application/octet-stream"
        };
    }
}
