﻿namespace PrjFunNowWebApi.Models.DTO
{
    public class IndexHotelSearchDTO
    {
        public string? keyword { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? adults { get; set; } = 0;
        public int? children { get; set; } = 0;
        public int? totalpeople
        {
            get
            {
                return (adults ?? 0) + (children ?? 0);
            }
        }

    }
}
