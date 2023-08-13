namespace FinalProject.Dtos
{
    public class JwtDto
    {
        internal int UserId { get; set; }
        public string PhoneNumber { get; set; }
        //only for test
        public string access_token { get; set; };
    }
}
