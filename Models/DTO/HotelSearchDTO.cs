namespace PrjFunNowWebApi.Models.DTO
{
    public class HotelSearchDTO
    {
        public string? keyword { get; set; }
        public int? HotelId { get; set; } 

        public int? MemberId { get; set; } 

        //public string? sortBy { get; set; }
        //public string? sortType { get; set; } 
        //public int? page { get; set; } 
        //public int? pageSize { get; set; } 
    }
}
