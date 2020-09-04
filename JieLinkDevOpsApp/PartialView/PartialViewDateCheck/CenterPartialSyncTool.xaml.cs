using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PartialViewInterface;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using System.IO;
using PartialViewInterface.Models;
using PartialViewInterface.Utils;
using Panuon.UI.Silver;

namespace PartialViewDateCheck
{
    /// <summary>
    /// CenterPartialSyncTool.xaml 的交互逻辑
    /// </summary>
    public partial class CenterPartialSyncTool : UserControl, IPartialView
    {
        public string MenuName
        {
            get { return "凭证数据核对"; }
        }

        public string TagName
        {
            get { return "CenterPartialSyncTool1"; }
        }

        public MenuType MenuType
        {
            get { return MenuType.Center; }
        }
        CenterPartialSyncToolViewModel viewModel;
        public CenterPartialSyncTool()
        {
            InitializeComponent();
            viewModel = new CenterPartialSyncToolViewModel();
            DataContext = viewModel;
            
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EnvironmentInfo.ConnectionString))
            {
                viewModel.CenterIp = EnvironmentInfo.DbConnEntity.Ip;
                viewModel.CenterDbPort = EnvironmentInfo.DbConnEntity.Port.ToString();
                viewModel.CenterDbUser = EnvironmentInfo.DbConnEntity.UserName;
                viewModel.CenterDbPwd = EnvironmentInfo.DbConnEntity.Password;
                viewModel.CenterDb = EnvironmentInfo.DbConnEntity.DbName;
                
            }
        }
    }
}
