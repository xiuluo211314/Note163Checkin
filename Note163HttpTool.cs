using Newtonsoft.Json.Linq;
using PuppeteerSharp;

public class Note163HttpTool{
    /// 每日打开客户端（即登陆） 
    public static string DAILY_LOGIN_URL = "https://note.youdao.com/yws/api/daupromotion?method=sync";
    public static string NOTE_163_WEB_URL = "https://note.youdao.com/web";
    /// 签到前和签到后的api
    public static string SIGN_IN_BEFORE_AFTER_URL = "https://note.youdao.com/yws/mapi/user?method=get";
    /// PC端 签到
    public static string NOTE_CHECK_IN_PC_URL = "https://note.youdao.com/yws/mapi/user?method=checkin";

    /// 看广告的url
    public static string NOTE_AD_URL = "https://note.youdao.com/yws/mapi/user?method=adPrompt";
    /// 看视频广告url
    public static string NOTE_AD_VIDEO_URL = "https://note.youdao.com/yws/mapi/user?method=adRandomPrompt";


    public Note163HttpTool(){}

    /// <summary>
    /// 验证是否可用
    /// </summary>
    /// <param name="cookie"></param>
    /// <returns></returns>
    public async Task<(bool isInvalid, string result)> IsInvalid(string cookie)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ynote-android");
        client.DefaultRequestHeaders.Add("Cookie", cookie);
        //1.每日打开客户端（即登陆）
        string result = await (await client.PostAsync(DAILY_LOGIN_URL, null)).Content.ReadAsStringAsync();
        return (result.Contains("error", StringComparison.OrdinalIgnoreCase), result);
    }

    /// <summary>
    ///  模拟登陆到 有道云笔记网站获取最新的cookies
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<string> Login(string username, string password)
    {
        var launchOptions = new LaunchOptions
        {
            Headless = false,
            DefaultViewport = null,
            // ExecutablePath = @"/usr/bin/google-chrome"
            // ExecutablePath = @"I:\\vs_code_pro\\source-code\\AutoFillBaidujingyan\\support\\chromedriver.exe"
        };
        // Download the Chromium revision if it does not already exist
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);


        var browser = await Puppeteer.LaunchAsync(launchOptions);
        Page page = await browser.DefaultContext.NewPageAsync();

        await page.GoToAsync(NOTE_163_WEB_URL, 60_000);
        await page.WaitForSelectorAsync(".login-btn", new WaitForSelectorOptions { Visible = true });
        await page.TypeAsync(".login-username", username);
        await page.TypeAsync(".login-password", password);
        await Task.Delay(5_000);
        await page.ClickAsync(".login-btn");

        await page.WaitForSelectorAsync("#flexible-right", new WaitForSelectorOptions { Visible = true });

        var client = await page.Target.CreateCDPSessionAsync();
        var ckObj = await client.SendAsync("Network.getAllCookies");
        var cks = ckObj.Value<JArray>("cookies")
            .Where(p => p.Value<string>("domain").Contains("note.youdao.com"))
            .Select(p => $"{p.Value<string>("name")}={p.Value<string>("value")}");

        await browser.DisposeAsync();
        return string.Join(';', cks);
    }

  

}