using MySql.Data.MySqlClient;
using PartialViewDateCheck;
using PartialViewInterface;
using PartialViewInterface.Commands;
using PartialViewInterface.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Panuon.UI.Silver;
using PartialViewInterface.Models;
using System.Windows.Controls;


namespace PartialViewDateCheck
{
    public class CenterPartialSyncToolViewModel : DependencyObject
    {
        public DelegateCommand TestConnCommand { get; set; }

        public DelegateCommand StartTaskCommand { get; set; }


        /// <summary>
        /// 连接字符串
        /// </summary>
        public string DbConnectString { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string CenterDbPwd { get; set; }
        /// <summary>
        /// 连接字符串配置
        /// </summary>
        public DbConfigEntity DbConfig = new DbConfigEntity();

        public CenterPartialSyncToolViewModel()
        {
            TestConnCommand = new DelegateCommand();
            TestConnCommand.ExecuteAction = TestConn_Click;

            StartTaskCommand = new DelegateCommand();
            StartTaskCommand.ExecuteAction = StartTask_Click;
            //读取到配置文件，加载参数
            DbConfig.CenterDbConnStr = new DbConnEntity();
            DbConfig.BoxDbConnStrs = new List<DbConnEntity>();
            IsEnable0 = true;
            IsEnable1 = false;
        }



        public bool IsEnable0
        {
            get { return (bool)GetValue(IsEnable0Property); }
            set { SetValue(IsEnable0Property, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnable0Property =
            DependencyProperty.Register("IsEnable0", typeof(bool), typeof(CenterPartialSyncToolViewModel));
        public bool IsEnable1
        {
            get { return (bool)GetValue(IsEnable1Property); }
            set { SetValue(IsEnable1Property, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnable1Property =
            DependencyProperty.Register("IsEnable1", typeof(bool), typeof(CenterPartialSyncToolViewModel));

        /// <summary>
        /// 测试连接中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestConn_Click(object parameter)
        {

            PasswordBox passwordBox = parameter as PasswordBox;
            //PasswordBox的Password属性因为安全原因不支持直接绑定
            this.CenterDbPwd = PasswordBoxHelper.GetPassword(passwordBox);
            DbConnectString = $"Data Source={CenterIp};port={CenterDbPort};User ID={CenterDbUser};Password={CenterDbPwd};Initial Catalog={CenterDb};";

            try
            {
                MySqlHelper.ExecuteDataset(DbConnectString, "select * from sys_user limit 1");
                ShowMessage("中心数据库连接成功！");
                //存储中心连接字符串
                SaveCenterDbConfig();
                CheckBoxConnStr();
                if (dictBoxConnStr.Count > 0)
                {
                    IsEnable0 = false;
                    IsEnable1 = true;
                }
                else
                { ShowMessage("没有获取到任何盒子的连接信息！"); }
            }
            catch (Exception ex)
            {
                string str = ex.ToString();
                MessageBox.Show("数据库连接失败！");
            }
        }
        private void StartTask_Click(object parameter)
        {
            this.IsEnable1 = false;
            //2.开启任务
            StartCompareData();
            //3.禁用按钮
            this.IsEnable1 = true;
        }
        /// <summary>
        /// 比对数据
        /// </summary>
        private void StartCompareData()
        {


            List<string> strList = new List<string>();
            strList.Add("车牌号码" + "\t" + "盒子IP" + "                   " + "中心人名" + "   " + "中心人事编号" + "   " + "盒子人名" + "   " + "盒子人事编号");//循环添加元素
            List<PlateInfo> centerDatas = GetCenterData();
            if (centerDatas.Count <= 0)
            {
                ShowMessage($"未获取到满足条件的数据");

                return;
            }

            foreach (var ip in dictBoxConnStr.Keys)
            {
                ShowMessage($"比对盒子{ip}的数据");

                List<PlateInfo> boxDatas = GetBoxData(dictBoxConnStr[ip], ip);
                var centerNotExists = centerDatas.Where(a => !boxDatas.Exists(t => a.Plate == t.Plate)).ToList();
                if (centerNotExists.Count <= 0)
                {
                    foreach (var entity in centerDatas)
                    {
                        string BoxpersonName = boxDatas.FirstOrDefault((PlateInfo a) => a.Plate == entity.Plate).BoxpersonName;
                        string BoxpersonNo = boxDatas.FirstOrDefault((PlateInfo a) => a.Plate == entity.Plate).BoxpersonNo;
                        if (BoxpersonName != entity.CenterpersonName)
                        {
                            strList.Add(entity.Plate + "\t" + ip + "\t" + entity.CenterpersonName + "\t" + entity.CenterpersonNo + "\t" + BoxpersonName + "\t" + BoxpersonNo);//循环添加中心和盒子完全一致，但是人不一样的数据
                        }
                        //InsertData(dictBoxConnStr[ip], entity);
                    }

                    ShowMessage($"盒子{ip}的数据与中心一致");
                }
                else  //中心比盒子多
                {
                    foreach (var entity in centerNotExists)
                    {

                        strList.Add(entity.Plate + "\t" + ip + "\t" + entity.CenterpersonName + "\t" + entity.CenterpersonNo + "\t" + "无" + "\t" + "无");//循环添加中心比盒子多的数据

                    }
                    var centerExists = centerDatas.Where(a => boxDatas.Exists(t => a.Plate == t.Plate)).ToList();
                    foreach (var entity in centerDatas)
                    {
                        string BoxpersonName = boxDatas.FirstOrDefault((PlateInfo a) => a.Plate == entity.Plate).BoxpersonName;
                        string BoxpersonNo = boxDatas.FirstOrDefault((PlateInfo a) => a.Plate == entity.Plate).BoxpersonNo;
                        if (BoxpersonName != entity.CenterpersonName)
                        {
                            strList.Add(entity.Plate + "\t" + ip + "\t" + entity.CenterpersonName + "\t" + entity.CenterpersonNo + "\t" + BoxpersonName + "\t" + BoxpersonNo);//循环添加中心和盒子一致部分，但是人不一致的数据
                        }

                    }
                }

            }
            string[] strArray = strList.ToArray();
            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\CheckPerson.txt"))
            {
                File.Delete(Environment.CurrentDirectory + "\\CheckPerson.txt");
                FileStream Checkperson = new FileStream((Environment.CurrentDirectory + "\\CheckPerson.txt").ToString(), FileMode.Create);
                System.IO.File.SetAttributes((Environment.CurrentDirectory + "\\CheckPerson.txt").ToString(), FileAttributes.Normal);
                Checkperson.Close();
                File.WriteAllLines(Environment.CurrentDirectory + "\\CheckPerson.txt", strArray, Encoding.UTF8);
            }
            else
            {
                FileStream Checkperson = new FileStream((Environment.CurrentDirectory + "\\CheckPerson.txt").ToString(), FileMode.Create);
                System.IO.File.SetAttributes((Environment.CurrentDirectory + "\\CheckPerson.txt").ToString(), FileAttributes.Normal);
                Checkperson.Close();
                File.WriteAllLines(Environment.CurrentDirectory + "\\CheckPerson.txt", strArray, Encoding.UTF8);
            }
            System.Diagnostics.Process.Start("notepad.exe", Environment.CurrentDirectory + "\\CheckPerson.txt");

        }

        /// <summary>
        /// 获取中心的数据
        /// </summary>
        /// <returns></returns>
        private List<PlateInfo> GetCenterData()
        {

            ShowMessage($"查询control_person+control_voucher表");
            List<PlateInfo> syncBoxEntities = new List<PlateInfo>();
            string sql = $"select a.PersonName as CenterpersonName, b.voucherNo as Plate ,a.PersonNo as CenterpersonNo from control_person a inner join control_voucher b on a.PersonNo=b.PersonNO where b.Status=1";

            DataTable dt = MySqlHelper.ExecuteDataset(DbConnectString, sql).Tables[0];
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    syncBoxEntities.Add(new PlateInfo() { Plate = dr["Plate"].ToString(), CenterpersonName = dr["CenterpersonName"].ToString(), CenterpersonNo = dr["CenterpersonNo"].ToString() });
                }
            }
            return syncBoxEntities;
        }

        /// <summary>
        /// 获取盒子数据
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="minId"></param>
        /// <returns></returns>
        private List<PlateInfo> GetBoxData(string ConnString, string ip)
        {

            ShowMessage($"查询crd_credential+crd_person表");
            List<PlateInfo> syncBoxEntities = new List<PlateInfo>();
            string sql = $"Select a.No as Plate,b.Name as BoxpersonName ,a.personNo as BoxpersonNo from crd_credential a inner join crd_person b on a.personNo=b.No";
            DataTable dt = MySqlHelper.ExecuteDataset(ConnString, sql).Tables[0];
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    syncBoxEntities.Add(new PlateInfo() { Plate = dr["Plate"].ToString(), BoxpersonName = dr["BoxpersonName"].ToString(), BoxpersonNo = dr["BoxpersonNo"].ToString(), Ip = ip });
                }
            }
            return syncBoxEntities;
        }
        /// <summary>
        /// 获取所有盒子的连接信息
        /// </summary>
        Dictionary<string, string> dictBoxConnStr = new Dictionary<string, string>();
        private void CheckBoxConnStr()
        {
            ShowMessage("正在检测盒子的数据库连接，请等待！");
            string sql = "SELECT IP from control_devices where DeviceType = 25";
            DataTable dt = MySqlHelper.ExecuteDataset(DbConnectString, sql).Tables[0];
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ip = dr["IP"].ToString();
                    if (dictBoxConnStr.ContainsKey(ip))
                    {
                        continue;
                    }

                    string boxConn = $"Data Source={ip};port=3306;User ID=test;Password=js*168;Initial Catalog=smartbox;";

                    //读取到盒子配置文件，加载参数
                    ReadBoxConfig(ref boxConn, ip);

                    try
                    {
                        string cmd = "select * from sys_boxinformation";
                        MySqlHelper.ExecuteDataset(boxConn, cmd);
                        ShowMessage($"盒子{ip}连接成功");
                        //存储盒子连接字符串
                        
                        SaveBoxDbConfig(DbConfig, boxConn);
                        dictBoxConnStr.Add(ip, boxConn);
                    }
                    catch (Exception)
                    {
                        (Application.Current.MainWindow as WindowX).IsMaskVisible = true;
                        DbConfig dbConfig = new DbConfig(ip);
                        if (dbConfig.ShowDialog() == true)
                        {
                            ShowMessage($"盒子{ip}连接成功");
                            //存储盒子连接字符串
                            SaveBoxDbConfig(DbConfig, dbConfig.DbConnString);
                            dictBoxConnStr.Add(ip, dbConfig.DbConnString);
                        }
                        (Application.Current.MainWindow as WindowX).IsMaskVisible = false;
                    }
                }
                //全部保存盒子字符串后保存到文件
                SaveBoxDbConfigFile(DbConfig);
            }
        }
        private bool ReadBoxConfig(ref string str, string ip)
        {
            if (File.Exists(@"DbBoxConfig.ini"))
            {
                string ReadStr = File.ReadAllText(@"DbBoxConfig.ini");
                DbConfig.BoxDbConnStrs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DbConnEntity>>(ReadStr);
                if (DbConfig.BoxDbConnStrs != null)
                {
                    DbConnEntity find = DbConfig.BoxDbConnStrs.Find(a => a.Ip == ip);
                    if (find != null)
                    {
                        str = $"Data Source={find.Ip};port={find.Port};User ID={find.UserName};Password={find.Password};Initial Catalog={find.DbName};";
                        DbConfig.BoxDbConnStrs.Remove(find);
                        return true;
                    }
                    else
                    { 
                        return false; 
                    }
                    
                }

            }
            return  false;
        }
        private void SaveBoxDbConfigFile(DbConfigEntity dbconfig)
        {
            System.IO.File.WriteAllText("DbBoxConfig.ini", Newtonsoft.Json.JsonConvert.SerializeObject(dbconfig.BoxDbConnStrs), Encoding.UTF8);
        }
        private void SaveCenterDbConfig()
        {
            EnvironmentInfo.DbConnEntity.Ip = CenterIp;
            EnvironmentInfo.DbConnEntity.Port = Convert.ToInt32(CenterDbPort);
            EnvironmentInfo.DbConnEntity.UserName = CenterDbUser;
            EnvironmentInfo.DbConnEntity.Password = CenterDbPwd;
            EnvironmentInfo.DbConnEntity.DbName = CenterDb;
            ConfigHelper.WriterAppConfig("ConnectionString", JsonHelper.SerializeObject(EnvironmentInfo.DbConnEntity));
        }

        private void SaveBoxDbConfig(DbConfigEntity dbconfig, string conbox)
        {
            //string boxConn = $"Data Source={ip};port=10080;User ID=test;Password=123456;Initial Catalog=smartbox;";

            string[] strsplit = conbox.Split(new char[2] { '=', ';' });//以‘=’开始‘；’结束截取组装数组
            if (dbconfig.BoxDbConnStrs == null)
            {
                dbconfig.BoxDbConnStrs = new List<DbConnEntity>();
            }
            DbConnEntity boxcon = new DbConnEntity();
            boxcon.Ip = strsplit[1];
            boxcon.Port = Convert.ToInt32(strsplit[3]);
            boxcon.UserName = strsplit[5];
            boxcon.Password = strsplit[7];
            boxcon.DbName = strsplit[9];
            dbconfig.BoxDbConnStrs.Add(boxcon);

        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message"></param>

        public void ShowMessage(string message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (Message != null && Message.Length > 5000)
                {
                    Message = string.Empty;
                }

                if (message.Length > 0)
                {
                    Message += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}{Environment.NewLine}";
                }
            }));
        }
        public string CenterIp
        {
            get { return (string)GetValue(CenterIpProperty); }
            set { SetValue(CenterIpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PersonNo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterIpProperty =
            DependencyProperty.Register("CenterIp", typeof(string), typeof(CenterPartialSyncToolViewModel));
        public string CenterDbPort
        {
            get { return (string)GetValue(CenterDbPortProperty); }
            set { SetValue(CenterDbPortProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PersonNo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterDbPortProperty =
            DependencyProperty.Register("CenterDbPort", typeof(string), typeof(CenterPartialSyncToolViewModel));
        public string CenterDbUser
        {
            get { return (string)GetValue(CenterDbUserProperty); }
            set { SetValue(CenterDbUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PersonNo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterDbUserProperty =
            DependencyProperty.Register("CenterDbUser", typeof(string), typeof(CenterPartialSyncToolViewModel));
        public string CenterDb
        {
            get { return (string)GetValue(CenterDbProperty); }
            set { SetValue(CenterDbProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PersonNo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterDbProperty =
            DependencyProperty.Register("CenterDb", typeof(string), typeof(CenterPartialSyncToolViewModel));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(CenterPartialSyncToolViewModel));
    }
}
