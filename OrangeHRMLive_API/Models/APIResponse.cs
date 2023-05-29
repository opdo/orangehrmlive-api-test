namespace OrangeHRMLive_API.Models
{
    public class APIResponse
    {
        public APIError error { get; set; }
        public dynamic data { get; set; }
    }

    public class APIError
    {
        public int status { get; set; }
        public string message { get; set; }
    }
}
