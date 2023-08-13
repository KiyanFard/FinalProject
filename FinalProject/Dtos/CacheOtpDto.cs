namespace FinalProject.Dtos
{
    internal class CacheOtpDto
    {
        public object OtpCode { get; set; }
        public string NatoinalCode { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int FailedCount { get; set; }
    }
}