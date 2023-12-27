using static WebBanHoa.Controllers.XacNhanDonHangController;

namespace WebBanHoa.Models
{
    public class PromotionCheck
    {
        public string PROMOTION_ID { get; set; }

        public List<ProductQuantity> Products { get; set; }
    }
}
