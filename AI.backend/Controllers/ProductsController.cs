using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI.backend.Data;
using AI.backend.Models;

namespace AI.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // POST: api/products/rate
        [HttpPost("rate")]
        public async Task<ActionResult> RateProduct([FromBody] RatingRequest request)
        {
            var rating = new Rating
            {
                ProductId = request.ProductId,
                UserId = request.UserId,
                RatingValue = request.Rating, // ← Make sure it's RatingValue here
                CreatedAt = DateTime.Now
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rating saved!" });
        }
    }

    public class RatingRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
    }
}