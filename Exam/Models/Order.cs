namespace Exam.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } 
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
