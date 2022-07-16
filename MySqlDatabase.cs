
using MySql.Data.MySqlClient;

public class MySqlDatabase{
    private string mySqlServr;
    private string mysqlUserName;
    private string mySqlPwd;
    private string mySqlDatabase;
    private MySqlConnection connection;
    public MySqlDatabase(Conf conf){
        this.mySqlServr = conf.MySqlServer;
        this.mySqlPwd = conf.MySqlPwd;
        this.mysqlUserName = conf.MysqlUserName;
        this.mySqlDatabase = conf.MySqlDatabase;
        this.connection = new MySqlConnection();
        this.connection.ConnectionString = $"server={this.mySqlServr};uid={this.mysqlUserName};pwd={this.mySqlPwd};database={this.mySqlDatabase}";
    }

    public Object executeScript(string sql){
        int rowNum = 0;
        try{
            this.connection.Open();
            MySqlCommand mycmd = new MySqlCommand();
            mycmd.Connection = this.connection;
            mycmd.CommandText = sql;
            rowNum = mycmd.ExecuteNonQuery();
            Console.WriteLine("MySql数据库执行结果：影响行数" + rowNum.ToString());
        }catch(Exception ex){
            Console.WriteLine("MySql数据库executeScript失败！" + ex.Message); 
            Console.WriteLine(ex.StackTrace);
        }finally{
            this.connection.Close();
        }
        return rowNum;
       
    }


}