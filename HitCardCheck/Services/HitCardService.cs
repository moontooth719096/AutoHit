using HitCardCheck.Models;
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

namespace HitCardCheck.Services
{
    public class HItCardService : IHostedService
    {
        private readonly LineNotifyService _line;
        private readonly AccountInfosetting _accountInfo;
        private readonly ClickPathSetting _clicksetting;
        private readonly int _unworktime;
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
            var work = new IdWorker(1, 1);
            List<long> aa = new List<long>();
            for (int i = 0; i < 20; i++)
            {
                aa.Add(work.NextId());
            }

            //var work1 = new IdWorker(2, 1);
            List<long> aa1 = new List<long>();
            for (int i = 0; i < 20; i++)
            {
                aa1.Add(work.NextId());
            }
;
            var service = ChromeDriverService.CreateDefaultService(@".", "chromedriver.exe");
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            DateTime Now = DateTime.Now;

            //判斷目前是否為上班時間
            bool isWorktime = Now.Hour < _unworktime ? true : false;

            //string trigger = Now.Hour < _unworktime ? "上班": "下班";



            //if (Now.Hour < _unworktime)
            //{
            //    trigger = "上班";
            //}
            //else
            //{
            //    trigger = "下班";
            //}
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options))
            {
                Login_Do(driver);

                //查詢刷卡紀錄頁籤
                HitCardLogSearch(driver, _accountInfo);


                //確認刷卡紀錄
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(3000);
                HitCheck(driver, isWorktime);
            }

            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options))
            {
                Login_Do(driver);

                Thread.Sleep(10000);

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(15000);
                HitCheck(driver, isWorktime);
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

        private void HitCardLogSearch(IWebDriver driver, AccountInfosetting acinfo)
        {
            string searchdate = DateTime.Today.ToString("yyyy/MM/dd");
            driver.FindElement(By.XPath(_clicksetting.HitCardLogButton)).Click();//點擊打卡紀錄頁籤按鈕
            driver.FindElement(By.XPath(_clicksetting.HitCardLogButton)).Click();//點擊打卡紀錄頁籤按鈕
            driver.FindElement(By.XPath(_clicksetting.HitCardSearchStartDate)).Clear();
            driver.FindElement(By.XPath(_clicksetting.HitCardSearchStartDate)).SendKeys(searchdate);//查詢刷卡紀錄起始時間
            driver.FindElement(By.XPath(_clicksetting.HitCardSearchEndDate)).Clear();
            driver.FindElement(By.XPath(_clicksetting.HitCardSearchEndDate)).SendKeys(searchdate);//查詢刷卡紀錄結束時間
            driver.FindElement(By.XPath(_clicksetting.HitCardSearchButton)).Click();//查詢刷卡紀錄結束時間

        }

        private void HitCheck(IWebDriver driver, bool isWorkTime)
        {

            string target = isWorkTime ? "上班" : "下班";
            try
            {
                //IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.ChickHit));//拉取打卡紀錄
                IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.ChickHit));//拉取打卡紀錄
                 string re =  lstTrElem[0].Text;
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
