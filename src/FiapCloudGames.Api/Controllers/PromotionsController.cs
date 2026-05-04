using FiapCloudGames.Application.DTOs.Promotions;
using FiapCloudGames.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotions;
    public PromotionsController(IPromotionService promotions) => _promotions = promotions;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromotionResponse>>> List(CancellationToken ct)
        => Ok(await _promotions.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromotionResponse>> Get(Guid id, CancellationToken ct)
        => Ok(await _promotions.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PromotionResponse>> Create([FromBody] PromotionRequest request, CancellationToken ct)
    {
        var promo = await _promotions.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = promo.Id }, promo);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PromotionResponse>> Update(Guid id, [FromBody] PromotionRequest request, CancellationToken ct)
        => Ok(await _promotions.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _promotions.DeleteAsync(id, ct);
        return NoContent();
    }
}
