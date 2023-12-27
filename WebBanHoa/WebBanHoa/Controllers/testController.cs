using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet(Name = "TestAPI")]
        public IActionResult GetData()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM FLOWER_CATEGORISES"; // Thay 'YourTable' bằng tên bảng trong cơ sở dữ liệu của bạn

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Đọc dữ liệu từ SqlDataReader và chuyển thành danh sách đối tượng
                        var data = new List<object>();

                        while (reader.Read())
                        {
                            // Đọc các cột từ SqlDataReader
                            // Ví dụ: Đọc cột "ColumnName" có kiểu dữ liệu là string
                            string columnName = reader.GetString(reader.GetOrdinal("FLOWER_CATE_NAME"));

                            // Tạo đối tượng từ dữ liệu đọc được
                            var item = new
                            {
                                ColumnName = columnName
                            };

                            // Thêm vào danh sách
                            data.Add(item);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Ok(data);
                    }
                }
            }
        }

        // Chức năng nút đăng ký tài khoản


    }
}