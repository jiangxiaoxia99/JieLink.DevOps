﻿using Panuon.UI.Silver;
using PartialViewCheckUpdate.Commands;
using PartialViewCheckUpdate.Models.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PartialViewCheckUpdate.ViewModels
{
    public class CheckFilesViewModel : DependencyObject
    {
        public event Action UpdateFaildNotify;

        public DelegateCommand CheckUpdateCommand { get; set; }
        public CheckFilesViewModel()
        {
            this.CheckUpdateCommand = new DelegateCommand();
            this.CheckUpdateCommand.ExecuteAction = new Action<object>(this.CheckUpdate);
        }

        #region Property
        public string InstallPath
        {
            get { return (string)GetValue(InstallPathProperty); }
            set { SetValue(InstallPathProperty, value); OnTextChanaged(); }
        }

        // Using a DependencyProperty as the backing store for InstallPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InstallPathProperty =
            DependencyProperty.Register("InstallPath", typeof(string), typeof(CheckFilesViewModel));


        public string SetUpPackagePath
        {
            get { return (string)GetValue(SetUpPackagePathProperty); }
            set { SetValue(SetUpPackagePathProperty, value); OnTextChanaged(); }
        }

        // Using a DependencyProperty as the backing store for SetUpPackagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetUpPackagePathProperty =
            DependencyProperty.Register("SetUpPackagePath", typeof(string), typeof(CheckFilesViewModel));


        public string CheckResult
        {
            get { return (string)GetValue(CheckResultProperty); }
            set { SetValue(CheckResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckResultProperty =
            DependencyProperty.Register("CheckResult", typeof(string), typeof(CheckFilesViewModel));



        public bool EnableCheckUpdateResult
        {
            get { return (bool)GetValue(EnableCheckUpdateResultProperty); }
            set { SetValue(EnableCheckUpdateResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableCheckUpdateResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableCheckUpdateResultProperty =
            DependencyProperty.Register("EnableCheckUpdateResult", typeof(bool), typeof(CheckFilesViewModel));


        private void OnTextChanaged()
        {
            if (!string.IsNullOrEmpty(InstallPath) && !string.IsNullOrEmpty(SetUpPackagePath))
            {
                this.EnableCheckUpdateResult = true;
            }
            else
            {
                this.EnableCheckUpdateResult = false;

            }
        }
        #endregion


        private void CheckUpdate(object parameter)
        {
            string msg = string.Empty;
            EnumCheckFileResult result = EnumCheckFileResult.Ok;

            string sourcePath = this.InstallPath.Trim();
            string packagePath = this.SetUpPackagePath.Trim();
            this.CheckResult = string.Empty;
            //foreach (var dir in dirs)
            //{
            //    msg = $"开始检查目录{dir}……\n";
            //    this.CheckResult += msg;
            //    sourcePath = Path.Combine(this.SourceFilePath.Trim(), dir);
            //    r = CheckFileUpdate(sourcePath, packagePath);
            //    msg = $"完成检查目录{dir}……\n";
            //    this.CheckResult += msg;
            //    if (r == EnumCheckFileResult.FILE_ERROR1 || r == EnumCheckFileResult.OTHER_ERROR)
            //    {
            //        break;
            //    }
            //}
            result = CheckFileUpdate(sourcePath, packagePath);
            if (result == EnumCheckFileResult.Ok)
            {
                MessageBoxX.Show("JieLink软件升级成功！", "成功", null, MessageBoxButton.OK);
            }
            else if (result == EnumCheckFileResult.Faild)
            {
                Notice.Show("JieLink软件升级失败", "通知", 3, MessageBoxIcon.Warning);
                UpdateFaildNotify?.Invoke();
            }

        }

        /// <summary>
        /// 检测当前目录是否升级成功
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="packagePath"></param>
        /// <returns></returns>
        public EnumCheckFileResult CheckFileUpdate(string sourcePath, string packagePath)
        {
            string msg = string.Empty;
            try
            {
                int failCount = 0;

                //只根据安装包SmartCenter.Host.exe的修改时间来确定安装包的时间
                DateTime packageTime = File.GetLastWriteTime(packagePath + "\\programfiles\\SmartCenter\\SmartCenter.Host.exe");

                var files = from file in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                            where (file.EndsWith(".dll") || file.EndsWith(".exe"))
                            select file;
                if (files.Count() <= 0)
                {
                    msg = $"未检测到文件，请检查软件安装目录和安装包路径是否正确！\n";
                    this.CheckResult += msg;
                    MessageBoxX.Show(msg, "异常");
                    return EnumCheckFileResult.Error;
                }

                foreach (var f in files)
                {
                    if (packageTime.CompareTo(File.GetLastWriteTime(f)) > 0)
                    {
                        failCount++;
                    }
                }

                msg = $"[WARN] 检测到文件升级失败文件数量{failCount}个\n";
                this.CheckResult += msg;
                if (failCount > 5)
                {
                    msg = $"[WARN] JieLink软件升级失败\n";
                    this.CheckResult += msg;
                    return EnumCheckFileResult.Error;
                }
                msg = $"升级成功！\n";
                this.CheckResult += msg;
                return EnumCheckFileResult.Ok;
            }
            catch (UnauthorizedAccessException)
            {
                msg = "检查升级出错，文件没有访问权限，请设置文件夹读写权限\n";
                this.CheckResult += msg;
                return EnumCheckFileResult.Error;
            }
            catch (PathTooLongException)
            {
                msg = "检查升级出错，软件安装目录或安装包目录 指定的路径或文件名超过了系统定义的最大长度\n";
                this.CheckResult += msg;
                return EnumCheckFileResult.Error;
            }
            catch (DirectoryNotFoundException)
            {
                msg = "检查升级出错，所检查的目录不存在\n";
                this.CheckResult += msg;
                return EnumCheckFileResult.Error;
            }
            catch (Exception)
            {
                msg = "程序异常";
                this.CheckResult += msg;
                return EnumCheckFileResult.Error;
            }
        }
    }
}