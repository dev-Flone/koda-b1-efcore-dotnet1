using KodaStore.Api.Files;
using KodaStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KodaStore.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productCode}/image")]
public sealed class ProductImageController : ControllerBase
{
    private const long MaxFileBytes = 15L * 1024 * 1024;
    private const long MaxRequestBytes = 16L * 1024 * 1024;

    private IProductService _productService;
    private readonly IProductImageStorage _productImageStorage;

    public ProductImageController(IProductService productService, IProductImageStorage productImageStorage)
    {
        _productService = productService;
        _productImageStorage = productImageStorage;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Upload([FromRoute] string productCode, IFormFile? file, CancellationToken cancellationToken)
    {
        var safeCode = productCode.Trim().ToUpperInvariant();

        var validCode = safeCode.Length is >= 1 and <= 30 && safeCode.All(character => (character >= 'A' && character <= 'Z') || (character >= '0' && character <= '9') || character is '-' or '_');

        if (!validCode)
        {
            return Problem(statusCode: 400, title: "Invalid Product Code", detail: "Format Kode tidak valid");
        }

        if (file is null)
        {
            return Problem(statusCode: 400, title: "File is required", detail: "File must be inputted");
        }

        // REFUSE FILE IF EMPTY OR SIZE > 5MB
        if (file.Length <= 0 || file.Length > MaxFileBytes)
        {
            return Problem(statusCode: 400, title: "invalid File Size", detail: "File size was to big.");
        }
        var extension = Path.GetExtension(file.FileName);

        if (!string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: 400, title: "Invalid file extension.", detail: "jpg files only");
        }

        // CALLING ProductService
        var product = await _productService.GetByCodeAsync(safeCode, cancellationToken);

        if (product is null)
        {
            return Problem(statusCode: 404, title: "Produt not found.", detail: $"Product {safeCode} not found.");
        }
        var savedImage = await _productImageStorage.SaveAsync(safeCode, file, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Product image uploaded succesfully",
            productCode = safeCode,
            fileName = savedImage.StoredFileName,
            fileSize = savedImage.SizeInBytes,
            contentType = savedImage.ContextType
        });
    }
}
