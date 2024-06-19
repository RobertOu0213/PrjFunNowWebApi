namespace PrjFunNowWebApi.Models.louie_dto
{
    public class pgHotel_RoomDTO
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public decimal RoomPrice { get; set; }
        public int MaximumOccupancy { get; set; }
        public int MemberID { get; set; }
        public List<string>? RoomEquipments { get; set; }
        public List<pgHotel_ImageDTO>? RoomImages { get; set; }
        public bool IsBooked { get; set; }
    }
}
