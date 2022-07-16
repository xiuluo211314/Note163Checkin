using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class CheckInHandler{
        public CheckInHandler(HttpClient _scClient) 
        {
            this._scClient = _scClient;
   
        }
            private HttpClient _scClient{get; set;}
    private Conf _conf{get; set;}
    private RedisDatabase redisDatabase;
    private Note163HttpTool note163HttpTool = new Note163HttpTool();
    private static string AUTO_RECORD_INFO_INSERT = "INSERT INTO AUTO_RECORD_INFO (AUTO_COOKIE, AUTO_TYPE, AUTO_DESC, AUTO_COOKIE_STATUS, DAILY_LOGIN_SPACE_M, BEFORE_CHECK_IN_SPACE, BEFORE_CHECK_IN_SPACE_M, BEFORE_CHECK_IN_SPACE_G, PC_CHECK_IN_RES, PC_CHECK_IN_SPACE_M, PC_AD_SPACE_0_M, PC_AD_SPACE_1_M, PC_AD_SPACE_2_M, PC_AD_VIDEO_SPACE_0_M, PC_AD_VIDEO_SPACE_1_M, PC_AD_VIDEO_SPACE_2_M, PC_SPACE_ALL_M, CUR_ACCOUNT, AFTER_CHECK_IN_SPACE, AFTER_CHECK_IN_SPACE_M, AFTER_CHECK_IN_SPACE_G, CUR_FINALLY_ADD_SPACE_M,CREATE_TIME, UPDATE_TIME, DEL_FLAG, DAILY_DATE) VALUES(";
    private static string AUTO_RECORD_INFO_INSERT_END = " );";

    public static string NOTE_163_ = "Note163_";

    public static int SQUARE_1024 = 1048576;

    private StringBuilder stringBuilder;


    public CheckInHandler(HttpClient scClient, Conf conf){
        this._conf = conf;
        this._scClient = scClient;
        this.redisDatabase = null;
        this.stringBuilder = new StringBuilder(AUTO_RECORD_INFO_INSERT);
    }

    public async Task<string> mainHandler(){
        Console.WriteLine("有道云笔记签到开始运行 start...");
        if(this._conf == null){
            throw new Exception("CONF配置文件信息不存在！");
        }
        if(this._conf.Users == null || this._conf.Users.Length == 0){
            throw new Exception("CONF配置文件信息中，不包含Users数组信息!");
        }

        initRedisDatabase();         
        for (int i = 0; i < _conf.Users.Length; i++)
        {
            this.stringBuilder = new StringBuilder(AUTO_RECORD_INFO_INSERT);   
            User user = _conf.Users[i];
            string title = $"账号 {i + 1}: {user.Task} ";
            Console.WriteLine($"共 {_conf.Users.Length} 个账号，正在运行{title}...");

            // 先从redis中获取cookies
            string cookie = string.Empty;
            bool isInvalid = true;
            string result = string.Empty;
            // string userName = user.Username;
            // string redisKey = NOTE_163_ + userName;
            string redisKey = $"Note163_{user.Username}";
            bool isRedis = this.redisDatabase.isConnected("test");
            // redis连接上了
            if(isRedis){
                // 1. 从redis数据库中查询cookie
                var redisValue = await this.redisDatabase.Db.StringGetAsync(redisKey);
                // 2. 验证redis数据库中是否有值
                if (redisValue.HasValue)
                {
                    cookie = redisValue.ToString();
                    // 3. 调用每日登陆的api,DAILY_LOGIN_URL,来判断cookie是否可用
                    (isInvalid, result) = await note163HttpTool.IsInvalid(cookie);
                    Console.WriteLine("redis获取cookie,状态:{0}", isInvalid ? "无效" : "有效");
                }
            }
            // redis中的cookie不可用的话，需要模拟登陆login获取最新的cookie
            if(isInvalid){
                // 登陆网页，获取最新的cookie
                cookie = await this.note163HttpTool.Login(user.Username, user.Password);
                // 调用每日打开客户端（即登陆）的api,判断cookie是否可用
                (isInvalid, result) = await this.note163HttpTool.IsInvalid(cookie);
                Console.WriteLine("login登陆网页后获取cookie,状态:{0}", isInvalid ? "无效" : "有效");
                if (isInvalid)
                {
                    //Cookie失效， 发送通知
                    await Notify($"{title}Cookie失效，请检查登录状态！", true);
                    continue;
                }
                
            }
            // 更新redis中的cookie
            if (isRedis)
            {
                Console.WriteLine($"redis更新cookie:{await this.redisDatabase.Db.StringSetAsync(redisKey, cookie)}");
            }
            ///////////////////////////////////////////////////
            this.stringBuilder.Append($"'{cookie}',");
            this.stringBuilder.Append("'NOTE_163',");
            this.stringBuilder.Append("'有道云笔记的描述',");
            string cookieStatus = isInvalid ? "无效" : "有效";
            this.stringBuilder.Append($"'{cookieStatus}',");
            //////////////////////////////////////////////////////////////////////////////////////////////
            long space = 0; // 获得的空间
            space += Deserialize<YdNoteRsp>(result).RewardSpace;
            if(space > 0){
                string spaceStri = (space / SQUARE_1024).ToString();
                Console.WriteLine("调用登录API后，得到的空间大小是" + spaceStri + "M");
                this.stringBuilder.Append($"'{spaceStri}',");
            }else{
                this.stringBuilder.Append("'0',"); 
            }
                        
            // 调用各个API
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ynote-android");
            client.DefaultRequestHeaders.Add("Cookie", cookie);
            
            //A. 查看签到前的空间
            string beforeSign = await (await client.PostAsync(Note163HttpTool.SIGN_IN_BEFORE_AFTER_URL, null))
                .Content.ReadAsStringAsync();
            //// 签到前的存储空间
            string beforeSignQuote =  Deserialize<YdNoteBeforeSignInfo>(beforeSign).q;
            decimal tmp = 0;
            if(space > 0){
                // 登录api执行成功，并且获得了空间，此处需要去掉
                tmp = decimal.Parse(beforeSignQuote) - (decimal)space; 
                beforeSignQuote = tmp.ToString(); 
            }   
            Console.WriteLine("A.签到前的拥有空间大小:" + beforeSignQuote);
            stringBuilder.Append($"'{beforeSignQuote}',");
            stringBuilder.Append($"'{ bConvertToM(tmp)}',");
            // 转换成G  
            stringBuilder.Append($"'{bConvertToG(tmp)}',");

            // B. PC客户端的签到
            result = await (await client.PostAsync(Note163HttpTool.NOTE_CHECK_IN_PC_URL, null))
                .Content.ReadAsStringAsync();
            Console.WriteLine("B.PC端签到api的结果：" + result);
            stringBuilder.Append($"'{result}',");
            int bSpace = Deserialize<YdNoteRsp>(result).Space;
            Console.WriteLine("B.PC端签到api的结果，增加存储空间:" + (bSpace / SQUARE_1024).ToString() + "M");
            stringBuilder.Append($"'{bSpace / SQUARE_1024}',");
            space += bSpace;

            // TODO: 手机端签到呢？需要手机上的cookie

            // C. 看广告
            for (int j = 0; j < 3; j++)
            {
                result = await (await client.PostAsync(Note163HttpTool.NOTE_AD_URL, null))
                    .Content.ReadAsStringAsync();
                int cSpace = Deserialize<YdNoteRsp>(result).Space;
                Console.WriteLine("C-" + j.ToString() + ".看广告api的结果，增加存储空间:" + (cSpace / SQUARE_1024).ToString() + "M"); 
                stringBuilder.Append($"'{cSpace / SQUARE_1024}',");
                space += cSpace;
            }
            Console.WriteLine("C-ALL" +  ".看广告api的总结果，增加存储空间:" + (space / SQUARE_1024).ToString() + "M"); 

            // D. 看视频广告
            for (int j = 0; j < 3; j++)
            {
                result = await (await client.PostAsync(Note163HttpTool.NOTE_AD_VIDEO_URL, null))
                    .Content.ReadAsStringAsync();
                int dSpace = Deserialize<YdNoteRsp>(result).Space;
                Console.WriteLine("D-" + j.ToString() + ".看广告视频api的结果，增加存储空间:" + (dSpace / SQUARE_1024).ToString() + "M"); 
                stringBuilder.Append($"'{dSpace / SQUARE_1024}',");
                space += dSpace;
            }
            Console.WriteLine("D-ALL" +  ".看广告视频api的总结果，增加存储空间:" + (space / SQUARE_1024).ToString() + "M");

            await Notify($"有道云笔记{title}签到成功，共获得空间 {space / SQUARE_1024} M");
            stringBuilder.Append($"'{space / SQUARE_1024}',");
            stringBuilder.Append($"'{user.Task}',");
            // E. 查看签到后的空间
            string afterSign = await (await client.PostAsync(Note163HttpTool.SIGN_IN_BEFORE_AFTER_URL, null))
                .Content.ReadAsStringAsync();
            // 签到后的存储空间
            string afterSignQuote =  Deserialize<YdNoteBeforeSignInfo>(afterSign).q;   
            Console.WriteLine("签到后的拥有空间大小:" + afterSignQuote); 
            stringBuilder.Append($"'{afterSignQuote}',");
            stringBuilder.Append($"'{bConvertToM(decimal.Parse(afterSignQuote))}',");
            stringBuilder.Append($"'{bConvertToG(decimal.Parse(afterSignQuote))}',");
            // Console.WriteLine("新的url:https://note.youdao.com/yws/mapi/payment?method=status&pversion=v2");
            // string newUrl = await (await client.PostAsync("https://note.youdao.com/yws/mapi/payment?method=status&pversion=v2",null))
            //     .Content.ReadAsStringAsync();
            // Console.WriteLine("新的url后的结果：" + newUrl);

            decimal addSpace = decimal.Parse(afterSignQuote) - decimal.Parse(beforeSignQuote);
            if(addSpace > 0){
                // 存储空间增加了
                decimal addSpaceM = addSpace / SQUARE_1024;
                string addSpaceStr = addSpaceM.ToString() + "M";
                Console.WriteLine("本次执行最终增加了空间" + addSpaceStr);
                stringBuilder.Append($"'{addSpaceM}',");
            }else if(addSpace == 0){
                Console.WriteLine("本次执行最终增加了空间0M" );
                stringBuilder.Append("'0',");
            }else{
                Console.WriteLine("本次执行最终增加了空间为负数！出错！");
                stringBuilder.Append("'0',");
            }

            ////////////////////////////////////////记录到数据库中
            if(_conf.MySqlServer == string.Empty || _conf.MysqlUserName == string.Empty
                || _conf.MySqlDatabase == string.Empty || _conf.MySqlPwd == string.Empty){
                    Console.WriteLine("未配置MySql数据库或者配置数据为空");
            }else{
                persistToMysql(stringBuilder);
            }
        }
        Console.WriteLine("签到运行完毕---end");
        return "success";
    }
    /// <summary>
    /// B转换成G
    /// </summary>
    /// <param name="digital"></param>
    /// <returns></returns>
    public float bConvertToG(decimal digital){
        // 转换成G
        return MathF.Round(((float)digital / SQUARE_1024 / 1024),5);
    }
    public float bConvertToM(decimal digital){
        return MathF.Round((float)digital / SQUARE_1024,  0);
    }

    /// <summary>
    /// 存储到数据库中
    /// </summary>
    /// <param name="stringBuilder"></param>
    public void persistToMysql(StringBuilder stringBuilder){
        // 拼接字段： CREATE_TIME, UPDATE_TIME, DEL_FLAG
        DateTime currentTime = System.DateTime.Now;
        String format = "yyyy-MM-dd hh:mm:ss";
        currentTime = currentTime.AddHours(8);//转化为北京时间(北京时间=UTC时间+8小时 )            

        String dateStr = currentTime.ToString(format);
        stringBuilder.Append($"'{dateStr}','{dateStr}',");
        stringBuilder.Append("'',");
        // 执行日期 DAILY_DATE
        string curDay = currentTime.ToString("yyyy-MM-dd");
        stringBuilder.Append($"'{curDay}'");
        stringBuilder.Append(AUTO_RECORD_INFO_INSERT_END);

        MySqlDatabase mySqlDatabase = new MySqlDatabase(_conf);
        int rowNum = (int)mySqlDatabase.executeScript(stringBuilder.ToString());
        if (Debugger.IsAttached){
            Console.WriteLine(stringBuilder.ToString());
        }
        if(rowNum == 0){
            Console.WriteLine("MySql保存数据失败！!!");
        }else{
            Console.WriteLine($"MySql保存数据成功，条数是：{rowNum}!");
        }
        Console.WriteLine("------------------------------分割线-----------------------------------");

    }

    private void initRedisDatabase(){
        if(redisDatabase == null){
            redisDatabase = new RedisDatabase(this._conf);
        }
    }

    /// <summary>
    /// CONF文件中配置了Server酱的话，此处可以发送通知
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="isFailed"></param>
    /// <returns></returns>
    async Task Notify(string msg, bool isFailed = false)
    {
        Console.WriteLine(msg);
        if (this._conf.ScType == "Always" || (isFailed && this._conf.ScType == "Failed"))
        {
            await _scClient?.GetAsync($"https://sc.ftqq.com/{_conf.ScKey}.send?text={msg}");
        }
    }

    T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    });

}