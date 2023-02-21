using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Snowflake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WintonHitCardAuto.Models;

namespace WintonHitCardAuto.Services
{
    public class HItCardService : IHostedService
    {
        private readonly LineNotifyService _line;
        private readonly AccountInfosetting _accountInfo;
        private readonly ClickPathSetting _clicksetting;
        private readonly int _unworktime;
        private readonly string[] StatusAllow = { "待簽", "核准" };
        public HItCardService(IConfiguration config, LineNotifyService line)//, AccountInfosetting accountInfo , ClickPathSetting clicksetting
        {
            _line = line;
            _accountInfo = new AccountInfosetting();
            var se1t = config.GetSection("AccountInfosetting");
            se1t.Bind(_accountInfo);
            
            _clicksetting = new ClickPathSetting();
            var clickse1t = config.GetSection("ClickPath");
            clickse1t.Bind(_clicksetting);

            Int32.TryParse(config["UnWorkTime"], out _unworktime);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime? SearchSDate = null;
            DateTime? SearchEDate = null;

            while (SearchSDate == null)
            {
                Console.WriteLine("請輸入起始日期");
                string input = Console.ReadLine();
                if (DateTime.TryParse(input, out DateTime SearchTSDate))
                    SearchSDate = SearchTSDate;
            }

            while (SearchEDate == null)
            {
                Console.WriteLine("請輸入結束日期");
                string input = Console.ReadLine();
                if (DateTime.TryParse(input, out DateTime SearchTEDate))
                    SearchEDate = SearchTEDate;
            }

            var service = ChromeDriverService.CreateDefaultService(@".", "chromedriver.exe");
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            options.AddArgument("--headless");

            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options))
            {
                Login_Do(driver);

                //查詢刷卡紀錄頁籤
                HitCardLogSearch(driver, _accountInfo,(DateTime)SearchSDate,(DateTime)SearchEDate);
            }
            return Task.CompletedTask;
        }

        private void Login_Do(IWebDriver driver)
        {

            driver.Navigate().GoToUrl(_clicksetting.Url);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);
            Login(driver, _accountInfo);

            
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();   
        }

        private void Login(IWebDriver driver, AccountInfosetting acinfo)
        {
            driver.FindElement(By.XPath(_clicksetting.Login.Account)).SendKeys(acinfo.Account);//填入帳號
            driver.FindElement(By.XPath(_clicksetting.Login.Password)).SendKeys(acinfo.Password);//談入密碼
            driver.FindElement(By.XPath(_clicksetting.Login.LoginButton)).Click();///點擊登入按鈕
        }

        private void HitCardLogSearch(IWebDriver driver, AccountInfosetting acinfo,DateTime StartTime,DateTime EndTime)
        {
            string searchdate = DateTime.Today.ToString("yyyy/MM/dd");
            driver.FindElement(By.XPath(_clicksetting.HitCardLog_Button)).Click();//點擊打卡紀錄頁籤按鈕
            driver.FindElement(By.XPath(_clicksetting.SignSearch_Tab)).Click();//點擊籤和查詢tab
            driver.FindElement(By.XPath(_clicksetting.applicant_Input)).Clear();//清空申請人欄位
            driver.FindElement(By.XPath(_clicksetting.applicant_Input)).SendKeys(_clicksetting.applicantName);//輸入申請人
            driver.FindElement(By.XPath(_clicksetting.Search_Btn)).Click();//點擊查詢
            IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.SignLogData));//拉取打卡紀錄
            

            List<SignData> SignDatas = null;
            if (lstTrElem.Count > 0)
            {
                foreach (var elemTd in lstTrElem)
                {
                    List<IWebElement> lstTrElem1 = new List<IWebElement>(elemTd.FindElements(By.TagName("tr")));

                    if (lstTrElem1.Count > 0)
                    {
                        if (SignDatas == null)
                            SignDatas = new List<SignData>();

                        foreach (var elemtd in lstTrElem1)
                        {
                            List<IWebElement> lstTdElem1 = new List<IWebElement>(elemtd.FindElements(By.TagName("td")));
                            string N_Status = lstTdElem1[0].Text;
                            if (Array.IndexOf(StatusAllow, N_Status) <=-1)
                                continue;
                            DateTime? N_StartDate = null;
                            if (DateTime.TryParse(lstTdElem1[4].Text, out DateTime P_Tiem))
                                N_StartDate = P_Tiem;
                            else
                                continue;
                            DateTime? N_EndtDate = null;
                            if (DateTime.TryParse(lstTdElem1[6].Text, out DateTime P_ETiem))
                                N_EndtDate = P_ETiem;
                            else
                                continue;
                            SignData data = new SignData
                            {
                                Status = N_Status,
                                StartDate = (DateTime)N_StartDate,
                                EndDate = (DateTime)N_EndtDate,
                                Time = $"{ lstTdElem1[5].Text}-{lstTdElem1[7].Text}",
                                NowProccess = lstTdElem1[14].Text
                            };
                            if(data.StartDate >= StartTime && data.StartDate<= EndTime)
                                SignDatas.Add(data);
                        }
                    }
                }
            }
            if (SignDatas != null && SignDatas.Count > 0)
            {
                SignDatas = SignDatas.OrderBy(x => x.StartDate).ToList();
                for (int i = 0; i < SignDatas.Count; i++)
                {
                    SignData thisdata = SignDatas[i];
                    Console.WriteLine($"{thisdata.Status} {thisdata.StartDate.ToString("yyyy/MM/dd")}-{thisdata.EndDate.ToString("yyyy/MM/dd")} {thisdata.Time} 目前關卡：{thisdata.NowProccess}");
                }
            }

            //產生EXCEL

        }

        private void HitCheck(IWebDriver driver, bool isWorkTime)
        {

            string target = isWorkTime ? "上班" : "下班";
            try
            {
                //IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.ChickHit));//拉取打卡紀錄
                //IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.ChickHit));//拉取打卡紀錄
                 //string re =  lstTrElem[0].Text;
            }
            catch (Exception EX)
            { 
            
            }

            //HitCardInfo data = null;
            //try
            //{
            //    //var HitRecordDatas = lstTrElem.Select(x =>
            //    //{
            //    //    string texts = x.Text;
            //    //    string[] spl = texts.Split("\r\n");
            //    //    return new HitCardInfo { Hittime = DateTime.Parse(spl[0]), Content = spl[1] };

            //    //});

            //    List<HitCardInfo> aaa = lstTrElem.Select(x =>
            //     {
            //         HitCardInfo re = new HitCardInfo();
            //         string texts = x.Text;
            //         string[] spl = texts.Split("\r\n");
            //         re = new HitCardInfo { Hittime = DateTime.Parse(spl[0]), Content = spl[1] };
            //         return re;
            //     }).Where(z => z.Content.Trim() == target).ToList();

            //    if (aaa != null && aaa.Count() > 0)
            //        data = aaa.OrderBy(y => y.Hittime).FirstOrDefault();
            //}
            //catch (Exception ex)
            //{
            //    string message = $"HitCheck發生例外：{ex}";
            //    Console.WriteLine(message);
            //}
            //if (data != null)
            //{
            //    _line.LineNotify_Send($"\n最早已於 {data.Hittime.ToString("yyyy/MM/dd HH:mm:ss")} 打了 {data.Content} 卡");
            //}
        }

     
    }
}
