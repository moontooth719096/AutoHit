using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HitCardCheck.Services
{
    public class APIService
    {
        public readonly IHttpClientFactory _clientF;
        public APIService(IHttpClientFactory ClientF)
        {
            _clientF = ClientF;
        }
        #region Post
        public async Task<HttpResponseMessage> Post_async(string Url, string Pramater = null, Dictionary<string,object> HeaderList = null, int Timeout = 90)
        {
            HttpResponseMessage response = null;
            try
            {
                var _httpClient = _clientF.CreateClient();

                if (HeaderList != null && HeaderList.Count > 0)
                {
                    foreach (var data in HeaderList)
                    {
                        _httpClient.DefaultRequestHeaders.Add(data.Key, data.Value.ToString());
                    }
                }

                StringContent httpContent = new StringContent(Pramater, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                //確保不會0秒，0秒就會一直等
                if (Timeout == 0)
                    Timeout = 90;
                _httpClient.Timeout = TimeSpan.FromSeconds(Timeout);
                response = await _httpClient.PostAsync(Url, httpContent);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }
        #endregion
    }
}
