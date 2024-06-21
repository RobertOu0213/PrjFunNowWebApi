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
        public string HotelAddress { get; set; }     // 新增
        public string HotelPhone { get; set; }       // 新增
        public string HotelTypeName { get; set; }    // 新增
        public List<string> HotelImages { get; set; }
        public string RoomTypeName { get; set; }     // 新增
        public List<string> RoomImages { get; set; } // 新增
        public List<string> RoomEquipmentNames { get; set; } // 新增

    }
}
