using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class UploadsController : ApiControllerBase
    {
        private readonly AppDbContext _db;
        private readonly BlobService _blobs;

        public UploadsController(AppDbContext db, BlobService blobs)
        {
            _db = db;
            _blobs = blobs;
        }

        /// <summary>Uploads a banner image for an event to Azure Blob Storage.</summary>
        /// <remarks>Organiser who owns the event only. Send the image as form-data field "file" (JPG, PNG, GIF or WEBP, max 2 MB). Returns the image URL.</remarks>
        [HttpPost("api/events/{id}/banner")]
        [Authorize(Roles = "Organiser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UploadBanner(int id, IFormFile file)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            string error = BlobService.Validate(file);
            if (error != null) return BadRequest(error);

            try
            {
                raceEvent.BannerUrl = _blobs.Upload(file, "banners");
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return StatusCode(500, "Could not store the image. Check the Azure storage settings.");
            }

            return Ok(new { bannerUrl = raceEvent.BannerUrl });
        }

        /// <summary>Uploads the logged-in user's profile picture to Azure Blob Storage.</summary>
        /// <remarks>Any role, own id only. Send the image as form-data field "file" (JPG, PNG, GIF or WEBP, max 2 MB). Returns the image URL.</remarks>
        [HttpPost("api/users/{id}/picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UploadPicture(int id, IFormFile file)
        {
            if (id != CurrentUserId()) return Forbid();

            var user = _db.Users.Find(id);
            if (user == null) return NotFound("User does not exist.");

            string error = BlobService.Validate(file);
            if (error != null) return BadRequest(error);

            try
            {
                user.ProfilePictureUrl = _blobs.Upload(file, "profiles");
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return StatusCode(500, "Could not store the image. Check the Azure storage settings.");
            }

            return Ok(new { profilePictureUrl = user.ProfilePictureUrl });
        }
    }
}
