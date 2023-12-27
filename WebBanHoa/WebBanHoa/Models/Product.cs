namespace WebBanHoa.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string PRODUCT_NAME { get; set; }
        public int PRICE { get; set; }
        public string IMAGE { get; set; }
        public string DESCRIPTION { get; set; }
        public int FLOWER_CATE_ID { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public string STATUS { get; set; }
    }
}
