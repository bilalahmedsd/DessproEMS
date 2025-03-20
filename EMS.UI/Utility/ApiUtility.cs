

using EMS.Core.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EMS.UI.Utilities
{
    public static class ApiUtility
    {
        public static string BaseUrl { get; set; }
        public static async Task<ResponseModel> PostApi(string Route, string JsonBody, string BearerToken = "")
        {
            ResponseModel resp = new ResponseModel();
            string JsonStr = "";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    if (!string.IsNullOrEmpty(BearerToken))
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                    }

                    HttpContent content = new StringContent(JsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(Route, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JsonStr = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        JsonStr =  "";
                        resp.IsSuccess = false;
                      
                    }
                }
                resp = !string.IsNullOrEmpty(JsonStr) ? JsonConvert.DeserializeObject<ResponseModel>(JsonStr) : resp;
                resp.Data = JsonConvert.SerializeObject(resp.Data);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resp;
        }
        public static async Task<ResponseModel> GetApi(string Route, string BearerToken = "")
        {
            ResponseModel resp = new ResponseModel();
            string JsonStr = "";
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                if (!string.IsNullOrEmpty(BearerToken))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                }

                HttpResponseMessage response =  client.GetAsync(Route).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonStr = await  response.Content.ReadAsStringAsync();
                }
                else
                {
                    JsonStr =  await response.Content.ReadAsStringAsync();
                }
            }

            resp = !string.IsNullOrEmpty(JsonStr) ? JsonConvert.DeserializeObject<ResponseModel>(JsonStr) : resp;
            resp.Data = JsonConvert.SerializeObject(resp.Data);

            return resp;
        }
        public static async Task<ResponseModel> PostFormCollectionApi(string Route, IFormCollection form,string key , string BearerToken = "")
        {
            ResponseModel resp = new ResponseModel();
            string JsonStr = "";
            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(BearerToken))
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                    }

                    using (var formData = new MultipartFormDataContent())
                    {
                        Dictionary<string, string> jsondata = new Dictionary<string, string>();
                        foreach (var val in form.AsEnumerable().ToList())
                        {
                            jsondata.Add(val.Key, val.Value);
                        }
                        HttpContent stringContent = new StringContent(JsonConvert.SerializeObject(jsondata));
                        formData.Add(stringContent, key);
                        if (form.Files.Count > 0)
                        {
                            var ms = new MemoryStream();
                            form.Files[0].CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            HttpContent bytesContent = new ByteArrayContent(fileBytes);
                            formData.Add(bytesContent, "File", form.Files[0].FileName);
                        }
                        var response =  client.PostAsync(new Uri(BaseUrl) + Route, formData).Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            JsonStr =  await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            JsonStr =  await response.Content.ReadAsStringAsync();
                        }
                    }

                    resp = !string.IsNullOrEmpty(JsonStr) ? JsonConvert.DeserializeObject<ResponseModel>(JsonStr) : resp;
                    resp.Data = JsonConvert.SerializeObject(resp.Data);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resp;
        }
        public static byte[] GetReportApi(string Route, string BearerToken = "")
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(30);
                //client.BaseAddress = new Uri(ReportUrl);
                if (!string.IsNullOrEmpty(BearerToken))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                }
                return client.GetByteArrayAsync(BaseUrl + Route).Result;
            }
        }
    }
}
