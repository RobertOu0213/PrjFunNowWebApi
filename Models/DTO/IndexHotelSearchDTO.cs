namespace PrjFunNowWebApi.Models.DTO
{
    public class IndexHotelSearchDTO
    {
        public string? keyword { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? adults { get; set; } = 0;
        public int? children { get; set; } = 0;
        public int roomnum { get; set; }  //客戶訂房數
        public string? sortBy { get; set; }
        public string? sortType { get; set; } = "asc";
        public int? lowerPrice { get; set; } = 1;
        public int? upperPrice { get; set; } = 50000;

    }
}
