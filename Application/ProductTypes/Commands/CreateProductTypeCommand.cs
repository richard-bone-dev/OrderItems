namespace Api.Application.ProductTypes.Commands;

public record CreateProductTypeCommand(string Name, decimal UnitPrice);