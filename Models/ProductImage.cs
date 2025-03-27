namespace WebBanHang.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public string Url { get; set; } = "~/images/default.jpg";

        public int ProductId { get; set; }

        // Mối quan hệ với Product
        public Product? Product { get; set; }
    }
}
