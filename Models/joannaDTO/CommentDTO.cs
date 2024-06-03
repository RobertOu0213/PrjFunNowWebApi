namespace PrjFunNowWebApi.Models.joannaDTO
{
    public class CommentDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; }
        public int RatingFilter { get; set; }
        public string DateFilter { get; set; }

    }
}
