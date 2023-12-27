using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminThemSanPhamController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminThemSanPhamController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("ThemSanPham")]
        public IActionResult AddProductTopic([FromBody] ProductTopicInfo productTopicInfo)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Thêm thông tin vào bảng Product
                string insertProductQuery = @"
                    INSERT INTO PRODUCT (PRODUCT_NAME, COST, PRICE, IMAGE, DESCRIPTION, STATUS)
                    VALUES (@ProductName, @Cost, @Price, @Image, @Description, 'SUCCESS');
                    SELECT SCOPE_IDENTITY();";

                int productId;

                using (SqlCommand insertProductCommand = new SqlCommand(insertProductQuery, connection))
                {
                    insertProductCommand.Parameters.AddWithValue("@ProductName", productTopicInfo.ProductName);
                    insertProductCommand.Parameters.AddWithValue("@Cost", productTopicInfo.Cost);
                    insertProductCommand.Parameters.AddWithValue("@Price", productTopicInfo.Price);
                    insertProductCommand.Parameters.AddWithValue("@Image", productTopicInfo.Image);
                    insertProductCommand.Parameters.AddWithValue("@Description", productTopicInfo.Description);

                    productId = Convert.ToInt32(insertProductCommand.ExecuteScalar());
                }

                // Thêm thông tin vào bảng Topic
                string insertTopicQuery = @"
                    INSERT INTO TOPIC (TOPIC_NAME, PRODUCT_ID)
                    VALUES (@TopicName, @ProductId);";

                using (SqlCommand insertTopicCommand = new SqlCommand(insertTopicQuery, connection))
                {
                    insertTopicCommand.Parameters.AddWithValue("@TopicName", productTopicInfo.TopicName);
                    insertTopicCommand.Parameters.AddWithValue("@ProductId", productId);

                    insertTopicCommand.ExecuteNonQuery();
                }

                return Ok("Thêm thông tin sản phẩm và chủ đề thành công!");
            }
        }

        [HttpPut("XoaSanPham")]
        public IActionResult CancelProduct([FromBody] int productId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateProductQuery = @"
                    UPDATE PRODUCT 
                    SET STATUS = 'CANCEL', UPDATE_DATE = @UpdateDate 
                    WHERE ID = @ProductId";

                using (SqlCommand updateProductCommand = new SqlCommand(updateProductQuery, connection))
                {
                    updateProductCommand.Parameters.AddWithValue("@ProductId", productId);
                    updateProductCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Now);

                    int rowsAffected = updateProductCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok("Sản phẩm đã được xóa thành công!");
                    }
                    else
                    {
                        return NotFound("Xóa không thành công!");
                    }
                }
            }
        }

        [HttpPut("SuaSanPham")]
        public IActionResult UpdateProductAndTopic([FromBody] ProductUpdateTopicInfo productTopic)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Cập nhật thông tin sản phẩm
                        string updateProductQuery = @"
                            UPDATE PRODUCT 
                            SET PRODUCT_NAME = @ProductName, COST = @Cost, PRICE = @Price, DESCRIPTION = @Description, IMAGE = @Image
                            WHERE ID = @ProductId";

                        using (SqlCommand updateProductCommand = new SqlCommand(updateProductQuery, connection, transaction))
                        {
                            updateProductCommand.Parameters.AddWithValue("@ProductId", productTopic.ProductId);
                            updateProductCommand.Parameters.AddWithValue("@ProductName", productTopic.ProductName);
                            updateProductCommand.Parameters.AddWithValue("@Cost", productTopic.Cost);
                            updateProductCommand.Parameters.AddWithValue("@Price", productTopic.Price);
                            updateProductCommand.Parameters.AddWithValue("@Description", productTopic.Description);
                            updateProductCommand.Parameters.AddWithValue("@Image", productTopic.Image);

                            updateProductCommand.ExecuteNonQuery();
                        }

                        // Cập nhật thông tin chủ đề
                        string updateTopicQuery = @"
                            UPDATE TOPIC 
                            SET TOPIC_NAME = @TopicName
                            WHERE PRODUCT_ID = @ProductId";

                        using (SqlCommand updateTopicCommand = new SqlCommand(updateTopicQuery, connection, transaction))
                        {
                            updateTopicCommand.Parameters.AddWithValue("@ProductId", productTopic.ProductId);
                            updateTopicCommand.Parameters.AddWithValue("@TopicName", productTopic.TopicName);

                            updateTopicCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        return Ok("Thông tin sản phẩm và chủ đề đã được cập nhật thành công!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
                    }
                }
            }
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
                    SELECT IMAGE, PRODUCT_NAME, COST, PRICE, DESCRIPTION 
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
                                Cost = reader.GetInt32(2),
                                Price = reader.GetInt32(3),
                                Description = reader.GetString(4)
                            };

                            products.Add(productInfo);
                        }
                    }
                }
            }

            return Ok(products);
        }
    }

    public class ProductTopicInfo
    {
        public string TopicName { get; set; }
        public string ProductName { get; set; }
        public int Cost { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        
    }
    public class ProductUpdateTopicInfo
    {
        public int ProductId { get; set; }
        public string TopicName { get; set; }
        public string ProductName { get; set; }
        public int Cost { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

    }
}
