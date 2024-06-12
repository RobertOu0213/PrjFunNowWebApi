namespace PrjFunNowWebApi.Models.louie_dto
{
    public class pgBackMemberDTO
    {
        public int MemberId { get; set; }

        public string? FullName { get; set; }//把First Name跟Last Name結合

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? RoleName { get; set; }//對照另一張表, 把Role Id轉成對應的Role Name
    }
}
