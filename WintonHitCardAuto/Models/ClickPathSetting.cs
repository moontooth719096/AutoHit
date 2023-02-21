namespace WintonHitCardAuto.Models
{
    public class ClickPathSetting
    {
        public string Url { get; set; }
        public Login Login { get; set; }
        public string HitCardLog_Button { get; set; }
        public string SignSearch_Tab { get; set; }
        public string applicant_Input { get; set; }
        public string applicantName { get; set; }
        public string Search_Btn { get; set; }
        public string SignLogData { get; set; }

    }

    public class Login
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string LoginButton { get; set; }

    }

}
