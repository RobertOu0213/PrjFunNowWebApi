namespace PrjFunNowWebApi.Models.louie_dto
{
    public class SearchParametersDTO
    {
        public string Keyword { get; set; } = "";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
