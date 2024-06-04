namespace PrjFunNowWebApi.Models.DTO
{
    public class HotelSearchDTO
    {
        public string? keyword { get; set; }
        public int? HotelId { get; set; } = 0;

        public int? MemberId { get; set; } = 0;

        public string? sortBy { get; set; }
        public string? sortType { get; set; } = "asc";
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 9;
    }
}
