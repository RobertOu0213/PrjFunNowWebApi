namespace PrjFunNowWebApi.Models.DTO
{
    public class HotelSearchBoxDTO
    {
        public int HotelId { get; set; }

        public string HotelName { get; set; }

        public string HotelAddress { get; set; }

        public string HotelPhone { get; set; }

        public string HotelDescription { get; set; }

        public int? LevelStar { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public bool? IsActive { get; set; }

        public int MemberId { get; set; }

        public int CityId { get; set; }

        public string CityName { get; set; }

        public string CommentTitle { get; set; }

        public string CommentText { get; set; }

        public string CountryName { get; set; }

        public int HotelEquipmentId { get; set; }

        public List<string> HotelEquipmentNames { get; set; }  // 修改為 List<string>

        public List<string> HotelImages { get; set; } // 新增這行

        public int HotelTypeId { get; set; }

        public string HotelTypeName { get; set; }

        public int RoomEquipmentId { get; set; }

        public List<string> RoomEquipmentNames { get; set; }  // 修改為 List<string>

        public decimal? HotelPrice { get; set; }

        public int? HotelMaximumOccupancy { get; set; }

        public decimal? TotalAverageScore { get; set; }

        public int AvailableRooms { get; set; }  // 新增這行 空房間數

        public int HotelOrderCount { get; set; }  // 旅館已獲取訂單數




        // 解析設備名稱和房間設備名稱
        public void ParseEquipmentNames(string hotelEquipmentNames, string roomEquipmentNames)
        {
            HotelEquipmentNames = hotelEquipmentNames?.Split(new[] { ", " }, StringSplitOptions.None).ToList() ?? new List<string>();
            RoomEquipmentNames = roomEquipmentNames?.Split(new[] { ", " }, StringSplitOptions.None).ToList() ?? new List<string>();
        }
    }
}
