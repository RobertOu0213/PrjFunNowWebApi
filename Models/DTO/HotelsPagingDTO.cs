namespace PrjFunNowWebApi.Models.DTO
{
    public class HotelsPagingDTO
    {
        //public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public List<HotelSearchBox>? HotelsResult { get; set; }

    }
}
