using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DangKyTaiKhoanController : Controller
    {
        private readonly IConfiguration _configuration;

        public DangKyTaiKhoanController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "DangKyTaiKhoanAPI")]
        public IActionResult InsertData([FromBody] Customers newData)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO CUSTOMERS (PHONE, CUSTOMER_NAME, ADDRESS, PASSWORD, EMAIL, STATUS) VALUES (@Value1, @Value2, @Value3, @Value4, @Value5, 'SUCCESS')";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Value1", newData.PHONE);
                    command.Parameters.AddWithValue("@Value2", newData.CUSTOMER_NAME);
                    command.Parameters.AddWithValue("@Value3", newData.ADDRESS);
                    command.Parameters.AddWithValue("@Value4", newData.PASSWORD);
                    command.Parameters.AddWithValue("@Value5", newData.EMAIL);



                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Đăng ký thành công.");
                    }
                    else
                    {
                        return BadRequest("Đăng ký thất bại.");
                    }
                }
            }
        }

    }
}
