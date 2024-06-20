namespace PrjFunNowWebApi.Models.DTO
{
    public class DialogWithHotelInfoDto
    {
        public int DialogId { get; set; }
        public int MemberId { get; set; }
        public string Detail { get; set; }
        public int CalltoMemberId { get; set; }
        public DateTime CreateAt { get; set; }
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string HotelImage { get; set; }
    }
}
