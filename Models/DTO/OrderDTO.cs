namespace PrjFunNowWebApi.Models.DTO
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string GuestLastName { get; set; }
        public string GuestFirstName { get; set; }
        public string GuestEmail { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; }
    }
}
