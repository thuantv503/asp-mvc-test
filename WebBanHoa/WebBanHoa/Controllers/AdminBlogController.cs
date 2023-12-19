using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminBlogController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminBlogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("ThemBlog")]
        public IActionResult AddFlower([FromBody] FlowerInfo flowerInfo)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"
            INSERT INTO FLOWERS (CONTENT, DESCRIPTION, IMAGE, STATUS)
            VALUES (@Content, @Description, @Image, 'SUCCESS')";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Content", flowerInfo.Content);
                    command.Parameters.AddWithValue("@Description", flowerInfo.Description);
                    command.Parameters.AddWithValue("@Image", flowerInfo.Image);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Thông tin hoa đã được thêm thành công.");
                    }
                    else
                    {
                        return BadRequest("Không thể thêm thông tin hoa.");
                    }
                }
            }
        }

        [HttpGet("LoadDanhSachBlog")]
        public IActionResult GetFlowersInfo()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            List<FlowerInfo> flowers = new List<FlowerInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT Content, Description, Image
                    FROM FLOWERS
                    WHERE STATUS = 'SUCCESS';";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FlowerInfo flower = new FlowerInfo
                            {
                                Content = reader["Content"].ToString(),
                                Description = reader["Description"].ToString(),
                                Image = reader["Image"].ToString()
                            };
                            flowers.Add(flower);
                        }
                    }
                }
            }

            return Ok(flowers);
        }

    }
    public class FlowerInfo
    {
        public string Content { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }

}
