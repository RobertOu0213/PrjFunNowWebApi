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
        //你可能喜歡
        public int CityId { get; set; } // 新增 CityId 属性
        public List<pgHotel_SimilarHotelsDTO>? SimilarHotels { get; set; } // 新增 SimilarHotels 属性
    }
}
