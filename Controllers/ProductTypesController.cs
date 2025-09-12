namespace Api.Controllers;

using Api.Application.Dtos;
using Api.Application.Interfaces;
using Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/productTypes")]
public class ProductTypesController : ControllerBase
{
    private readonly IProductTypeRepository _productTypeRepository;

    public ProductTypesController(IProductTypeRepository productTypeRepository)
    {
        _productTypeRepository = productTypeRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductTypeDto>> GetProductTypes()
    {
        var productTypes = _productTypeRepository.GetAll();

        return Ok(productTypes.OrderBy(p => p.UnitPrice).Select(ToProductTypeDto));
    }

    private static ProductTypeDto ToProductTypeDto(ProductType productType)
    {
        return new ProductTypeDto(productType.Id, productType.UnitPrice);
    }
}

//[Route("api/payments")]
//public class PaymentsController : ControllerBase
//{
//    private readonly IPaymentQueries _queries;

//    public PaymentsController(IPaymentQueries queries) => _queries = queries;

//    [HttpGet("{id:guid}/snapshot")]
//    public async Task<ActionResult<PaymentSnapshotDto>> GetSnapshot(Guid id)
//    {
//        var result = await _queries.GetSnapshotAsync(id);
//        return result is null ? NotFound() : Ok(result);
//    }

//    [HttpGet("{id:guid}/detail")]
//    public async Task<ActionResult<PaymentDetailDto>> GetDetail(Guid id)
//    {
//        var result = await _queries.GetDetailAsync(id);
//        return result is null ? NotFound() : Ok(result);
//    }
//}


//[ApiController]
//[Route("api/payments")]
//public class PaymentsController : ControllerBase
//{
//    private readonly PaymentService _service;

//    public PaymentsController(PaymentService service)
//    {
//        _service = service;
//    }

//    [HttpPost]
//    public ActionResult<MakePaymentResponse> MakePayment(
//        [FromRoute] UserId userId,
//        [FromBody] MakePaymentRequest request)
//    {
//        var result = _service.MakePayment(userId, request);
//        return Ok(result);
//    }

//    [HttpGet]
//    public ActionResult<IEnumerable<PaymentDto>> GetUserPayments([FromRoute] UserId userId)
//    {
//        var payments = _service.GetUserPayments(userId);
//        return Ok(payments);
//    }
//}
