namespace PrjFunNowWebApi.Models
{
    public class MemberInfo
    {
        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public DateTime? Birthday { get; set; }

        public int RoleId { get; set; }

        public string Image { get; set; }

        public string LastName { get; set; }

    }
}
