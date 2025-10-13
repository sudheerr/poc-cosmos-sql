using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IDataRepository<Product> _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IDataRepository<Product> productRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    // GET: api/products/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetByCategory(string category, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productRepository.FindAsync(p => p.Category == category, cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    // GET: api/products/search
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Product>>> Search(
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _productRepository.Query();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var products = query.ToList();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, "An error occurred while searching products");
        }
    }

    // GET: api/products/paged
    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                p => p.IsActive,
                cancellationToken);

            return Ok(new
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category
            };

            var createdProduct = await _productRepository.AddAsync(product, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    // PUT: api/products/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(string id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            existingProduct.Name = dto.Name;
            existingProduct.Description = dto.Description;
            existingProduct.Price = dto.Price;
            existingProduct.Category = dto.Category;
            existingProduct.IsActive = dto.IsActive;

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct, cancellationToken);
            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productRepository.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }

    // GET: api/products/count
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _productRepository.CountAsync(cancellationToken);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting products");
            return StatusCode(500, "An error occurred while counting products");
        }
    }
}
