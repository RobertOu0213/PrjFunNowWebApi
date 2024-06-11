namespace PrjFunNowWebApi.Models
{
    using System.Collections.Generic;

    public class HotelService : IHotelService
    {
        private readonly FunNowContext _context; // 请将 YourDbContext 替换为您的实际 DbContext 类型

        public HotelService(FunNowContext context)
        {
            _context = context;
        }

        public List<HotelSearchBox> GetHotels()
        {
            // 在这里编写获取酒店数据的逻辑，可以直接调用数据库上下文来查询数据
            var hotels = _context.Hotels.ToList(); // 示例代码，请根据您的实际情况进行修改

            // 将 hotels 转换为 HotelSearchBox 对象并返回
            var hotelSearchBoxes = hotels.Select(h => new HotelSearchBox
            {
                // 这里填写转换逻辑
            }).ToList();

            return hotelSearchBoxes;
        }
    }
}
