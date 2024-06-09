using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Models.louie_dto
{
    public class pgHotel_HotelDetailDTO
    {
        public string? HotelName { get; set; }
        public string? HotelAddress { get; set; }
        public string? HotelDescription { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public int? LevelStar { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
        public int MemberID { get; set; }
        public List<string>? HotelEquipments { get; set; }
        public List<pgHotel_ImageDTO>? HotelImages { get; set; }
        public List<pgHotel_RoomDTO>? Rooms { get; set; }
        public decimal AverageRoomPrice { get; set; }
    }
}
