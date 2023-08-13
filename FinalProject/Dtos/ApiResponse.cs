namespace FinalProject.Dtos
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        
    }
    public class ApiResponse<T>
    {
        public string Data { get; set; }
        public int StatusCode { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
