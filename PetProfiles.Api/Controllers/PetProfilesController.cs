using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetProfiles.Api.Data;
using PetProfiles.Api.Models;

namespace PetProfiles.Api.Controllers;

/// <summary>
/// Controller for managing pet profile CRUD operations.
/// Image operations are handled by the ImagesController.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class PetProfilesController : BaseController
{
    private readonly PetProfilesDbContext _context;

    public PetProfilesController(
        PetProfilesDbContext context, 
        ILogger<PetProfilesController> logger) 
        : base(logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all pet profiles.
    /// </summary>
    /// <returns>Collection of pet profiles.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetProfile>>> GetPetProfiles()
    {
        Logger.LogInformation("Getting all pet profiles");
        
        var petProfiles = await _context.PetProfiles.ToListAsync();
        
        return petProfiles;
    }

    /// <summary>
    /// Gets a specific pet profile by ID.
    /// </summary>
    /// <param name="id">The pet profile ID.</param>
    /// <returns>The pet profile.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PetProfile>> GetPetProfile(int id)
    {
        Logger.LogInformation("Getting pet profile with ID: {Id}", id);
        
        var petProfile = await GetPetProfileByIdAsync(id);
        if (petProfile == null)
        {
            return NotFound();
        }

        return petProfile;
    }

    /// <summary>
    /// Creates a new pet profile.
    /// </summary>
    /// <param name="petProfile">The pet profile to create.</param>
    /// <returns>The created pet profile.</returns>
    [HttpPost]
    public async Task<ActionResult<PetProfile>> CreatePetProfile(PetProfile petProfile)
    {
        if (petProfile == null)
        {
            return BadRequest("Pet profile data is required");
        }

        Logger.LogInformation("Creating new pet profile: {Name}", petProfile.Name);
        
        SetPetProfileTimestamps(petProfile, isNew: true);
        
        _context.PetProfiles.Add(petProfile);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPetProfile), new { id = petProfile.Id }, petProfile);
    }

    /// <summary>
    /// Updates an existing pet profile.
    /// </summary>
    /// <param name="id">The pet profile ID.</param>
    /// <param name="petProfile">The updated pet profile data.</param>
    /// <returns>No content on successful update.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePetProfile(int id, PetProfile petProfile)
    {
        if (petProfile == null)
        {
            return BadRequest("Pet profile data is required");
        }

        if (id != petProfile.Id)
        {
            return BadRequest("ID mismatch");
        }

        Logger.LogInformation("Updating pet profile with ID: {Id}", id);

        var existingPet = await GetPetProfileByIdAsync(id);
        if (existingPet == null)
        {
            return NotFound();
        }

        UpdatePetProfileProperties(existingPet, petProfile);
        SetPetProfileTimestamps(existingPet, isNew: false);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PetProfileExistsAsync(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a pet profile.
    /// Note: Associated images should be deleted separately via the ImagesController.
    /// </summary>
    /// <param name="id">The pet profile ID.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePetProfile(int id)
    {
        Logger.LogInformation("Deleting pet profile with ID: {Id}", id);

        var petProfile = await GetPetProfileByIdAsync(id);
        if (petProfile == null)
        {
            return NotFound();
        }

        try
        {
            _context.PetProfiles.Remove(petProfile);
            await _context.SaveChangesAsync();

            Logger.LogInformation("Pet profile deleted successfully: {Name}", petProfile.Name);
            return NoContent();
        }
        catch (Exception ex)
        {
            LogAndReturnError(ex, $"Error deleting pet profile with ID: {id}");
            return StatusCode(500, "Error deleting pet profile");
        }
    }

    /// <summary>
    /// Gets a pet profile by ID with proper error handling.
    /// </summary>
    /// <param name="id">The pet profile ID.</param>
    /// <returns>The pet profile or null if not found.</returns>
    private async Task<PetProfile?> GetPetProfileByIdAsync(int id)
    {
        var petProfile = await _context.PetProfiles.FindAsync(id);
        
        if (petProfile == null)
        {
            Logger.LogWarning("Pet profile with ID {Id} not found", id);
        }
        
        return petProfile;
    }

    /// <summary>
    /// Updates pet profile properties from another pet profile.
    /// </summary>
    /// <param name="target">The target pet profile to update.</param>
    /// <param name="source">The source pet profile with new values.</param>
    private static void UpdatePetProfileProperties(PetProfile target, PetProfile source)
    {
        target.Name = source.Name;
        target.Breed = source.Breed;
        target.Age = source.Age;
        target.ImageUrl = source.ImageUrl;
    }

    /// <summary>
    /// Sets the timestamps for a pet profile.
    /// </summary>
    /// <param name="petProfile">The pet profile to update.</param>
    /// <param name="isNew">Whether this is a new pet profile.</param>
    private static void SetPetProfileTimestamps(PetProfile petProfile, bool isNew)
    {
        var now = DateTime.UtcNow;
        
        if (isNew)
        {
            petProfile.CreatedAt = now;
        }
        
        petProfile.UpdatedAt = now;
    }

    /// <summary>
    /// Checks if a pet profile exists by ID.
    /// </summary>
    /// <param name="id">The pet profile ID.</param>
    /// <returns>True if the pet profile exists, false otherwise.</returns>
    private async Task<bool> PetProfileExistsAsync(int id) => 
        await _context.PetProfiles.AnyAsync(e => e.Id == id);

    /// <summary>
    /// Logs an error and returns a standardized error response.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="message">The error message to log.</param>
    private void LogAndReturnError(Exception ex, string message) => 
        Logger.LogError(ex, message);
} 