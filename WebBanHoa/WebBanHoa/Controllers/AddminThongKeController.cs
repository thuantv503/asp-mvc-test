using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace WebBanHoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddminThongKeController : Controller
    {
        private readonly IConfiguration _configuration;

        public AddminThongKeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("ThongKe")]
        public IActionResult GetSalesReport()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            List<SalesInfo> salesInfoList = new List<SalesInfo>();

            decimal totalQuantity = 0;
            decimal totalCost = 0;
            decimal totalRevenue = 0;
            decimal totalSales = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT P.ID, P.PRODUCT_NAME, P.PRICE, P.COST, OD.QUANTITY
                    FROM PRODUCT P
                    INNER JOIN ORDER_DETAILS OD ON P.ID = OD.PRODUCT_ID;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int productId = Convert.ToInt32(reader["ID"]);
                            string productName = reader["PRODUCT_NAME"].ToString();
                            decimal price = Convert.ToDecimal(reader["PRICE"]);
                            decimal cost = Convert.ToDecimal(reader["COST"]);
                            int quantity = Convert.ToInt32(reader["QUANTITY"]);

                            decimal totalProductCost = cost * quantity;
                            decimal totalProductSales = price * quantity;

                            SalesInfo salesInfo = new SalesInfo
                            {
                                ProductId = productId,
                                ProductName = productName,
                                Price = price,
                                Cost = cost,
                                QuantitySold = quantity,
                                TotalProductCost = totalProductCost,
                                TotalProductSales = totalProductSales
                            };

                            salesInfoList.Add(salesInfo);

                            totalQuantity += quantity;
                            totalCost += totalProductCost;
                            totalSales += totalProductSales;
                        }
                    }
                }
            }

            totalRevenue = totalSales - totalCost;

            SalesSummary summary = new SalesSummary
            {
                TotalQuantity = totalQuantity,
                TotalCost = totalCost,
                TotalRevenue = totalRevenue,
                TotalSales = totalSales
            };

            return Ok(new { SalesInfoList = salesInfoList, Summary = summary });
        }

    }
    public class SalesInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalProductCost { get; set; }
        public decimal TotalProductSales { get; set; }
    }

    public class SalesSummary
    {
        public decimal TotalQuantity { get; set; } 
        public decimal TotalSales { get; set; } 
        public decimal TotalCost { get; set; } 
        public decimal TotalRevenue { get; set; } 
        public List<SalesInfo> ProductSalesInfo { get; set; }
    }


}
