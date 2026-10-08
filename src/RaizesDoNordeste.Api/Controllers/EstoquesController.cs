using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Application.Interfaces;

namespace RaizesDoNordeste.Api.Controllers;

[ApiController]
[Route("api/estoques")]
[Authorize(Roles = "Gerente,Admin")]
public class EstoquesController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public EstoquesController(
        ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? unidadeId)
    {
        var estoques =
            await _catalogoService
                .ListarEstoquesAsync(unidadeId);

        return Ok(estoques);
    }
}
