using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WintonHitCardAuto.Services
{
    public class LineNotifyService
    {
        private readonly string APIstring;//APIURL
        private readonly string TokenID;//取得token的key  
        private readonly APIService _apiser;
        private readonly bool _isLineNotify;
        public LineNotifyService(IConfiguration config, APIService APIser)
        {
            APIstring = "https://notify-api.line.me/api/notify";
            TokenID = config.GetValue<string>("LineNotify:Token");
            _isLineNotify = config.GetValue<bool>("LineNotify:isNotify");
            _apiser = APIser;
        }

        #region 發送LINENOTIFY訊息
        /// <summary>
        ///  發送LINENOTIFY訊息
        /// </summary>
        /// <param name="Message">要發送的訊息</param>
        /// <param name="NowToken">不使用appsetting設定的token時要改帶這個</param>
        /// <param name="ProjectName">專案名稱，沒傳則用appsettings</param>
        /// <param name="ClassName">類別名稱</param>
        /// <param name="FunctionName"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public async Task<JObject> LineNotify_Send(string Message)
        {

            JObject PostResult = new JObject();

            if (!_isLineNotify)
                return PostResult;

            try
            {
                    string Parameter = $"Message={Message}";
                    Dictionary<string, object> header= new Dictionary<string, object>();
                    header.Add("Authorization",$"Bearer {TokenID}");
                    //Console.WriteLine($"Parameter：{Parameter}");
                    //呼叫傳送訊息API
                    await _apiser.Post_async(APIstring, Parameter, header);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"送Line失敗ex：{ex}");
            }

            return PostResult;
        }
        #endregion
    }
}
