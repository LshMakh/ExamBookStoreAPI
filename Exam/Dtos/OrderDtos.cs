namespace Exam.Dtos
{
    public class CartItem
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderRequest
    {
        public string CustomerName { get; set; }
        public List<CartItem> Items { get; set; }
    }

}
