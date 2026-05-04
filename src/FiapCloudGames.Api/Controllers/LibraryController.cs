using FiapCloudGames.Application.DTOs.Library;
using FiapCloudGames.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Api.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _library;
    public LibraryController(ILibraryService library) => _library = library;

    [HttpPost("acquire/{gameId:guid}")]
    public async Task<IActionResult> Acquire(Guid gameId, CancellationToken ct)
    {
        await _library.AcquireAsync(gameId, ct);
        return NoContent();
    }

    [HttpGet("my-games")]
    public async Task<ActionResult<IReadOnlyList<UserGameResponse>>> MyGames(CancellationToken ct)
        => Ok(await _library.GetMyLibraryAsync(ct));

    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<UserGameResponse>>> UserLibrary(Guid userId, CancellationToken ct)
        => Ok(await _library.GetUserLibraryAsync(userId, ct));
}
