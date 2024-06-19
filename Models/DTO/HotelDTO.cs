namespace PrjFunNowWebApi.Models.DTO
{
    public class HotelDTO
    {
        public string CityId { get; set; }
        public string HotelAddress { get; set; }
        public string HotelDescription { get; set; }
        public List<int> HotelEquipmentID { get; set; }
        public List<HotelImageDTO> HotelImage { get; set; }
        public string HotelName { get; set; }
        public string HotelPhone { get; set; }
        public string LevelStar { get; set; }
        public string TypeID { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }


    }
}
