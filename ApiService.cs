using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; }
        public string Error { get; set; }

        // Turns the JSON body into a C# object
        public T Read<T>()
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            return JsonSerializer.Deserialize<T>(Body, options);
        }
    }

    // All calls to the RaceDay API go through this class
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _accessor;

        public ApiService(HttpClient client, IHttpContextAccessor accessor)
        {
            _client = client;
            _accessor = accessor;
        }

        public ApiResult Get(string url)
        {
            return Send(HttpMethod.Get, url, null);
        }

        public ApiResult Post(string url, object body)
        {
            return Send(HttpMethod.Post, url, ToJson(body));
        }

        public ApiResult Put(string url, object body)
        {
            return Send(HttpMethod.Put, url, ToJson(body));
        }

        public ApiResult Delete(string url)
        {
            return Send(HttpMethod.Delete, url, null);
        }

        public ApiResult PostFile(string url, IFormFile file)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);
            return Send(HttpMethod.Post, url, content);
        }

        private StringContent ToJson(object body)
        {
            return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        private ApiResult Send(HttpMethod method, string url, HttpContent content)
        {
            var result = new ApiResult();

            try
            {
                var request = new HttpRequestMessage(method, url);
                request.Content = content;

                // Send the logged-in user's token with every request
                string token = _accessor.HttpContext.Session.GetString("Token");
                if (token != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = _client.Send(request);
                result.StatusCode = (int)response.StatusCode;
                result.Success = response.IsSuccessStatusCode;
                result.Body = response.Content.ReadAsStringAsync().Result;

                if (!result.Success)
                {
                    result.Error = FriendlyError(result.StatusCode, result.Body);
                }
            }
            catch (Exception)
            {
                result.Success = false;
                result.Error = "Cannot reach the server. Please try again later.";
            }

            return result;
        }

        // Never show raw error text to the user
        private string FriendlyError(int status, string body)
        {
            if (status == 401) return "Please log in again.";
            if (status == 403) return "You are not allowed to do that.";

            if (status == 400 || status == 404 || status == 409)
            {
                string text = (body ?? "").Trim().Trim('"');
                if (text.Length > 0 && text.Length < 200 && !text.StartsWith("{"))
                {
                    return text;
                }
                return "Please check the information you entered.";
            }

            return "Something went wrong. Please try again.";
        }
    }
}
