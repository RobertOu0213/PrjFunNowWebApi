namespace PrjFunNowWebApi.Models.DTO
{
    public class cartItemsDTO
    {
        public string HotelName { get; set; }
        public string RoomType { get; set; }
        public string RoomName { get; set; }

        public decimal RoomPrice { get; set; }
        public string CityName { get; set; }
        public int AllCommentsCount { get; set; }
        public int LevelStar { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int RoomID { get; set; }
        public int MaximumOccupancy { get; set; }
        public int AllOrderDetailsCount { get; set; }

        public string RoomImage { get; set; }

        public int OrderDetailID { get; set; }
        public int HotelID { get; set; }
        
        public int? GuestNumber { get; set; }
        public bool? IsExpired { get; set; }
    }
}
