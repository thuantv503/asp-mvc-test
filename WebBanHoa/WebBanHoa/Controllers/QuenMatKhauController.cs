using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuenMatKhauController : Controller
    {
        private readonly IConfiguration _configuration;

        public QuenMatKhauController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("QuenMatKhau")]
        public IActionResult UpdatePassword([FromBody] PasswordUpdateRequest passwordUpdate)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Kiểm tra xem email có tồn tại trong cơ sở dữ liệu không
                string checkEmailQuery = "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL = @Email";
                using (SqlCommand checkEmailCommand = new SqlCommand(checkEmailQuery, connection))
                {
                    checkEmailCommand.Parameters.AddWithValue("@Email", passwordUpdate.Email);
                    int count = (int)checkEmailCommand.ExecuteScalar();

                    if (count > 0) // Email tồn tại, cập nhật mật khẩu
                    {
                        string updatePasswordQuery = "UPDATE CUSTOMERS SET PASSWORD = @NewPassword WHERE EMAIL = @Email";
                        using (SqlCommand updatePasswordCommand = new SqlCommand(updatePasswordQuery, connection))
                        {
                            updatePasswordCommand.Parameters.AddWithValue("@Email", passwordUpdate.Email);
                            updatePasswordCommand.Parameters.AddWithValue("@NewPassword", passwordUpdate.NewPassword);
                            updatePasswordCommand.ExecuteNonQuery();
                        }

                        return Ok("Mật khẩu đã được cập nhật!");
                    }
                    else
                    {
                        return NotFound("Email không tồn tại!");
                    }
                }
            }
        }
    }
}
