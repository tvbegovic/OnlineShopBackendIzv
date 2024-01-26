using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace OnlineShopBackendIzv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ProductController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("")]
        public List<Product> GetAlLProducts()
        {
            using(var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                return db.Query<Product>("SELECT * FROM Product").ToList();
            }
        }

        [HttpGet("{id}")]
        public Product? GetProduct(int id)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                return db.QueryFirstOrDefault<Product>("SELECT * FROM Product WHERE id = @id", new { id });
            }
        }

        [HttpPost("")]
        [Authorize]
        public IActionResult Create(Product product)
        {
            if(string.IsNullOrEmpty(product.Name))
            {
                return BadRequest("Naziv proizvoda je obavezan");
            }
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = @"INSERT INTO Product(
                Name,Code,Price,IdManufacturer,IdCategory
                ) OUTPUT inserted.id VALUES(
                @Name,@Code,@Price,@IdManufacturer,@IdCategory
                )";
                product.ID = db.ExecuteScalar<int>(sql, product);
                return Ok(product);
            }
        }

        [HttpPut("")]
        [Authorize]
        public IActionResult Update(Product product)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                return BadRequest("Naziv proizvoda je obavezan");
            }
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = @"UPDATE Product SET 
                Name=@Name,Code=@Code,Price=@Price,IdManufacturer=@IdManufacturer,IdCategory=@IdCategory
                WHERE id = @id";
                db.Execute(sql, product);
                return Ok();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = "DELETE FROM Product WHERE id = @id";
                db.Execute(sql, new { id });
            }
        }

        [HttpGet("search/{text}")]
        public List<Product> Search(string text)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = @"SELECT Product.*
                    FROM Product INNER JOIN Manufacturer ON Product.IdManufacturer = Manufacturer.id
                    INNER JOIN Category ON Product.IdCategory = Category.id
                    WHERE Product.Name LIKE @text OR Manufacturer.Name LIKE @text OR Category.name LIKE @text";
                return db.Query<Product>(sql, new { text = $"%{text}%" }).ToList();
            }
        }
    }
}
