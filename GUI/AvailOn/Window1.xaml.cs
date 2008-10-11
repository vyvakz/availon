using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Win32;
using Scheduler;
using BackupService;

namespace AvailOn
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private List<FileReport> fileReport = new List<FileReport>();
        public List<FileReport> FileReport
        {
            get { return fileReport; }
        }

        private List<Report> reports = new List<Report>();
        public List<Report> ReportDetails
        {
            get { return reports; }
        }

        string[] days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        string[] minIntervals = new string[] { "00", "15", "30", "45" };
        List<ProviderInfo> providers;
      
        //ConfigManager configMgr = new ConfigManager();
        //BackupManager.BackupManager backupManager = BackupManager.BackupManager.getInstance();

        public Window1()
        {
            InitializeComponent();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             folderTree1.Visible = true;
            this.folderTree1.CheckBoxes = true;
            this.folderTree1.ImageIndex = 0;
          //  this.folderTree1.Location = new System.Drawing.Point(24, 30);
            this.folderTree1.Name = "folderTree1";
            this.folderTree1.SelectedImageIndex = 0;
         //   this.folderTree1.Size = new System.Drawing.Size(287, 267);
            this.folderTree1.TabIndex = 1;
            folderTree1.GetDrives();
            //folderTree1.BeginUpdate();
          //  folderTree1.ToggleAfterCheckEvent(false);

            // Load the provider details
            
            BackupManager backupManager = BackupManager.getInstance();

            providers = backupManager.GetMediaProviders();

            foreach ( ProviderInfo i in providers) {
                SelectProviderComboBox.Items.Add(i.Name);
            } 

            ConfigManager configMgr = ConfigManager.Load();


            SelectProviderComboBox.SelectedItem = configMgr.getMediaType();

            Load_RestoreTree(configMgr);
            
           
        }

        private void Load_RestoreTree(ConfigManager configMgr)
        {
            BackupManager backupManager = BackupManager.getInstance();
            RestoreInfo restoreInfo = backupManager.GetRestoreInfo(configMgr);
            List<string> restoredFiles = restoreInfo.FileList;
            //load restore tree
            folderTree2 = new FolderTree.FolderTree();
            folderTree2.IsAccessible = true;
            folderTree2.CheckBoxes = true;
            foreach (string str in restoredFiles)
            {
                folderTree2.SetVirtualTree(str);
            }
        }
        // Save the profile

        private void button4_Click(object sender, RoutedEventArgs e)
        {
           


            // folderTree1.Update();
            folderTree1.ToggleAfterCheckEvent(false);
       

        //    listbox1.Items.Clear();

            BackupInfo backupItem = null;
            ConfigManager configMgr = ConfigManager.Load();

            //save tree ;)

            
                //TextWriter writer = new StreamWriter(configMgr.ProfileName + "FolderTree.xml");
                //XmlSerializer serializer = new XmlSerializer(typeof(FolderTree.FolderTree));
                //serializer.Serialize(writer, folderTree1);
                //writer.Close();
            

            // Clear the old set and rebuild new one.
            configMgr.BackupSet.Clear();
            foreach (FolderTree.FolderTree.DirTreeNode newItem in folderTree1.CheckedNodes)
            {

            //    listbox1.Items.Add(newItem.FullPath.ToString() + newItem.Type);
                backupItem = new BackupInfo(newItem.FullPath.ToString(), true, (BackupInfo.FileType) newItem.Type);
                configMgr.BackupSet.Add(backupItem);
            }
            folderTree1.ToggleAfterCheckEvent(true);
          
            configMgr.Priority = (ConfigManager.Priorities)
                            Convert.ToInt32(slider1_Copy.Value);
            
            // Now send the request to Backup Manager

            BackupManager backupMgr = BackupManager.getInstance();
            
            backupMgr.SaveProfile(configMgr);

       
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AvailOn.Close();
        }


        private void restore_Click(object sender, RoutedEventArgs e)
        {
            RestoreReq restoreReq = new RestoreReq();

            foreach (FolderTree.FolderTree.DirTreeNode newItem in folderTree2.CheckedNodes)
            {
             //   listbox1.Items.Add(newItem.FullPath.ToString());
                restoreReq.FileList.Add(newItem.FullPath.ToString());
                
            }

            restoreReq.Destination = alternateRestoreLocationText.Text;
            restoreReq.OverWrite = false;

            ConfigManager configMgr = ConfigManager.Load();

            BackupManager backupManager = BackupManager.getInstance();
            backupManager.StartRestore(configMgr, restoreReq);

            //load reports tab
            ReportTab_Loaded(sender, e);
        }

        //
        // Backup Now.
        //
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager configMgr = new ConfigManager();
            BackupInfo backupItem = null;
           
            foreach (FolderTree.FolderTree.DirTreeNode newItem in folderTree1.CheckedNodes)
            {
               // listbox1.Items.Add(newItem.FullPath.ToString() + newItem.Type);
                backupItem = new BackupInfo(newItem.FullPath.ToString(), true, (BackupInfo.FileType)newItem.Type);
                configMgr.BackupSet.Add(backupItem);
            }
            
            BackupManager.StartBackup(configMgr);

            //load reports tab
            ReportTab_Loaded(sender, e);

            //load restore tree
            Load_RestoreTree(configMgr);

        }

        // Configure the provider details
        //
        private void Config_Provider(object sender, RoutedEventArgs e)
        {
            fsMediaConfig1.IsEnabled = true;

            string selectedProvider = SelectProviderComboBox.SelectedItem.ToString();

            foreach (ProviderInfo i in providers)
            {
                if (i.Name.Equals(selectedProvider))
                {
                    fsMediaConfig1.setConfigDetails(i.getConfigDetails());
                    break;
                }
            }

        }

        private void SaveProvider_Click(object sender, RoutedEventArgs e)
        {
            string selectedProvider = SelectProviderComboBox.SelectedItem.ToString();
            ProviderInfo provider = null;

            Object detailsObject = fsMediaConfig1.getConfigDetails();
            foreach ( ProviderInfo i in providers)
            {
                if (i.Name.Equals(selectedProvider)) {
                    i.setConfigDetails(detailsObject);
                    provider = i;
                    break;
                }
            }

            if (provider != null)
            {
                BackupManager backupManager = BackupManager.getInstance();
                backupManager.SaveProviderDetails(provider);
            }
        }

        
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            scheduleGrid.Visibility = Visibility.Visible;
            tabControl1.IsEnabled = false;
        }

        private void Select_Restore_Location_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();

            fd.ShowDialog();
            alternateRestoreLocationText.Text = fd.SelectedPath;
        }

       

        private void scheduleRadioButton2_UnChecked(object sender, RoutedEventArgs e)
        {
            scheduleDailyComboBox.IsEnabled = false;
            scheduleDailyComboBox2.IsEnabled = false;
            scheduleDailyComboBox.Items.Clear();
            scheduleDailyComboBox2.Items.Clear();
        }

        private void scheduleRadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            scheduleDailyComboBox.IsEnabled = true;
            scheduleDailyComboBox2.IsEnabled = true;
            for (int i = 1; i <= 24; i++)
            {
                scheduleDailyComboBox.Items.Add(i);   
            }
            foreach (string mins in minIntervals)
            {
                scheduleDailyComboBox2.Items.Add(mins);

            }
        }

        private void scheduleRadioButton3_UnChecked(object sender, RoutedEventArgs e)
        {
            scheduleWeeklyComboBox1.IsEnabled = false;
            scheduleWeeklyComboBox1.Items.Clear();

            scheduleWeeklyComboBox2.IsEnabled = false;
            scheduleWeeklyComboBox2.Items.Clear();

            scheduleWeeklyComboBox3.IsEnabled = false;
            scheduleWeeklyComboBox3.Items.Clear();
        }

        private void scheduleRadioButton3_Checked(object sender, RoutedEventArgs e)
        {
            scheduleWeeklyComboBox1.IsEnabled = true;
            scheduleWeeklyComboBox2.IsEnabled = true;
            scheduleWeeklyComboBox3.IsEnabled = true;

            scheduleWeeklyComboBox2.Items.Add("AM");
            scheduleWeeklyComboBox2.Items.Add("PM");
            string time;
            for (int i = 1; i < 13; i++)
            {
                foreach (string mins in minIntervals)
                {
                    time = i.ToString() + ":" + mins;
                    scheduleWeeklyComboBox1.Items.Add(time);

                }
            }

            foreach (string day in days)
                scheduleWeeklyComboBox3.Items.Add(day);
        }

        private void EnableThrottleChecklBox_Checked(object sender, RoutedEventArgs e)
        {
            SettingsThrottleGroupBox.IsEnabled = true;
        }
        private void EnableThrottleChecklBox_UnChecked(object sender, RoutedEventArgs e)
        {
            SettingsThrottleGroupBox.IsEnabled = false;
        }


        private void radioButton5_Checked(object sender, RoutedEventArgs e)
        {
            
           AutomaticGroupBox1.Visibility =  Visibility.Visible;
           for (int i = 0; i <= 180; i++)
           {
               if(i <= 100)
                   scheduleCPUcomboBox.Items.Add(i);
               if (i < 180)
                   scheduleIdleComboBox.Items.Add(i + 1);
               if (i < 12)
                   scheduleTimesComboBox.Items.Add(i + 1);
           }

        }
        private void radioButton5_UnChecked(object sender, RoutedEventArgs e)
        {
            AutomaticGroupBox1.Visibility = Visibility.Hidden;
            scheduleCPUcomboBox.Items.Clear();
            scheduleIdleComboBox.Items.Clear();
            scheduleTimesComboBox.Items.Clear();

        }

        private void radioButton6_Checked(object sender, RoutedEventArgs e)
        {
            ScheduleGroupBox1.Visibility = Visibility.Visible;
        }

        private void radioButton6_UnChecked(object sender, RoutedEventArgs e)
        {
            ScheduleGroupBox1.Visibility = Visibility.Hidden;
        }

        private void ScheduleCancelButton_Click(object sender, RoutedEventArgs e)
        {
            scheduleGrid.Visibility = Visibility.Hidden;
            tabControl1.IsEnabled = true;
           
        }

        // Updated schedule
        private void ScheduleSaveButton1_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager configMgr = ConfigManager.Load();
            Scheduler.Trigger trigger = null;

            if (radioButton7.IsChecked == true)
            {
                // This is for automated schedule
                trigger = new OnIdleTrigger((int)Convert.ToInt32(scheduleCPUcomboBox.SelectedItem.ToString()),
                    (int) Convert.ToInt32(scheduleIdleComboBox.SelectedItem.ToString()),
                    (int) Convert.ToInt32(scheduleTimesComboBox.SelectedItem.ToString()));
              
            }
            else if (radioButton11.IsChecked == true)
            {
                if (scheduleRadioButton2.IsChecked == true)  // Daily trigger
                {
                    trigger = new DailyTrigger( (short) Convert.ToInt32(scheduleDailyComboBox.SelectedItem.ToString()), 
                        (short) Convert.ToInt32(scheduleDailyComboBox2.SelectedItem.ToString()) );
                }
                else if (scheduleRadioButton3.IsChecked == true) // Weekly trigger
                {
                }
            }
            else
            {
                // Unsupported trigger
            }

            configMgr.Trigger = trigger;
            BackupManager.ScheduleBackup(configMgr);
        }


        private void reportListViewTop_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //Update file report object on selection
            fileReport = reports[reportListViewTop.SelectedIndex].FileReports;
            //Update the binding so that the designer gets updated
            if (reportListViewBottom != null)
            reportListViewBottom.ItemsSource = FileReport;
        }
       
        private void reportListViewTop_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void fsMediaConfig1_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ReportTab_Loaded(object sender, RoutedEventArgs e)
        {
            reports = Scheduler.Report.GetReports();
            reportListViewTop.ItemsSource = ReportDetails;
        }

        private void ScheduleSaveButton1_Click_1(object sender, RoutedEventArgs e)
        {

        }

    }
}
