using System.Text.Json.Nodes;

namespace EMS.UI.Services
{
    public class ReCaptchaService
    {
        private readonly HttpClient _httpClient;
        public ReCaptchaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static async Task<bool> VerifyCaptchaV2(string response, string secret)
        {
            using (var client = new HttpClient())
            {
                string url = "https://www.google.com/recaptcha/api/siteverify";
                MultipartFormDataContent content = new();
                content.Add(new StringContent(response), "response");
                content.Add(new StringContent(secret), "secret");

                var result = await client.PostAsync(url, content);


                if (result.IsSuccessStatusCode)
                {
                    var strResponse = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(strResponse);

                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        var success = ((bool?)jsonResponse["success"]);
                        if (success != null && success == true) return true;
                    }
                }
            }
            return false;
        }
    }
}
