using Api.Application.ProductTypes.Commands;
using Api.Application.ProductTypes.Commands.Handlers;
using Api.Application.ProductTypes.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/product-types")]
public class ProductTypesController : ControllerBase
{
    private readonly CreateProductTypeHandler _createProductType;

    public ProductTypesController(CreateProductTypeHandler createProductType)
    {
        _createProductType = createProductType;
    }

    [HttpPost]
    public async Task<ActionResult<ProductTypeDto>> Create([FromBody] CreateProductTypeCommand cmd, CancellationToken ct)
        => Ok(await _createProductType.Handle(cmd, ct));
}