using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChuDeHoaController : Controller
    {
        private readonly IConfiguration _configuration;

        public ChuDeHoaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("LaySanPhamTheoChuDe")]
        public IActionResult GetProductsByTopic(string topicName)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT p.PRODUCT_NAME, p.PRICE, p.IMAGE
            FROM PRODUCT p
            INNER JOIN TOPIC t ON p.ID = t.PRODUCT_ID
            WHERE t.TOPIC_NAME = @TopicName;";

                List<object> products = new List<object>();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TopicName", topicName);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var productInfo = new
                            {
                                ProductName = reader.GetString(0),
                                Price = reader.GetInt32(1),
                                Image = reader.GetString(2),
                            };

                            products.Add(productInfo);
                        }
                    }
                }

                return Ok(products);
            }
        }

    }
}
