
using StackExchange.Redis;

public class RedisDatabase : IMyDatabase{
    // private string connectString;
    private string rdsServer;
    private string rdsPwd;
    private IDatabase db;
    public IDatabase Db{
        get{
            return this.db;
        }
    }

    public RedisDatabase(Conf conf){
        this.rdsServer = conf.RdsServer;
        this.rdsPwd = conf.RdsPwd;
        string connectStr = $"{this.rdsServer},password={this.rdsPwd},name=Note163Checkin,defaultDatabase=0,allowadmin=true,abortConnect=false";
        this.connectDatabase(connectStr);
    }
    public bool checkDb(){
         if(this.db == null){
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "不能连接上redis");
        }
        return true;
    }

    public bool isConnected(object obj)
    {
        checkDb();
        string content = (string) obj;
        bool res = db.IsConnected(content);
        Console.WriteLine("redis连接状态:{0}", res ? "有效" : "无效");
        return res;
    }

    private void connectDatabase(string connectStr){
        try{
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectStr);
            this.db = redis.GetDatabase();        
        }catch(Exception ex){
            Console.WriteLine("获取Redis数据库的连接失败！" + ex.Message);
            Console.WriteLine(ex.StackTrace); // 显示出错位置
            this.db = null;
        }
    }

    // public bool redis

}
