using PrjFunNowWebApi.Models.louie_dto;

namespace PrjFunNowWebApi.Models.louie_class
{
    public class CPaging<T>
    {
        public int TotalRecords { get; set; }//總筆數
        public int PageNumber { get; set; }//第x頁
        public int PageSize { get; set; }//一頁y筆
        public List<T> Data { get; set; }//頁面呈現的資料

        public CPaging(List<T> data, int totalRecords, int pageNumber, int pageSize)
        {
            Data = data;
            TotalRecords = totalRecords;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
