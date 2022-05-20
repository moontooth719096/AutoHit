using AutoClick.Models;
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

namespace AutoClick.Services
{
    public class HItCardService : IHostedService
    {
        private readonly LineNotifyService _line;
        private readonly AccountInfosetting set;
        private readonly ClickPathSetting _clicksetting;
        private readonly int _unworktime;
        public HItCardService(IConfiguration config, LineNotifyService line)
        {
            _line = line;
            var se1t = config.GetSection("AccountInfosetting");
            set = new AccountInfosetting();
            se1t.Bind(set);
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
            string trigger = string.Empty;
            if (Now.Hour < 19)
            {
                trigger = "上班";
            }
            else
            {
                trigger = "下班";
            }
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options))
            {

                driver.Navigate().GoToUrl("http://10.1.1.252/Ehrsnet/Account/Index");
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

                Login(driver,set);

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

                
                if (Now.Hour < _unworktime)
                {
                    //上班
                    Work(driver);
                }
                else
                {
                    //下班
                    UnWork(driver);
                }
            }

            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(service, options))
            {

                driver.Navigate().GoToUrl("http://10.1.1.252/Ehrsnet/Account/Index");
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

                Login(driver, set);

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);
                Thread.Sleep(10000);

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(15000);
                HitCheck(driver, trigger);


            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void Login(IWebDriver driver, AccountInfosetting acinfo)
        {
            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/div[1]/div/div/div/input")).SendKeys("9S0268");
            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/form/div[1]/div/div/div/input")).SendKeys(acinfo.Account);
            driver.FindElement(By.XPath(_clicksetting.Login.Account)).SendKeys(acinfo.Account);


            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/div[2]/div/div/div/input")).SendKeys("Q123660151");
            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/form/div[2]/div/div/div/input")).SendKeys(acinfo.Password);
            driver.FindElement(By.XPath(_clicksetting.Login.Password)).SendKeys(acinfo.Password);
            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/div[4]/div[3]/a/img")).Click();
            //driver.FindElement(By.XPath("//*[@id=\"package3\"]/div/div/div/div[3]/div/div[2]/div[2]/div[3]/a/img")).Click();
            driver.FindElement(By.XPath(_clicksetting.Login.LoginButton)).Click();
            
        }

        public void Work(IWebDriver driver)
        {
            //driver.FindElement(By.XPath("//*[@id=\"package39\"]/div/div/div[1]/ul/li[1]/button")).Click();
            driver.FindElement(By.XPath(_clicksetting.WorkButton)).Click();
                                         
        }

        public void UnWork(IWebDriver driver)
        {
            //driver.FindElement(By.XPath("//*[@id=\"package39\"]/div/div/div[1]/ul/li[2]/button")).Click();
            driver.FindElement(By.XPath(_clicksetting.UnWorkButton)).Click();
        }

        private void HitCheck(IWebDriver driver, string trigger)
        {
            //IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath("//*[@id=\"package39\"]/div/div/div[2]/div[1]/div[2]/div/div[1]/div/div[2]/table/tbody/tr"));
            //IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath("//*[@id=\"package39\"]/div/div/div[2]/div/div[1]/div[2]/div/div[1]/div/div[2]/table/tbody/tr"));
            IReadOnlyList<IWebElement> lstTrElem = driver.FindElements(By.XPath(_clicksetting.ChickHit));
            //IReadOnlyList<IWebElement> lstTrElem = aa.FindElements(By.TagName("tr"));

            HitCardInfo data  =null;
            try
            {
                var ssss = lstTrElem.Select(x =>
                {
                    string texts = x.Text;
                    string[] spl = texts.Split("\r\n");
                    return new HitCardInfo { Hittime = DateTime.Parse(spl[0]), Content = spl[1] };

                });

                List<HitCardInfo> aaa = lstTrElem.Select(x =>
                 {
                     HitCardInfo re = new HitCardInfo();
                     try
                     {
                         string texts = x.Text;
                         string[] spl = texts.Split("\r\n");
                         re = new HitCardInfo { Hittime = DateTime.Parse(spl[0]), Content = spl[1] };
                     }
                     catch (Exception ex)
                     { 
                     
                     }
                     return re;
                 }).Where(z => z.Content.Trim() == trigger).ToList();

                if(aaa!=null && aaa.Count()>0)
                    data =aaa.OrderBy(y => y.Hittime).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            if (data != null)
            {
                _line.LineNotify_Send($"\n最早已於 {data.Hittime.ToString("yyyy/MM/dd HH:mm:ss")} 打了 {data.Content} 卡");
            }
        }

     
    }
}
