namespace PrjFunNowWebApi.Models.DTO
{
    public class RoomDTO
    {
        public string Description { get; set; }
        public string MaximumOccupancy { get; set; }
        public List<int> RoomEquipmentID { get; set; }
        public List<RoomImageDTO> RoomImages { get; set; }
        public string RoomName { get; set; }
        public string RoomPrice { get; set; }
        public string RoomSize { get; set; }
        public string RoomTypeID { get; set; }
    }
}
