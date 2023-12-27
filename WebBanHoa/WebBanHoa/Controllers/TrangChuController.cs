using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrangChuController : Controller
    {
        private readonly IConfiguration _configuration;

        public TrangChuController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("TimKiem", Name = "TimKiemAPI")]
        public IActionResult TimKiem([FromBody] string sanPhamCanTim)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Product WHERE PRODUCT_NAME LIKE @sanPhamCanTim";

                string searchKeyword = $"%{sanPhamCanTim}%";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sanPhamCanTim", searchKeyword);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<Product> productList = new List<Product>();

                        while (reader.Read())
                        {
                            // Đọc thông tin sản phẩm từ cơ sở dữ liệu và thêm vào danh sách kết quả
                            Product product = new Product
                            {
                                // Thay đổi tên các cột dữ liệu tương ứng với cơ sở dữ liệu của bạn
                                ID = Convert.ToInt32(reader["ID"]),
                                PRODUCT_NAME = reader["PRODUCT_NAME"].ToString(),
                                PRICE = Convert.ToInt32(reader["PRICE"]),
                                IMAGE = reader["IMAGE"].ToString(),
                                DESCRIPTION = reader["DESCRIPTION"].ToString(),
                                FLOWER_CATE_ID = Convert.ToInt32(reader["FLOWER_CATE_ID"])
                                //Thêm các thông tin sản phẩm khác nếu cần thiết
                            };

                            productList.Add(product);
                        }

                        // Kiểm tra danh sách kết quả trống hay không để trả về thông báo phù hợp
                        if (productList.Count > 0)
                        {
                            return Ok(productList);
                        }
                        else
                        {
                            return NotFound("Không tìm thấy sản phẩm phù hợp.");
                        }
                    }
                }
            }
        }

        [HttpGet("SanPhamDangGiamGia", Name = "SanPhamDangGiamGiaAPI")]
        public IActionResult GetDiscountedProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<PromotionProduct> discountedProducts = new List<PromotionProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT 
                        P.PRODUCT_NAME,
                        P.PRICE,
                        P.IMAGE,
                        PR.PROMOTIONAL_PRICE
                    FROM 
                        PRODUCT AS P
                    INNER JOIN 
                        PROMOTION AS PR ON P.ID = PR.PRODUCT_ID
                    WHERE 
                        PR.STATUS = 'SUCCESS' ";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PromotionProduct product = new PromotionProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),
                                PROMOTIONAL_PRICE = reader.GetInt32(reader.GetOrdinal("PROMOTIONAL_PRICE"))
                            };

                            discountedProducts.Add(product);
                        }
                    }
                }
            }
            return Ok(discountedProducts);
        }

        [HttpGet("SanPhamNoiBat", Name = "SanPhamNoiBatAPI")]
        public IActionResult GetBestSellingProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FeaturedProduct> bestSellingProducts = new List<FeaturedProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT TOP 10 
                P.PRODUCT_NAME,
                P.PRICE,
                P.IMAGE
            FROM 
                ORDER_DETAILS OD
            INNER JOIN 
                PRODUCT P ON OD.PRODUCT_ID = P.ID
            GROUP BY 
                P.PRODUCT_NAME, P.PRICE, P.IMAGE
            ORDER BY 
                SUM(OD.QUANTITY) DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeaturedProduct product = new FeaturedProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),

                            };

                            bestSellingProducts.Add(product);
                        }
                    }
                }
            }

            return Ok(bestSellingProducts);
        }

        [HttpGet("SanPhamMoi", Name = "SanPhamMoiAPI")]
        public IActionResult GetNewProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FeaturedProduct> bestSellingProducts = new List<FeaturedProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
             SELECT TOP 10 *
	            FROM PRODUCT
	            WHERE STATUS = 'SUCCESS'
	            ORDER BY CREATE_DATE DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeaturedProduct product = new FeaturedProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),

                            };

                            bestSellingProducts.Add(product);
                        }
                    }
                }
            }

            return Ok(bestSellingProducts);
        }

        [HttpGet("HoaSinhNhat", Name = "HoaSinhNhatAPI")]
        public IActionResult GetBirthDayProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FeaturedProduct> bestSellingProducts = new List<FeaturedProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
              SELECT P.*
                FROM PRODUCT P
                JOIN TOPIC T ON P.ID = T.PRODUCT_ID
                WHERE T.TOPIC_NAME = N'Hoa Sinh Nhật'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeaturedProduct product = new FeaturedProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),

                            };

                            bestSellingProducts.Add(product);
                        }
                    }
                }
            }

            return Ok(bestSellingProducts);
        }

        [HttpGet("HoaLanHoDiep", Name = "HoaLanHoDiepAPI")]
        public IActionResult GetLanHoDiepProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FeaturedProduct> bestSellingProducts = new List<FeaturedProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
              SELECT P.*
                FROM PRODUCT P
                INNER JOIN FLOWER_CATEGORISES FC ON P.FLOWER_CATE_ID = FC.ID
                WHERE FC.FLOWER_CATE_NAME = N'Lan Hồ Điệp'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeaturedProduct product = new FeaturedProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),

                            };

                            bestSellingProducts.Add(product);
                        }
                    }
                }
            }

            return Ok(bestSellingProducts);
        }

        [HttpGet("HoaTangLe", Name = "HoaTangLeAPI")]
        public IActionResult GetHoaTangLeProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FeaturedProduct> bestSellingProducts = new List<FeaturedProduct>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
              SELECT P.*
                FROM PRODUCT P
                JOIN TOPIC T ON P.ID = T.PRODUCT_ID
                WHERE T.TOPIC_NAME = N'Hoa Tang Lễ'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FeaturedProduct product = new FeaturedProduct
                            {
                                PRODUCT_NAME = reader.GetString(reader.GetOrdinal("PRODUCT_NAME")),
                                PRICE = reader.GetInt32(reader.GetOrdinal("PRICE")),
                                IMAGE = reader.GetString(reader.GetOrdinal("IMAGE")),

                            };

                            bestSellingProducts.Add(product);
                        }
                    }
                }
            }

            return Ok(bestSellingProducts);
        }
    }
}
