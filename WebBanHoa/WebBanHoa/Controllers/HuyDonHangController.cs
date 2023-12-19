using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderCancellationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderCancellationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut("CancelOrder")]
        public IActionResult CancelOrder([FromBody] int order_Id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateOrderQuery = @"
                    UPDATE ORDERS 
                    SET STATUS = 'CANCEL', UPDATE_DATE = @UpdateDate 
                    WHERE ID = @Order_Id";

                using (SqlCommand updateOrderCommand = new SqlCommand(updateOrderQuery, connection))
                {
                    updateOrderCommand.Parameters.AddWithValue("@Order_Id", order_Id);
                    updateOrderCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Now);

                    int rowsAffected = updateOrderCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Đơn hàng đã được hủy thành công!");
                    }
                    else
                    {
                        return NotFound("Không tìm thấy đơn hàng cần hủy!");
                    }
                }
            }
        }
    }
}
