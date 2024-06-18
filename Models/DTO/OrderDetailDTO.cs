namespace PrjFunNowWebApi.Models.DTO
{
    public class OrderDetailDTO
    {
        public int OrderDetailId { get; set; }
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int HotelID { get; set; }
        public string HotelName { get; set; }
        public List<string> HotelImages { get; set; }  // 更新這行

    }
}
