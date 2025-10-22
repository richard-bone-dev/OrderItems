using Api.Application.ProductTypes.Commands;
using Api.Application.ProductTypes.Commands.Handlers;
using Api.Application.ProductTypes.Dtos;
using Api.Application.ProductTypes.Queries;
using Api.Application.ProductTypes.Queries.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/productTypes")]
public class ProductTypesController : ControllerBase
{
    private readonly CreateProductTypeHandler _createProductType;
    private readonly GetProductTypesHandler _getProductTypes;

    public ProductTypesController(
        CreateProductTypeHandler createProductType,
        GetProductTypesHandler getProductTypes
    )
    {
        _createProductType = createProductType;
        _getProductTypes = getProductTypes;
    }

    [HttpGet]
    public async Task<ActionResult<ProductTypeDto?>> GetAll(CancellationToken ct)
        => Ok(await _getProductTypes.Handle(new GetProductTypeQuery(), ct));

    [HttpPost]
    public async Task<ActionResult<ProductTypeDto>> Create([FromBody] CreateProductTypeCommand cmd, CancellationToken ct)
        => Ok(await _createProductType.Handle(cmd, ct));
}