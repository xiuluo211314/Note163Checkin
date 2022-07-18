public class Conf
{
    /// <summary>
    /// 配置文件的类型，LOCAL_FILE: 表示conf文件夹下的conf.json文件；ACTION_SECRET:表示配置在github-action中的secret
    /// </summary>
    /// <value></value>
    public string ConfType { get; set; }
    public User[] Users { get; set; }
    public string ScKey { get; set; }
    public string ScType { get; set; }
    public string RdsServer { get; set; }
    public string RdsPwd { get; set; }

    public string MySqlServer {get; set;}
    public string MySqlPwd { get; set;}
    public string MySqlDatabase { get; set;}
    public string MysqlUserName {get; set;}

}