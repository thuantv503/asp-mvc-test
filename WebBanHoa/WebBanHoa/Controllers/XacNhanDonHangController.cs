using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class XacNhanDonHangController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public XacNhanDonHangController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("XacNhanDonHang")]
        public IActionResult ConfirmOrder([FromBody] OrderRequest orderConfirmation)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open(); 

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Thêm đơn hàng mới vào bảng Orders
                        string insertOrderQuery = @"
                    INSERT INTO Orders (Note, Customer_Id, Promotion_Id, Initial_Price, Final_Price, Create_Date, Update_Date, Status)
                    VALUES (@Note, @Customer_Id, @Promotion_Id, @Initial_Price, @Final_Price, @Create_Date, @Update_Date, @Status);
                    SELECT SCOPE_IDENTITY();";

                        int orderId;

                        using (SqlCommand insertOrderCommand = new SqlCommand(insertOrderQuery, connection, transaction))
                        {
                            insertOrderCommand.Parameters.AddWithValue("@Note", orderConfirmation.Note);
                            insertOrderCommand.Parameters.AddWithValue("@Customer_Id", orderConfirmation.CustomerId);
                            // Kiểm tra và xử lý Promotion_Id
                            object promotionIdParamValue = DBNull.Value;
                            if (!string.IsNullOrEmpty(orderConfirmation.PromotionId) && orderConfirmation.PromotionId.StartsWith("PRO"))
                            {
                                promotionIdParamValue = orderConfirmation.PromotionId;
                            }

                            insertOrderCommand.Parameters.AddWithValue("@Promotion_Id", promotionIdParamValue);

                            insertOrderCommand.Parameters.AddWithValue("@Initial_Price", orderConfirmation.InitialPrice);
                            insertOrderCommand.Parameters.AddWithValue("@Final_Price", orderConfirmation.FinalPrice);
                            insertOrderCommand.Parameters.AddWithValue("@Create_Date", DateTime.Now);
                            insertOrderCommand.Parameters.AddWithValue("@Update_Date", DateTime.Now);
                            insertOrderCommand.Parameters.AddWithValue("@Status", "SUCCESS");

                            orderId = Convert.ToInt32(insertOrderCommand.ExecuteScalar());
                        }

                        // Thêm chi tiết đơn hàng vào bảng OrderDetails
                        foreach (var product in orderConfirmation.Products)
                        {
                            string insertOrderDetailQuery = @"
                        INSERT INTO Order_Details (Order_Id, Product_Id, Quantity, Create_Date, Update_Date)
                        VALUES (@Order_Id, @Product_Id, @Quantity, @Create_Date, @Update_Date);";

                            using (SqlCommand insertOrderDetailCommand = new SqlCommand(insertOrderDetailQuery, connection, transaction))
                            {
                                insertOrderDetailCommand.Parameters.AddWithValue("@Order_Id", orderId);
                                insertOrderDetailCommand.Parameters.AddWithValue("@Product_Id", product.ProductId);
                                insertOrderDetailCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
                                insertOrderDetailCommand.Parameters.AddWithValue("@Create_Date", DateTime.Now);
                                insertOrderDetailCommand.Parameters.AddWithValue("@Update_Date", DateTime.Now);

                                insertOrderDetailCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return Ok("Order confirmed successfully!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, $"An error occurred: {ex.Message}");
                    }
                }
            }
        }


        // Đoạn này là hàm GetProductPrice để lấy giá sản phẩm từ CSDL, bạn cần implement nó
        private decimal GetProductPrice(int productId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Price FROM Product WHERE Id = @ProductId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 0; // Trả về giá mặc định hoặc xử lý khi không tìm thấy giá
        }


        public class OrderRequest
        {
            public string Note { get; set; }
            public int CustomerId { get; set; }
            public string PromotionId { get; set; }
            public int InitialPrice { get; set; }
            public int FinalPrice { get; set; }

            public List<ProductQuantity> Products { get; set; }
        }

        public class ProductQuantity
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost("LayThongTinGiaBanSanPham")]
        public IActionResult CalculateTotalPrice([FromBody] List<ProductQuantity> products)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var prices = new List<object>();
            // Tổng giá của đơn hàng
            decimal totalPrice = 0;

            // Kiểm tra từng sản phẩm trong danh sách
            foreach (var product in products)
            {
                int productId = product.ProductId;
                int quantity = product.Quantity;

                // Lấy giá của sản phẩm từ CSDL hoặc từ bất kỳ nguồn dữ liệu nào
                decimal productPrice = GetProductPrice(productId);

                // Tính giá của từng sản phẩm (= giá * số lượng)
                decimal subtotalPrice = productPrice * quantity;

                // Thêm thông tin giá từng sản phẩm vào danh sách
                prices.Add(new
                {
                    ProductId = productId,
                    ProductPrice = productPrice,
                    SubtotalPrice = subtotalPrice
                });

                // Cộng giá của từng sản phẩm để tính tổng giá của đơn hàng
                totalPrice += subtotalPrice;
            }

            // Trả về tổng giá của đơn hàng
            return Ok(new { SubPrices = prices, TotalPrice = totalPrice });
        }

        [HttpPost("LayMaGiamGia")]
        public IActionResult ValidatePromotion([FromBody] PromotionCheck promotionCheck)
        {
            var now = DateTime.Now;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string promotionId = promotionCheck.PROMOTION_ID;

                // Kiểm tra từng sản phẩm trong danh sách
                foreach (var product in promotionCheck.Products)
                {
                    int productId = product.ProductId;
                    int quantity = product.Quantity;

                    string query = @"
                    SELECT COUNT(*) 
                    FROM PROMOTION p
                    INNER JOIN PRODUCT pr ON p.PRODUCT_ID = pr.ID
                    WHERE 
                        p.ID = @PromotionId AND
                        pr.ID = @ProductId AND
                        p.BEGIN_DATE <= @Now AND
                        p.END_DATE >= @Now AND
                        p.STATUS = 'SUCCESS';";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PromotionId", promotionId);
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@Now", now);

                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            // Nếu có Product_Id thuộc mã giảm giá Promotion_Id, thực hiện truy vấn thông tin Promotion
                            string promoInfoQuery = @"
                        SELECT 
                            p.PROMOTION_NAME,
                            pr.PRODUCT_NAME,
                            p.PROMOTIONAL_PRICE,
                            p.DESCRIPTION
                        FROM 
                            PROMOTION p
                        INNER JOIN 
                            PRODUCT pr ON p.PRODUCT_ID = pr.ID
                        WHERE 
                            p.ID = @PromotionId AND
                            pr.ID = @ProductId AND
                            p.BEGIN_DATE <= @Now AND
                            p.END_DATE >= @Now AND
                            p.STATUS = 'SUCCESS';";

                            using (SqlCommand promoCommand = new SqlCommand(promoInfoQuery, connection))
                            {
                                promoCommand.Parameters.AddWithValue("@PromotionId", promotionId);
                                promoCommand.Parameters.AddWithValue("@ProductId", productId);
                                promoCommand.Parameters.AddWithValue("@Now", now);

                                using (SqlDataReader reader = promoCommand.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        var promotionInfo = new
                                        {
                                            PromotionName = reader.GetString(0),
                                            ProductName = reader.GetString(1),
                                            PromotionalPrice = reader.GetInt32(2),
                                            Description = reader.GetString(3)
                                        };

                                        return Ok(promotionInfo);
                                    }
                                }
                            }
                        }
                    }
                }

                return NotFound("Không có sản phẩm nào thuộc mã giảm giá hoặc mã giảm giá không hợp lệ.");
            }
        }

        [HttpGet("LoadThongTinKhachHang")]
        public IActionResult LoadThongTinKhachHang(int customer_Id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT CUSTOMER_NAME, PHONE, ADDRESS 
                FROM CUSTOMERS 
                WHERE ID = @Customer_Id;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Customer_Id", customer_Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var customerInfo = new
                            {
                                CustomerName = reader.GetString(0),
                                Phone = reader.GetString(1),
                                Address = reader.GetString(2),
                            };

                            return Ok(customerInfo);
                        }
                        else
                        {
                            return NotFound("Không tìm thấy thông tin khách hàng.");
                        }
                    }
                }
            }
        }

    }



}
