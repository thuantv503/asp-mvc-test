using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminThemKhuyenMaiController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminThemKhuyenMaiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("LoadDanhSachSanPham")]
        public IActionResult GetAllProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            List<object> products = new List<object>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT IMAGE, PRODUCT_NAME, PRICE 
                    FROM PRODUCT 
                    WHERE STATUS = 'SUCCESS'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var productInfo = new
                            {
                                Image = reader.GetString(0),
                                ProductName = reader.GetString(1),
                                Price = reader.GetInt32(2)
                            };

                            products.Add(productInfo);
                        }
                    }
                }
            }

            return Ok(products);
        }

        [HttpPost("ThemKhuyenMai")]
        public IActionResult AddPromotion([FromBody] PromotionModel promotionModel)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Thêm thông tin khuyến mãi vào bảng PROMOTION
                        string insertPromotionQuery = @"
                            INSERT INTO PROMOTION (ID, PROMOTION_NAME, PROMOTIONAL_PRICE, DESCRIPTION, BEGIN_DATE, END_DATE, PRODUCT_ID, CREATE_DATE, UPDATE_DATE, STATUS)
                            VALUES (@Id, @PromotionName, @PromotionalPrice, @PromotionDescription, @BeginDate, @EndDate, @ProductId, @CreateDate, @UpdateDate, @Status);";

                        foreach (var id in promotionModel.ProductIds)
                        {
                            using (SqlCommand insertPromotionCommand = new SqlCommand(insertPromotionQuery, connection, transaction))
                            {
                                insertPromotionCommand.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString().Substring(0, 6));
                                insertPromotionCommand.Parameters.AddWithValue("@PromotionName", promotionModel.PromotionName);
                                insertPromotionCommand.Parameters.AddWithValue("@PromotionalPrice", promotionModel.PromotionalPrice);
                                insertPromotionCommand.Parameters.AddWithValue("@PromotionDescription", promotionModel.PromotionDescription);
                                insertPromotionCommand.Parameters.AddWithValue("@BeginDate", promotionModel.BeginDate);
                                insertPromotionCommand.Parameters.AddWithValue("@EndDate", promotionModel.EndDate);
                                insertPromotionCommand.Parameters.AddWithValue("@ProductId", id);
                                insertPromotionCommand.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                                insertPromotionCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
                                insertPromotionCommand.Parameters.AddWithValue("@Status", "SUCCESS");

                                insertPromotionCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return Ok("Thêm khuyến mãi thành công!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
                    }
                }
            }

        }
        [HttpGet("LoadDanhSachKhuyenMai")]
        public IActionResult GetPromotions()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<PromotionInfo> promotions = new List<PromotionInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT PROMOTION_NAME, BEGIN_DATE, END_DATE
                FROM PROMOTION
                WHERE STATUS = 'SUCCESS';";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var promotion = new PromotionInfo
                            {
                                PromotionName = reader.GetString(0),
                                BeginDate = reader.GetDateTime(1),
                                EndDate = reader.GetDateTime(2)
                            };
                            promotions.Add(promotion);
                        }
                    }
                }
            }

            return Ok(promotions);
        }

        [HttpPut("KetThucKhuyenMai")]
        public IActionResult EndPromotion(string promotionId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateQuery = @"
                UPDATE PROMOTION 
                SET STATUS = 'CANCEL'
                WHERE ID = @PromotionId";

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@PromotionId", promotionId);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok($"Chương trình khuyến mãi có ID {promotionId} đã kết thúc.");
                    }
                    else
                    {
                        return NotFound("Xóa thất bại.");
                    }
                }
            }
        }


    }
    public class PromotionModel
    {
        public string PromotionName { get; set; }
        public int PromotionalPrice { get; set; }
        public string PromotionDescription { get; set; }
        public List<int> ProductIds { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PromotionInfo
    {
        public string PromotionName { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
