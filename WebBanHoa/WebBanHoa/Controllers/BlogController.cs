using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : Controller
    {
        private readonly IConfiguration _configuration;

        public BlogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("LoadThongTinBlog")]
        public IActionResult GetFlowers()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<object> flowers = new List<object>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT CONTENT, DESCRIPTION, IMAGE
                    FROM FLOWERS;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var flowerInfo = new
                            {
                                Content = reader.GetString(0),
                                Description = reader.GetString(1),
                                Image = reader.GetString(2),
                            };

                            flowers.Add(flowerInfo);
                        }
                    }
                }
            }

            return Ok(flowers);
        }
    }
}
