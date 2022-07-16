using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using StackExchange.Redis;
using System.Text.Json;

namespace NOTE163CHECKIN{
    class Program{

        static string GetEnvValue(string key) => Environment.GetEnvironmentVariable(key);

        static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        static async Task Main(string[] args)
        {
            HttpClient _scClient = null;
            Conf _conf = Deserialize<Conf>(GetEnvValue("CONF"));
            if (!string.IsNullOrWhiteSpace(_conf.ScKey))
            {
                _scClient = new HttpClient();
            }

            CheckInHandler handler =  new CheckInHandler(_scClient, _conf);
            await handler.mainHandler();
        }
        // static void Main(string[] args){
 
        //     // Conf _conf = Deserialize<Conf>(GetEnvValue("CONF"));
        //     // MySqlDatabase mySqlDatabase = new MySqlDatabase(_conf);
        //     // string sql = "INSERT INTO AUTO_RECORD_INFO (CREATE_TIME, UPDATE_TIME, DEL_FLAG, AUTO_COOKIE, AUTO_TYPE, AUTO_DESC, AUTO_COOKIE_STATUS, DAILY_LOGIN_SPACE_M, BEFORE_CHECK_IN_SPACE, BEFORE_CHECK_IN_SPACE_M, BEFORE_CHECK_IN_SPACE_G, PC_CHECK_IN_RES, PC_CHECK_IN_SPACE_M, PC_AD_SPACE_0_M, PC_AD_SPACE_1_M, PC_AD_SPACE_2_M, PC_AD_VIDEO_SPACE_0_M, PC_AD_VIDEO_SPACE_1_M, PC_AD_VIDEO_SPACE_2_M, PC_SPACE_ALL_M, CUR_ACCOUNT, AFTER_CHECK_IN_SPACE, AFTER_CHECK_IN_SPACE_M, AFTER_CHECK_IN_SPACE_G, CUR_FINALLY_ADD_SPACE_M) VALUES('2022-07-16 10:34:09','2022-07-16 10:34:12',NULL,'Hm_lvt_fcbf8a457b2c5ae9cc58b5bf4cb7cef1=1657875554;Hm_lpvt_fcbf8a457b2c5ae9cc58b5bf4cb7cef1=1657875554;__snaker__id=XvEekH777KqYYzZK;___rl__test__cookies=1657875560702;JSESSIONID=A5C6A03A4C4447FF87450A9962B4343C.ynote-accountserver-docker-cwonline-bak-3-a7x86-ey6it-686bj6t8w-8081;__yadk_uid=xhsokeF7xifzmIb8mzMpPbKnUlwPO2VX;gdxidpyhxdE=Ogn14uHn0QAP%5Cjenxf1qmHO2iURikadhQKK3795I46ETbepoNYSh6mG0oT3H1tuSHGN7cW0xx5LyvIsl3KV6N4U5Gvein1EralDOP%2F8DMRuKi%5CdkEqWdG2oGtTEPEqynAdtUo2KuO50uwtauv1j%5CzhbMrwglYCEGeOlS9BKeLm%2FdEK5f%3A1657876453265;_9755xjdesxxd_=32;YD00688109880970%3AWM_NI=EsQonYj7sGdpOl1fL%2BvjjAgSv7Z%2BYcRIRiifA8b%2BHfQJwS75KMjkzYGhI%2B1RRA4C4SuJeR87%2FV0dejsMGrThKVvl4q%2BVp18KJolj%2FTtdWlIJE5dRkghJUXkZ3tdyw%2FIJQU8%3D;YD00688109880970%3AWM_NIKE=9ca17ae2e6ffcda170e2e6eed8f56a81e789abb74ef39e8bb3c54b928f8eacd159839f98a2ed53aae99a99e12af0fea7c3b92a87ad9f90ed53aae9bda8e77c8db4ff87cf21b6b6a6a2e659b6e996ccdc6992979e8aec6e83e8aecccb6e9396b9b4b63f8993a68cf95385b887ccb45a929ffca4b759b796abb3bb3ded8a0085e862b1938993d764898aba8db77be98ac0d2bb418ab1a48bae60b8bc998dcc4287ebacb3ea6dac969bb9ea52f1acac9af6258b969fb6bb37e2a3;YD00688109880970%3AWM_TID=geOOKUhVQHFAAQQBVEfFWBFSQmJzqElL;YNOTE_SESS=v2|NlmEN_kMSk5nLJLOMpB0Yf0HpyhHk5RPFO4kGO46BR6LkM6z0M6y0T4h4OEnfJuRk5P4k5RH6y0zmhLq4Ofw40gFRfPFnHUGR;YNOTE_PERS=\"v2|urstoken||YNOTE||web||-1||1657875559891||112.48.52.226||hengheng0607@163.com||qS6MwZ0LeBRYGnHTyRMq40UWOfUfRfgZ0pLRfOY6LzERzf0Hz5RLeK0YEO4Om6MeBRUWhfYW0Hkf0zWPMOAhfwyR\";YNOTE_LOGIN=3||1657875559906;YNOTE_CSTK=ATv9GDg9','NOTE_163','有道云笔记的描述','有效','1','3290431488','3138','3.06445','{\"total\":10485760,\"time\":1657904760924,\"success\":0,\"space\":2097152}','2','0','0','0','0','0','0','3','Auto_Space_163 ','3291480064','3139','3.06543','1');";
        //     // int rowNum = (int)mySqlDatabase.executeScript(sql);
        //     // Console.WriteLine("dddddd:" + rowNum.ToString());

        //     decimal tmp = 3290431488;
        //     float res = MathF.Round(((float)tmp / 1024 / 1024 / 1024),5);
        //     Console.WriteLine(res);
        //     // {"total":10485760,"time":1657904760924,"success":0,"space":2097152}

        // }

    }


}
