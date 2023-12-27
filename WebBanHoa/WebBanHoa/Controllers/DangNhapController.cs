using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DangNhapController : Controller
    {
        private readonly IConfiguration _configuration;

        public DangNhapController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "DangNhapAPI")]
        public IActionResult Login([FromBody] CustomersLogin loginData)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Email, Password FROM Customers WHERE Email = @Email AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", loginData.EMAIL);
                    command.Parameters.AddWithValue("@Password", loginData.PASSWORD);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            
                            // Thực hiện logic xác thực thành công ở đây
                            // Ví dụ: Trả về mã thông báo JWT hoặc thông báo xác thực thành công
                            return Ok("Đăng nhập thành công!");
                        }
                        else
                        {
                            // Tên đăng nhập hoặc mật khẩu không đúng
                            return Unauthorized("Email hoặc mật khẩu không đúng!");
                        }
                    }
                }
            }
        }

        // Chức năng nút đăng ký tài khoản


    }
}
