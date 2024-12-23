namespace Exam.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public string Title {  get; set; }
        public string Author { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
