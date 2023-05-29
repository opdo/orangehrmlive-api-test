using Newtonsoft.Json;
using OrangeHRMLive_API.Models;
using RestSharp;
using System.Net;
using System.Text.Json.Serialization;

namespace OrangeHRMLive_API.Helpers
{
    public class APIHelper
    {
        private RestClient restClient;
        private Cookie sessionCookie;

        /// <summary>
        /// Contructor khởi tạo
        /// </summary>
        /// <param name="host"></param>
        public APIHelper(string host)
        {
            // Khởi tạo theo host, host có thể đọc từ config
            // Vì khi test thì có thể phát sinh nhiều môi trường (QA, UAT, Prod,...), mỗi môi trường có 1 url khác nhau
            // lúc này nếu hard-code sẽ rất dở
            restClient = new RestClient(new RestClientOptions(host)
            {
                // Đây là web SPA vì vậy nó sẽ có cơ chế tự redirect, mình đang KHÔNG có document api offical nên
                // mò mẫn bằng postman và phát hiện ra có có cơ chế này, để dùng thuận tiện mình phải tắt nó
                FollowRedirects = false,
            });

            // Khởi tạo tạm giá trị
            sessionCookie = new Cookie();
        }

        /// <summary>
        /// CSRF token là 1 cơ chế chống spam của người ta
        /// Do ko có offical document nên mình phải tự mò
        /// Đọc thêm về CSRF token cho biết ở google
        /// </summary>
        /// <returns></returns>
        public string GetCSRFToken()
        {
            // Access vào trang login
            var request = new RestRequest("/web/index.php/auth/login", Method.Get);
            var response = restClient.Execute(request);
            sessionCookie = response.Cookies.FirstOrDefault(x => x.Name == "orangehrm");


            // Lấy html của nó
            var html = response.Content;

            // Dùng biểu thức chính quy (regex) để tách chuỗi
            var token = html.GetStringByRegex(":token=\"(.*?)\"").Replace("&quot;", string.Empty);

            // Báo lỗi nếu như ko lấy đc token
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token is not found");
            }

            return token;
        }

        /// <summary>
        /// Login và store lại cookie
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Login(string username, string password)
        {
            // Login thì method post
            var csrfToken = GetCSRFToken();
            var request = new RestRequest("/web/index.php/auth/validate", Method.Post);
            request.AddCookie(sessionCookie.Name, sessionCookie.Value, sessionCookie.Path, sessionCookie.Domain);

            var data = new LoginRequest
            {
                _token = csrfToken,
                username = username,
                password = password,
            };
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(data, ContentType.Json);

            // Execute api
            var response = restClient.Execute(request);

            // Web sẽ trả về status code 302 và redirect về 1 trang index, sau đó khởi tạo 1 cookie mới
            // Lý do mình ko muốn restsharp redirect theo là vì mình muốn bắt cái cookie mới này
            sessionCookie = response.Cookies.FirstOrDefault(x => x.Name == "orangehrm");

            // Sau đó mình sẽ redirect theo method get, tại sao mình biết điều này
            // Vì mình theo dõi cách web hoạt động trên postman để giả lập lại trên restsharp
            request = new RestRequest("/web/index.php/dashboard/index", Method.Get);
            request.AddCookie(sessionCookie.Name, sessionCookie.Value, sessionCookie.Path, sessionCookie.Domain);

            // Thực ra việc request thêm lần 2 là ko quá cần thiết, mục đích chỉ là check xem login success hay ko thôi
            // Chứ lấy cookie là đủ rồi
            response = restClient.Execute(request);

            // Verify API Response Code
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception("API Response code is " + response.ResponseStatus);
            }

            // Verify xem login success chưa
            if (string.IsNullOrEmpty(response.Content) || !response.Content.Contains("oxd-layout"))
            {
                throw new Exception("Login failed");
            }
        }

        /// <summary>
        /// Add employee API
        /// </summary>
        /// <param name="data"></param>
        public EmployeeResponse AddEmployee(EmployeeRequest data)
        {
            // Khúc này thì restClient đã store đc cookie rồi
            var request = new RestRequest("/web/index.php/api/v2/pim/employees", Method.Post);
            request.AddCookie(sessionCookie.Name, sessionCookie.Value, sessionCookie.Path, sessionCookie.Domain);
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(data, ContentType.Json);

            var response = restClient.Execute<APIResponse>(request);

            if (response.Data is null)
            {
                throw new Exception("Response data is null");
            }
            else if (response.Data?.error != null)
            {
                throw new Exception("Execute Add Employee API Failed with Error Code: " + response.Data.error.status + " and Message: " + response.Data.error.message);
            }

            // Debug thử response.Data
            return JsonConvert.DeserializeObject<EmployeeResponse>(response.Data.data.ToString());
        }
    }
}
