namespace PrjFunNowWebApi.Models.DTO
{
    public class OrderDailyDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? GuestNumber { get; set; }
        public string RoomName { get; set; }
        public decimal RoomPrice { get; set; }
    }
}
