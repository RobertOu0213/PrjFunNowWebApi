namespace PrjFunNowWebApi.Models.louie_dto
{
    public class pgHotel_SimilarHotelsDTO
    {
        public int HotelId { get; set; }
        public string? HotelName { get; set; }
        public string? HotelAddress { get; set; }
        public int LevelStar { get; set; }
        public decimal AverageRoomPrice { get; set; }
        public int AvailableRooms { get; set; }
    }
}
