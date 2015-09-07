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

using CsvHelper;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using VCS_WOZ;
using DailyCoding.EasyTimer;
using System.Windows.Resources;
using System.Collections.ObjectModel;




namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public static win_subject subject_window = new win_subject();
        public const string _version = "v0.13";
        
        public static int _Trial = 1;
        public static int _Max_Trials;
        public static int[] _randTrial;
        
        public static int line_select;  //Use to check optimal choice for POI nav
        
        public static string _logfilename;
        public static string _logAllData;
        public static string _logDataBackup;
        
        public static string _PID;
        public static string _RunType;
        public static string _Curr_Audio;

        public static List<restaurant> _Practice_NAV;
        public static List<restaurant> _Practice_Radio;
        public static List<restaurant> _Practice_Hyrbid_NAV;
        public static List<restaurant> _Practice_Audio_NAV;
        public static List<restaurant> _Debug_Trial;
        
        public static List<restaurant> _Main_Scen1A;
        public static List<restaurant> _Main_Scen2A;
        public static List<restaurant> _Main_Scen3A;
        public static List<restaurant> _Main_Scen4A;

        public static List<restaurant> _Main_Scen1B;
        public static List<restaurant> _Main_Scen2B;
        public static List<restaurant> _Main_Scen3B;
        public static List<restaurant> _Main_Scen4B;
        
        public static List<restaurant> _Curr_Task;
        public static List<restaurant> _Curr_Trial;

        public static WMPLib.WindowsMediaPlayer _Player;
        public static int _Volume = 30;
        public const int _TDTcount = 60;
        public static string _scenario = "NA";

        public static DateTime _StartTime;
        public static double _TempTime;

        public static int n_btn_none_count = 0;
        public static int r_cmd_wrong_count = 0;
        public static int r_cmd_repeat_count = 0;
        public static int r_cmf_wrong_count = 0;
        public static int r_cmf_repeat_count = 0;
        public static bool r_complete_once = true;

        public static IDisposable stopHandle;

        public MainWindow()
        {
            InitializeComponent();
            
            //Display Version
            wiz_version.Content = _version;
            Title = "Wizard Window "+_version;

            //Load CSV Files
            _Practice_NAV = GetRecord("taskfiles/practice_nav_visual.csv");
            _Practice_Radio = GetRecord("taskfiles/practice_Radio.csv");
            _Practice_Hyrbid_NAV = GetRecord("taskfiles/practice_nav_both.csv");
            _Practice_Audio_NAV = GetRecord("taskfiles/practice_nav_audio.csv");

            _Main_Scen1A = GetRecord("taskfiles/main_scen1a.csv");
            _Main_Scen2A = GetRecord("taskfiles/main_scen2a.csv");
            _Main_Scen3A = GetRecord("taskfiles/main_scen3a.csv");
            _Main_Scen4A = GetRecord("taskfiles/main_scen4a.csv");

            _Main_Scen1B = GetRecord("taskfiles/main_scen1b.csv");
            _Main_Scen2B = GetRecord("taskfiles/main_scen2b.csv");
            _Main_Scen3B = GetRecord("taskfiles/main_scen3b.csv");
            _Main_Scen4B = GetRecord("taskfiles/main_scen4b.csv");

            _Debug_Trial = GetRecord("taskfiles/tdt_only.csv");

            _Player = new WMPLib.WindowsMediaPlayer();

            //string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //MessageBox.Show(dir.ToString());
            
            subject_window.r_now_listening.Visibility = Visibility.Hidden;
            subject_window.radio_title.Visibility = Visibility.Hidden;

            wiz_btn_start.IsEnabled = false;
            wiz_btn_none.IsEnabled = false;
            wiz_btn_continue.IsEnabled = false;
            wiz_select.IsHitTestVisible = false;
            _Player.settings.volume = _Volume;
            volume_box.Text = _Player.settings.volume.ToString();

            //Radio BTN Initialization
            r_cmd_correct.IsEnabled = false;
            r_cmf_correct.IsEnabled = false;
            r_cmd_wrong.IsEnabled = false;
            r_cmf_wrong.IsEnabled = false;
            r_cmd_repeat.IsEnabled = false;
            r_cmf_repeat.IsEnabled = false;

            //Create Logall Data File in ...\AppData\Roaming\VCS Bridge Study
            string subPath =Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VCS Bridge Study"; // your code goes here
            if (!Directory.Exists(subPath)) { Directory.CreateDirectory(subPath); }

            _logAllData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VCS Bridge Study\\logdata_all.csv";
            if(!File.Exists(_logAllData))
            {
                using (StreamWriter writer = new StreamWriter(@_logAllData, true))
                {
                    string header = "PID, Date, Time, RunType, Scenario, Trial, Task, Mode, InputEvent, ElapsedTime, Duration, N_OptimalChoice, R_FirstTry, Notes";
                    writer.WriteLine(header);
                }
            }
            
 
        }

        private List<restaurant> GetRecord(string filename)
        {
            var stream = new StreamReader(@filename);
            var reader = new CsvReader(stream);
            var records = reader.GetRecords<restaurant>();

            return records.ToList();
        }

        private void updateTrial(int trial, List<restaurant> records)
        {
            List<restaurant> LISTrecord = new List<restaurant>();
            //_Max_Trials = (from item in records select item.Trial).Max();
            var query =
                    from record in records
                    //where record.Trial == trial
                    where record.Trial == _randTrial[trial-1]
                    orderby record.Distance ascending
                    select record;

            foreach (var g in query)
            {
                //Debug.Print("Name: {0}, Rating: {1}", g.Name, g.Rating);
                LISTrecord.Add(g);
            }
            _Curr_Trial = LISTrecord;
        }

        private int getAudioDuration(string filename)
        {
            int duration;
            WMPLib.WindowsMediaPlayer wmp = new WMPLib.WindowsMediaPlayer();
            WMPLib.IWMPMedia m = wmp.newMedia(filename);
            duration = Convert.ToInt32(m.duration);
            duration = duration * 1000;
            return duration;
        }

        //Show Starting Navigation Sign
        private void showNavConfirm()
        {   
            if (_Curr_Trial[0].Task.Contains("Nav"))
            {
                wiz_select.IsHitTestVisible = false;
                wiz_btn_none.IsEnabled = false;
                wiz_btn_continue.IsEnabled = false;
                Dispatcher.Invoke(new Action(() =>
                {
                    subject_window.nav_confirm.Visibility = Visibility.Visible;
                }));
                
                PlayAudioFile("audio/nav_confirm.mp3");
                logData("show confirmation", false, false);
                EasyTimer.SetTimeout(() =>
                {
                    hideWrongCommand();
                }, 5000);
            }
        }

        private void displayLIST(List<restaurant> LISTrecord)
        {
            //Audio Only Mode
            if(_Curr_Trial[0].Mode == "Audio Only")
            {   
                Dispatcher.Invoke(new Action(() =>
                {
                    subject_window.audio_only.Visibility = Visibility.Visible;
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    subject_window.audio_only.Visibility = Visibility.Hidden;
                }));
            }
            
            Dispatcher.Invoke(new Action(() =>
            {
                //wiz_trial.Content = "Trial: " + LISTrecord[0].Trial+" / "+_Max_Trials;
                wiz_trial.Content = "Trial: " + _Trial + " / " + _Max_Trials;
                wiz_mode.Content = "Mode: " + LISTrecord[0].Mode;
                wiz_task.Content = "Task: " + LISTrecord[0].Task;

                if (LISTrecord[0].Task == "Radio")
                {
                    Wiz_Tab.SelectedIndex = 1;
                    wiz_radio_correct.Content = LISTrecord[0].R_Name;

                    subject_window.subj_tab.SelectedIndex = 1;
                    subject_window.radio_title.Content = LISTrecord[0].R_Name;
                }
                else if (LISTrecord[0].Task == "Nav Easy" || LISTrecord[0].Task == "Nav Hard")
                {
                    Wiz_Tab.SelectedIndex = 0;
                    subject_window.subj_tab.SelectedIndex = 0;

                    l1_name.Content = LISTrecord[0].Name;
                    l1_cuisine.Content = LISTrecord[0].Cuisine;
                    l1_distance.Content = LISTrecord[0].Distance + " miles";
                    l1_reviews.Content = LISTrecord[0].Reviews + " reviews";
                    l1_price.Content = LISTrecord[0].Price;
                    l1_rating.Source = new BitmapImage(getImageURI(LISTrecord[0].Rating));

                    l2_name.Content = LISTrecord[1].Name;
                    l2_cuisine.Content = LISTrecord[1].Cuisine;
                    l2_distance.Content = LISTrecord[1].Distance + " miles";
                    l2_reviews.Content = LISTrecord[1].Reviews + " reviews";
                    l2_price.Content = LISTrecord[1].Price;
                    l2_rating.Source = new BitmapImage(getImageURI(LISTrecord[1].Rating));

                    l3_name.Content = LISTrecord[2].Name;
                    l3_cuisine.Content = LISTrecord[2].Cuisine;
                    l3_distance.Content = LISTrecord[2].Distance + " miles";
                    l3_reviews.Content = LISTrecord[2].Reviews + " reviews";
                    l3_price.Content = LISTrecord[2].Price;
                    l3_rating.Source = new BitmapImage(getImageURI(LISTrecord[2].Rating));

                    l4_name.Content = LISTrecord[3].Name;
                    l4_cuisine.Content = LISTrecord[3].Cuisine;
                    l4_distance.Content = LISTrecord[3].Distance + " miles";
                    l4_reviews.Content = LISTrecord[3].Reviews + " reviews";
                    l4_price.Content = LISTrecord[3].Price;
                    l4_rating.Source = new BitmapImage(getImageURI(LISTrecord[3].Rating));

                    l5_name.Content = LISTrecord[4].Name;
                    l5_cuisine.Content = LISTrecord[4].Cuisine;
                    l5_distance.Content = LISTrecord[4].Distance + " miles";
                    l5_reviews.Content = LISTrecord[4].Reviews + " reviews";
                    l5_price.Content = LISTrecord[4].Price;
                    l5_rating.Source = new BitmapImage(getImageURI(LISTrecord[4].Rating));

                    subject_window.sl1_name.Content = LISTrecord[0].Name;
                    subject_window.sl1_cuisine.Content = LISTrecord[0].Cuisine;
                    subject_window.sl1_distance.Content = LISTrecord[0].Distance + " miles";
                    subject_window.sl1_reviews.Content = LISTrecord[0].Reviews + " reviews";
                    subject_window.sl1_price.Content = LISTrecord[0].Price;
                    subject_window.sl1_rating.Source = new BitmapImage(getImageURI(LISTrecord[0].Rating));

                    subject_window.sl2_name.Content = LISTrecord[1].Name;
                    subject_window.sl2_cuisine.Content = LISTrecord[1].Cuisine;
                    subject_window.sl2_distance.Content = LISTrecord[1].Distance + " miles";
                    subject_window.sl2_reviews.Content = LISTrecord[1].Reviews + " reviews";
                    subject_window.sl2_price.Content = LISTrecord[1].Price;
                    subject_window.sl2_rating.Source = new BitmapImage(getImageURI(LISTrecord[1].Rating));

                    subject_window.sl3_name.Content = LISTrecord[2].Name;
                    subject_window.sl3_cuisine.Content = LISTrecord[2].Cuisine;
                    subject_window.sl3_distance.Content = LISTrecord[2].Distance + " miles";
                    subject_window.sl3_reviews.Content = LISTrecord[2].Reviews + " reviews";
                    subject_window.sl3_price.Content = LISTrecord[2].Price;
                    subject_window.sl3_rating.Source = new BitmapImage(getImageURI(LISTrecord[2].Rating));

                    subject_window.sl4_name.Content = LISTrecord[3].Name;
                    subject_window.sl4_cuisine.Content = LISTrecord[3].Cuisine;
                    subject_window.sl4_distance.Content = LISTrecord[3].Distance + " miles";
                    subject_window.sl4_reviews.Content = LISTrecord[3].Reviews + " reviews";
                    subject_window.sl4_price.Content = LISTrecord[3].Price;
                    subject_window.sl4_rating.Source = new BitmapImage(getImageURI(LISTrecord[3].Rating));

                    subject_window.sl5_name.Content = LISTrecord[4].Name;
                    subject_window.sl5_cuisine.Content = LISTrecord[4].Cuisine;
                    subject_window.sl5_distance.Content = LISTrecord[4].Distance + " miles";
                    subject_window.sl5_reviews.Content = LISTrecord[4].Reviews + " reviews";
                    subject_window.sl5_price.Content = LISTrecord[4].Price;
                    subject_window.sl5_rating.Source = new BitmapImage(getImageURI(LISTrecord[4].Rating));
                }                 
            }));
        }

        private void checkPromptAudio()
        {
            if (_Curr_Trial[0].Task == "Radio")
            {
                PlayAudioFile("audio/radio/" + _Curr_Trial[0].R_Audio_Prompt);

                int duration = getAudioDuration("audio/radio/" + _Curr_Trial[0].R_Audio_Prompt);

                EasyTimer.SetTimeout(() =>
                {
                    PlayAudioFile("audio/chime.mp3");
                    logData("chime", false, false);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        //Radio BTN enabling
                        r_cmd_correct.IsEnabled = true;
                        r_cmd_wrong.IsEnabled = true;
                        r_cmd_repeat.IsEnabled = true;
                    }));
                }, duration + 200);
            }
            else if (_Curr_Trial[0].Task == "Nav Easy" || _Curr_Trial[0].Task == "Nav Hard")
            {
                PlayNavPrompt();
            } 
            else if(_Curr_Task[0].Task.Contains("TDT"))
            {
                //startTDTcountdown();
            }
        }

        
        private Uri getImageURI(double star)
        {
            string uri="";
            if(star == 1){uri = "images/10star.png";}
            else if (star == 1.5) { uri = "images/15star.png"; }
            else if (star == 2) { uri = "images/20star.png"; }
            else if (star == 2.5) { uri = "images/25star.png"; }
            else if (star == 3) { uri = "images/30star.png"; }
            else if (star == 3.5) { uri = "images/35star.png"; }
            else if (star == 4) { uri = "images/40star.png"; }
            else if (star == 4.5) { uri = "images/45star.png"; }
            else if (star == 5) { uri = "images/50star.png"; }
            else if (star == 0) {uri = "images/none.png";}
            
            var uriImage = new Uri(@uri, UriKind.Relative);
            return uriImage;
        } 



        //==================
        //Mouse Movement Actions for Nav
        //==================
        private void mouse_enter(object sender, MouseEventArgs e)
        {
            if (sender == line1) { line1.BorderBrush = Brushes.LightGreen; }
            else if (sender == line2) { line2.BorderBrush = Brushes.LightGreen; }
            else if (sender == line3) { line3.BorderBrush = Brushes.LightGreen; }
            else if (sender == line4) { line4.BorderBrush = Brushes.LightGreen; }
            else { line5.BorderBrush = Brushes.LightGreen; }
        }

        private void mouse_leave(object sender, MouseEventArgs e)
        {
            var group_col = new SolidColorBrush(Color.FromArgb(255, 213, 223, 229));
            //line1.BorderBrush = group_col;

            if (sender == line1) { line1.BorderBrush = group_col; line1.Background = Brushes.White; }
            else if (sender == line2) { line2.BorderBrush = group_col; line2.Background = Brushes.White; }
            else if (sender == line3) { line3.BorderBrush = group_col; line3.Background = Brushes.White; }
            else if (sender == line4) { line4.BorderBrush = group_col; line4.Background = Brushes.White; }
            else { line5.BorderBrush = group_col; line5.Background = Brushes.White; }
        }

        private void mouse_down(object sender, MouseButtonEventArgs e)
        {
            if (sender == line1) { line1.Background = Brushes.LightGreen; }
            else if (sender == line2) { line2.Background = Brushes.LightGreen; }
            else if (sender == line3) { line3.Background = Brushes.LightGreen; }
            else if (sender == line4) { line4.Background = Brushes.LightGreen; }
            else { line5.Background = Brushes.LightGreen; }
        }

        private void mouse_up(object sender, MouseButtonEventArgs e)
        {
            
            if (sender == line1) { line1.Background = Brushes.White; }
            else if (sender == line2) { line2.Background = Brushes.White;}
            else if (sender == line3) { line3.Background = Brushes.White;}
            else if (sender == line4) { line4.Background = Brushes.White;}
            else { line5.Background = Brushes.White;}

            //Goes to next task on 'Line' click
            string line = sender.ToString().Substring(40,6).ToLower();           
            line_select = Convert.ToInt16(sender.ToString().Substring(45, 1));

            logData("(nav) " + line, true, false);
            showNavConfirm();
            clearNav();
            n_btn_none_count = 0;

            if(_Trial != _Max_Trials+1)
            {
                EasyTimer.SetTimeout(() =>
                {
                    next_task();

                    if (_Curr_Trial[0].Task == "Nav Easy" || _Curr_Trial[0].Task == "Nav Hard")
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if (_Trial <= _Max_Trials)
                            {
                                wiz_btn_none.IsEnabled = true;
                                wiz_btn_continue.IsEnabled = true;
                                logData("start trial " + _Trial, false, false);
                            }
                        }));
                    }
                }, 5000);
            }
               
        }

        //==================
        //Button Click Actions
        //==================
        private void wiz_btn_start_Click(object sender, RoutedEventArgs e)
        {
            subject_window.r_now_listening.Visibility = Visibility.Hidden;
            subject_window.radio_title.Visibility = Visibility.Hidden;
            clearNav();
            wiz_select.IsHitTestVisible = false;
            
            //displayLIST(_Curr_Trial);
            checkPromptAudio();
            
            
            if(_Curr_Trial[0].Task.Contains("Nav"))
            {
                Wiz_Tab.SelectedIndex = 0;
                subject_window.subj_tab.SelectedIndex = 0;
                
                if (_Curr_Trial[0].Mode.Contains("Audio"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        subject_window.audio_only.Visibility = Visibility.Visible;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        subject_window.audio_only.Visibility = Visibility.Hidden;
                    }));
                }
            }
            else if(_Curr_Trial[0].Task.Contains("Radio"))   //Radio Mode
            {
                updateTrial(1, _Curr_Task);
                displayLIST(_Curr_Trial);
                subject_window.subj_tab.SelectedIndex = 1;
            }
            else if(_Curr_Trial[0].Task.Contains("TDT"))
            {
                startTDTcountdown();
            }
            wiz_btn_start.IsEnabled = false;
            wiz_btn_none.IsEnabled = true;
            wiz_btn_continue.IsEnabled = true;

            _StartTime = DateTime.Now;
            logData("Start Run", false, false);           
        }

        private void PlayNavPrompt()
        {
            int duration = getAudioDuration("audio/chime.mp3");
            PlayAudioFile("audio/chime.mp3");
            EasyTimer.SetTimeout(() =>
            {
                PlayAudioFile("audio/n_where.mp3");
                logData("(nav) end prompt", false, false);
            }, duration + 100);    
        }

        private void next_task()
        {
            _Trial++;
            if (_Trial == _Max_Trials + 1)
            {
                //_Trial = 1;
                Dispatcher.Invoke(new Action(() =>
                {
                    Wiz_Tab.SelectedIndex = 3;
                    wiz_btn_continue.IsEnabled = false;
                    wiz_btn_none.IsEnabled = false;
                    logData("End Run", false, false);
                }));
            }
            else
            {
                updateTrial(_Trial, _Curr_Task);
                if (stopHandle != null) { stopHandle.Dispose(); }



                if (_Curr_Trial[0].Task.Contains("TDT"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Wiz_Tab.SelectedIndex = 2;
                        subject_window.subj_tab.SelectedIndex = 2;
                    }));
                    //PlayAudioFile("audio/tdt_prompt.mp3");
                    startTDTcountdown();
                }

                if (_Curr_Trial[0].Task == "Radio")
                {
                    updateTrial(_Trial, _Curr_Task);
                    checkPromptAudio();
                    displayLIST(_Curr_Trial);
                    r_cmd_wrong_count = 0;
                    r_cmd_repeat_count = 0;
                    r_cmf_wrong_count = 0;
                    r_cmf_repeat_count = 0;
                    r_complete_once = true;
                }
                if (_Curr_Trial[0].Task.Contains("Nav"))
                {
                    //PlayNavPrompt();
                    checkPromptAudio();
                }
            }
                
        }

        //Loads Subject Window from Wizard Window
        private void load_subj_Click(object sender, RoutedEventArgs e)
        {
            if(subject_window == null)
            {
                subject_window = new win_subject();
            } 
            else
            {
                if (subject_window.IsVisible)
                {
                    subject_window.Hide();
                }
                else if(subject_window != null)
                {
                    subject_window.Show();
                }
            }     
        }
       
        //Hide Border Button
        private void hide_border_Click(object sender, RoutedEventArgs e)
        {
            if(subject_window.WindowStyle != WindowStyle.None)
            {
                subject_window.WindowStyle = System.Windows.WindowStyle.None;
                subject_window.ResizeMode = System.Windows.ResizeMode.NoResize;   
            }
            else
            {
                subject_window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                subject_window.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            }
        }

        //Exit program - kills entire process
        protected override void OnClosing(CancelEventArgs e)
        {
            GC.Collect(); // Start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish
            Process.GetCurrentProcess().Kill();
        }
        
        private void wiz_btn_continue_Click(object sender, RoutedEventArgs e)
        {
            if (_Curr_Trial[0].Task == "Nav Easy" || _Curr_Trial[0].Task == "Nav Hard")
            {
                wiz_select.IsHitTestVisible = true;
                updateTrial(_Trial, _Curr_Task);
                displayLIST(_Curr_Trial);

                PlayAudioFile("audio/n_here.mp3");
                
                if(_Curr_Trial[0].Mode == "Audio Only" || _Curr_Trial[0].Mode == "Hybrid")
                {
                    int duration = getAudioDuration("audio/n_here.mp3");
                    string url = "audio/" + _Curr_Trial[0].N_Audio;
                    _Curr_Audio = url;
                    EasyTimer.SetTimeout(() =>
                    {
                        PlayAudioFile(url);
                    }, duration); //old duration was 1200 ms
                }
            }
            else if(_Curr_Trial[0].Task.Contains("TDT"))
            {
                next_task();
                Dispatcher.Invoke(new Action(() =>
                {
                    //next_task();
                    //checkPromptAudio();
                    wiz_btn_continue.IsEnabled = true;
                    wiz_btn_none.IsEnabled = true;

                    //Change Tab
                    int index;
                    if (_Curr_Trial[0].Task == "Radio") { index = 1; }
                    else { index = 0; }

                    Wiz_Tab.SelectedIndex = index;
                    subject_window.subj_tab.SelectedIndex = index;
                }));
            }
        }
        
        //None of the above button - Displays a message of available options which goes away after 2 seconds
        private void wiz_btn_none_Click(object sender, RoutedEventArgs e)
        {
            if(_Curr_Trial[0].Task != "Radio")
            {
                n_btn_none_count++;
                logData("btn-none of the above", false, false);

                if (wiz_select.IsHitTestVisible == false)
                {
                    PlayAudioFile("audio/n_where.mp3");
                }
                else
                {
                    if (n_btn_none_count < 3)
                    {
                        subject_window.wrong_command.Visibility = Visibility.Visible;
                        EasyTimer.SetTimeout(() =>
                        {
                            hideWrongCommand();
                        }, 5000);

                        if (_Curr_Task[0].Mode != "Visual Only")
                        {
                            PlayAudioFile("audio/btn_none_nav.mp3");
                        }

                        EasyTimer.SetTimeout(() =>
                        {
                            PlayAudioFile(_Curr_Audio);
                        }, 5200);
                    }
                    else
                    {
                        next_task();
                    }      
                }
            }
        }

        private void hideWrongCommand()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                subject_window.wrong_command.Visibility = Visibility.Hidden;
                subject_window.nav_confirm.Visibility = Visibility.Hidden;
            }));
        }

        public async void DelayedExecute(Action action, int delay)
        {
            await Task.Delay(delay);
            action();
        }

        

        private void btn_pid_submit_Click(object sender, RoutedEventArgs e)
        {
            _PID = txt_box_pid.Text;
            WIZ_BG.Background = Brushes.LightBlue;
            wiz_pid.Content = "PID: " + _PID;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboItem = sender as ComboBox;
            int index = comboItem.SelectedIndex;
            _Curr_Task = null;

            if (index == 1) { _Curr_Task = _Practice_Radio; }
            else if (index == 2) { _Curr_Task = _Practice_NAV; }
            else if (index == 3) { _Curr_Task = _Practice_Hyrbid_NAV; }
            else if (index == 4) { _Curr_Task = _Practice_Audio_NAV; }

            else if (index == 6) { _Curr_Task = _Main_Scen1A; _scenario = "1A"; }
            else if (index == 7) { _Curr_Task = _Main_Scen1B; _scenario = "1B"; }
            else if (index == 8) { _Curr_Task = _Main_Scen2A; _scenario = "2A"; }
            else if (index == 9) { _Curr_Task = _Main_Scen2B; _scenario = "2B"; }
            else if (index == 10) { _Curr_Task = _Main_Scen3A; _scenario = "3A"; }
            else if (index == 11) { _Curr_Task = _Main_Scen3B; _scenario = "3B"; }
            else if (index == 12) { _Curr_Task = _Main_Scen4A; _scenario = "4A"; }
            else if (index == 13) { _Curr_Task = _Main_Scen4B; _scenario = "4B"; }
            
            else if (index == 0) { _Curr_Task = _Debug_Trial; _scenario = "NA"; }   //For debugging only
            

            _Max_Trials = (from item in _Curr_Task select item.Trial).Max();
            _randTrial = new int[_Max_Trials];
            
            //Disable buttons if combobox selection is not valid
            List<int> iList = new List<int>();
            for (int i = 1; i < 5; i++ )
            {
                iList.Add(i);
            }   
            for (int i = 6; i < 14; i++)
            {
                iList.Add(i);
            }
            //iList.Add(0);  //For debugging only
            
            if(index < 5 && index >= 0)
            {
                _RunType = "Practice";
                for(int i=0; i < _Max_Trials; i++)
                {
                    _randTrial[i] = i + 1;
                }
            }
            else if(index > 5 && index <= iList.Max())
            {
                _RunType = "Main Run";
                //int[] block1 = shuffle(new[] { 1, 2, 3, 4, 5, 6 });
                //int[] tdt1 = new[] { 7 };
                //int[] block2 = shuffle(new[] { 8, 9, 10, 11, 12, 13 });
                //int[] tdt2 = new[] { 14 };
                //int[] block3 = shuffle(new[] { 15, 16, 17, 18, 19, 20 });
                //int[] tdt3 = new[] { 21 };
                //int[] block4 = shuffle(new[] { 22, 23, 24, 25, 26, 27 });

                //int[] combined = block1.Concat(tdt1.Concat(block2.Concat(tdt2.Concat(block3.Concat(tdt3.Concat(block4)))))).ToArray();
                //_randTrial = combined;

                int[] tdt1 = new[] { 1 };
                int[] block1 = shuffle(new[] { 2, 3, 4, 5, 6, 7 });
                int[] tdt2 = new[] { 8 };
                int[] block2 = shuffle(new[] { 9, 10, 11, 12, 13, 14 });
                int[] tdt3 = new[] { 15 };
                int[] combined = tdt1.Concat(block1.Concat(tdt2.Concat(block2.Concat(tdt3)))).ToArray();
                _randTrial = combined;
            }
           
            if (iList.Contains(index))
            {                
                List<restaurant> LISTrecord = new List<restaurant>();
                var query =
                        from record in _Curr_Task
                        //where record.Trial == 1
                        where record.Trial == _randTrial[0]
                        orderby record.Distance ascending
                        select record;

                foreach (var g in query)
                {
                    LISTrecord.Add(g);
                }

                _Curr_Trial = LISTrecord;
                _Trial = 1;
                updateTrial(_Trial, _Curr_Trial);
                wiz_btn_start.IsEnabled = true;
                logData("change scenario",false, false);
                
                //Populate Jump List Dropdown
                jump_box.Items.Clear();
                var query2 =
                    from record in _Curr_Task
                    select new {record.Trial,record.Mode, record.Task };

                //var distlist = query2.GroupBy(x => x.Trial).Select(y => y.First().Task);
                var distlist =
                    from record in query2
                    group record by record.Trial into g
                    select new { g.First().Mode, g.First().Task };

                int counter = 1;
                foreach(var item in distlist)
                {
                    jump_box.Items.Add(counter+": "+item.Mode+" || "+item.Task.Substring(0,3));
                    counter++;
                }

                //Create a file name
                if (_PID == null) { _PID = "xxx"; }
                var datestamp = DateTime.Now.ToString("yyyy-MM-dd");
                _logfilename = "logfiles/PID" + _PID + "_" + _RunType + "_" + datestamp + "_WOZ.csv";
                _logDataBackup = "backup/PID" + _PID + "_" + _RunType + "_" + datestamp + "_WOZ.csv";
                string header = "PID, Date, Time, RunType, Scenario, Trial, Task, Mode, InputEvent, ElapsedTime, Duration, N_OptimalChoice, R_FirstTry, Notes";

                if(!File.Exists(_logfilename) || !File.Exists(_logDataBackup) || !File.Exists("backup\\logdata_all.csv"))
                {
                    //Write Column Header and Create csv file 
                    writeEntry(_logfilename, header);
                    if (_RunType != "Practice")
                    {
                        writeEntry(_logDataBackup, header); //Backup folder
                        writeEntry("backup\\logdata_all.csv", header);
                    }
                }                
            }
        
        }

        private void PlayAudioFile(string url)
        {
            if (_Player != null && (int)_Player.playState == 3) //playState = 3 when media player is active
            {
                _Player.controls.stop();
                _Player.close();
            }
            _Player = new WMPLib.WindowsMediaPlayer();
            
            if(_Curr_Trial[0].Task == "Radio" && url.Contains("r_play"))
            {
                int new_volume = _Volume + 20;
                _Player.settings.volume = new_volume;
            }
            else
            {
                _Player.settings.volume = _Volume;
            }
            _Player.URL = url;  //Only need URL to play audio
            _Player.controls.play();
        }

        //Change Volume
        private void volume_submit_Click(object sender, RoutedEventArgs e)
        {
            int volume = Convert.ToInt16(volume_box.Text);
            _Player.settings.volume = Convert.ToInt16(volume);
            _Volume = volume;
        }

        //Temporary test button for debugging
        private void btn_debug_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private int[] shuffle(int[] numlist)
        {
            return numlist.OrderBy(n => Guid.NewGuid()).ToArray();
        }

        private void startTDTcountdown()
        {
            if(stopHandle != null)
            {
                stopHandle.Dispose();
            }
            
            displayLIST(_Curr_Trial);
            int startnum;
            string url;
            if (_Curr_Trial[0].Task.Contains("Long"))
            {
                startnum = 180;
                url = "audio/tdt_prompt_long.mp3";
            }
            else
            {
                startnum = _TDTcount;
                url = "audio/tdt_prompt.mp3";
            }

            int countdown = startnum;
            int duration = getAudioDuration(url);

            PlayAudioFile(url);
           
            Dispatcher.Invoke(new Action(() =>
            {
                wiz_tdt_label.Content = "TDT Time Remaining:";
                tdt_countdown.Content = countdown;
                subject_window.subj_tab.SelectedIndex = 2;
                Wiz_Tab.SelectedIndex = 2;
            }));

            EasyTimer.SetTimeout(() =>
            {
                PlayAudioFile("audio/chime.mp3");
                logData("chime", false, false);

                stopHandle = EasyTimer.SetInterval(() =>
                {
                    countdown--;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        tdt_countdown.Content = countdown;
                        //subject_window.subj_tab.SelectedIndex = 2;
                        if (Wiz_Tab.SelectedIndex != 2) { stopHandle.Dispose(); }
                    }));
                   
                }, 1000);

                //Action when TDT countdown is finished
                EasyTimer.SetTimeout(() =>
                {
                    stopHandle.Dispose();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        //next_task();
                        //checkPromptAudio();
                        wiz_btn_continue.IsEnabled = true;
                        wiz_btn_none.IsEnabled = true;
                        wiz_tdt_label.Content = "Press Continue to go to next trial";
                        //Change Tab
                        //int index;
                        //if (_Curr_Trial[0].Task == "Radio") { index = 1; }
                        //else { index = 0; }

                        //Wiz_Tab.SelectedIndex = index;
                        //subject_window.subj_tab.SelectedIndex = index;
                    }));
                }, startnum * 1000);
            },duration);    
        }
       
        private void logData(string input_event, bool n_optimal, bool r_firsttry)
        {
            if (File.Exists(@_logfilename))
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                var datestamp = DateTime.Now.ToString("M/d/yyyy");
                var elapsed_time = DateTime.Now.Subtract(_StartTime);
                var e_time = new DateTime(elapsed_time.Ticks).ToString("H:mm:ss.ff");
                var decimal_time = DateTime.Now.Subtract(_StartTime).TotalSeconds;
                string best_choice = "NA";
                string firstTry = "NA";
                
                decimal_time = Math.Round(decimal_time, 3);
                if(_Trial == 1)
                {
                    _TempTime = 0;
                }
                var task_duration = Math.Round(decimal_time - _TempTime,3);

                if((_Curr_Trial[0].Task == "Nav Easy" || _Curr_Trial[0].Task == "Nav Hard") && line_select != 0 && n_optimal == true && r_firsttry == false)
                {
                    best_choice = checkOptimalChoice(line_select);
                }
                else if(_Curr_Trial[0].Task == "Radio" && n_optimal == false && r_firsttry == true)
                {
                    if(r_cmd_wrong_count == 0 && r_cmf_wrong_count == 0)
                    {
                        firstTry = "yes";
                    }
                    else if(r_complete_once == false)
                    {
                        firstTry = "no";
                    }
                    else
                    {
                        firstTry = "no";
                    }
                }
                
                string delim = ",";   //Set delimiter
                string record = _PID + delim +
                                datestamp + delim +
                                timestamp + delim +
                                _RunType + delim +
                                _scenario + delim +
                                _Trial + delim +
                                _Curr_Trial[0].Task + delim +    
                                _Curr_Trial[0].Mode + delim +
                                input_event + delim +
                                e_time + delim +
                                task_duration + delim +
                                best_choice + delim +
                                firstTry + delim
                                ;
                
                //Write Record
                if(_Curr_Task != null)
                {                     
                    writeEntry(_logfilename, record);
                    writeEntry(_logAllData, record);  //AppData Roaming Directory
                   
                    if (_RunType != "Practice")
                    {
                        writeEntry("backup\\logdata_all.csv", record); //Backup folder for All data
                        writeEntry(_logDataBackup, record); //Backup for individual files
                    }
                    
                    
                }
                _TempTime = decimal_time;                
            }
            else
            {
                ////Create a file name
                //if (_PID == null) { _PID = "xxx"; }
                //var datestamp = DateTime.Now.ToString("yyyy-MM-dd");
                //_logfilename = "logfiles/PID" + _PID + "_" + _RunType + "_" + datestamp + "_WOZ.csv";
                //_logDataBackup = "backup/PID" + _PID + "_" + _RunType + "_" + datestamp + "_WOZ.csv";
                //string header = "PID, Date, Time, RunType, Scenario, Trial, Task, Mode, InputEvent, ElapsedTime, Duration, N_OptimalChoice, R_FirstTry, Notes";
                
                ////Write Column Header and Create csv file 
                //writeEntry(_logfilename, header);
                //if (_RunType != "Practice")
                //{
                //    writeEntry(_logDataBackup, header); //Backup folder
                //    writeEntry("backup\\logdata_all.csv", header);
                //}
            } 
        }

        private void writeEntry(string filepath, string record)
        {
            using (StreamWriter writer = new StreamWriter(@filepath, true)) 
            {
                writer.WriteLine(record);
            }
        }


        private string checkOptimalChoice(int num)
        {
            if(_Curr_Trial[0].Task != "Radio")
            {
                var best_choice = _Curr_Trial[num - 1].Score;
                if (best_choice == 1)
                {
                    return "yes";
                }
                else
                {
                    return "no";
                }
            }
            else
            {
                return "NA";
            }   
        }

        //-------------------
        //Radio Buttons
        //-------------------
        private void r_cmd_correct_Click(object sender, RoutedEventArgs e)
        {
            logData("r_command_correct", false, false);
            Dispatcher.Invoke(new Action(() =>
            {
                subject_window.r_now_listening.Visibility = Visibility.Visible;
                subject_window.radio_title.Visibility = Visibility.Visible;

                r_cmd_correct.IsEnabled = false;
                r_cmd_wrong.IsEnabled = false;
                r_cmd_repeat.IsEnabled = false;

                r_cmf_correct.IsEnabled = true;
                r_cmf_wrong.IsEnabled = true;
                r_cmf_repeat.IsEnabled = true;
            }));
            
            PlayAudioFile("audio/radio/" + _Curr_Trial[0].R_Audio_Confirm);
        }

        private void r_cmf_correct_Click(object sender, RoutedEventArgs e)
        {
            logData("r_confirm_correct", false, true);
            hideRadioPrompt();
            if(r_complete_once == true)
            {
                next_task();
                logData("next trial",false, false);
            }
            else
            {
                //Repeat Trial
                logData("catch user error",false, false);
                displayLIST(_Curr_Trial);
                checkPromptAudio();
                r_complete_once = true;
            }
        }

        private void r_cmd_wrong_Click(object sender, RoutedEventArgs e)
        {
            logData("r_command_incorrect",false, false);
            r_complete_once = false;
            r_cmd_wrong_count++;
            if (r_cmd_wrong_count + r_cmd_repeat_count < 3)
            {
                //Show Erroneous Radio Station
                Dispatcher.Invoke(new Action(() =>
                {
                    subject_window.radio_title.Content = _Curr_Trial[0].R_Name_Incorrect;

                    r_cmd_correct.IsEnabled = false;
                    r_cmd_wrong.IsEnabled = false;
                    r_cmd_repeat.IsEnabled = false;

                    r_cmf_correct.IsEnabled = true;
                    r_cmf_wrong.IsEnabled = true;
                    r_cmf_repeat.IsEnabled = true;

                    subject_window.r_now_listening.Visibility = Visibility.Visible;
                    subject_window.radio_title.Visibility = Visibility.Visible;
                }));

                PlayAudioFile("audio/radio/" + _Curr_Trial[0].R_Audio_Incorrect);
            }
            else
            {
                //Wrong too many times, skip to next trial
                logData("fail command stage: skip to next trial", false, true);
                
                r_cmd_wrong_count = 0;
                r_cmd_repeat_count = 0;
                r_cmf_wrong_count = 0;
                r_cmf_repeat_count = 0;
                r_complete_once = true;

                next_task();
                logData("next trial", false, false);
            }   
        }

        private void r_cmf_wrong_Click(object sender, RoutedEventArgs e)
        {
            logData("r_confirm_incorrect", false, false);
            r_cmf_wrong_count++;

            //Wrong Command + Wrong Confirm - automatically go to next task
            if(r_complete_once == false)
            {
                logData("fail entire trial", false, true);
                hideRadioPrompt();
                
                r_cmd_wrong_count = 0;
                r_cmd_repeat_count = 0;
                r_cmf_wrong_count = 0;
                r_cmf_repeat_count = 0;
                r_complete_once = true;
                
                logData("next trial", false, false);
                next_task();
            }
            else
            {
                //Correct Command + Wrong Confirm
                if(r_cmf_wrong_count + r_cmf_repeat_count >= 3)
                {
                    logData("fail confirm stage: skip to next trial", false, true);
                    hideRadioPrompt();

                    r_cmd_wrong_count = 0;
                    r_cmd_repeat_count = 0;
                    r_cmf_wrong_count = 0;
                    r_cmf_repeat_count = 0;
                    r_complete_once = true;
                    next_task();       
                }
                else
                {
                    //Repeat Trial
                    hideRadioPrompt();
                    checkPromptAudio();
                    displayLIST(_Curr_Trial);
                }
            }
        }

        private void r_cmd_repeat_Click(object sender, RoutedEventArgs e)
        {
            r_cmd_repeat_count++;

            logData("r_command_repeat", false, false);
            if (r_cmd_wrong_count + r_cmd_repeat_count < 3)
            {
                checkPromptAudio();
                displayLIST(_Curr_Trial);
            }
            else
            {
                logData("fail command stage: skip to next trial", false, true);

                r_cmd_wrong_count = 0;
                r_cmd_repeat_count = 0;
                r_cmf_wrong_count = 0;
                r_cmf_repeat_count = 0;
                r_complete_once = true;

                next_task();
                logData("next trial", false, false);
            }
        }

        private void r_cmf_repeat_Click(object sender, RoutedEventArgs e)
        {
            r_cmf_repeat_count++;
            logData("r_confirm_repeat", false, false);
            if(r_cmf_wrong_count + r_cmf_repeat_count < 3 && r_complete_once == true)
            {
                PlayAudioFile("audio/radio/" + _Curr_Trial[0].R_Audio_Confirm);
            }
            else if (r_cmf_wrong_count + r_cmf_repeat_count < 3 && r_complete_once == false)
            {
                PlayAudioFile("audio/radio/" + _Curr_Trial[0].R_Audio_Incorrect);
            }
            else
            {
                logData("fail confirm stage: skip to next trial", false, true);

                r_cmd_wrong_count = 0;
                r_cmd_repeat_count = 0;
                r_cmf_wrong_count = 0;
                r_cmf_repeat_count = 0;
                r_complete_once = true;

                hideRadioPrompt();
                next_task();
                logData("next trial", false, false);
            }

        }

        //Hides "You are now listening to:" and Radio Title
        //Also disables the Confirm Row buttons
        private void hideRadioPrompt()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                subject_window.r_now_listening.Visibility = Visibility.Hidden;
                subject_window.radio_title.Visibility = Visibility.Hidden;

                r_cmf_correct.IsEnabled = false;
                r_cmf_wrong.IsEnabled = false;
                r_cmf_repeat.IsEnabled = false;
            }));
        }

        private void clearNav()
        {
            l1_name.Content = "";
            l1_cuisine.Content = "";
            l1_distance.Content = "";
            l1_reviews.Content = "";
            l1_price.Content = "";
            l1_rating.Source = new BitmapImage(getImageURI(0));

            l2_name.Content = "";
            l2_cuisine.Content = "";
            l2_distance.Content = "";
            l2_reviews.Content = "";
            l2_price.Content = "";
            l2_rating.Source = new BitmapImage(getImageURI(0));

            l3_name.Content = "";
            l3_cuisine.Content = "";
            l3_distance.Content = "";
            l3_reviews.Content = "";
            l3_price.Content = "";
            l3_rating.Source = new BitmapImage(getImageURI(0));

            l4_name.Content =      ""; 
            l4_cuisine.Content =   ""; 
            l4_distance.Content =  ""; 
            l4_reviews.Content =   ""; 
            l4_price.Content =     ""; 
            l4_rating.Source = new BitmapImage(getImageURI(0));

            l5_name.Content =          "";
            l5_cuisine.Content =       "";
            l5_distance.Content =      "";
            l5_reviews.Content =       "";
            l5_price.Content =         "";
            l5_rating.Source = new BitmapImage(getImageURI(0));

            subject_window.sl1_name.Content =      ""; 
            subject_window.sl1_cuisine.Content =   ""; 
            subject_window.sl1_distance.Content =  ""; 
            subject_window.sl1_reviews.Content =   ""; 
            subject_window.sl1_price.Content =     ""; 
            subject_window.sl1_rating.Source = new BitmapImage(getImageURI(0));

            subject_window.sl2_name.Content =       "";
            subject_window.sl2_cuisine.Content =    "";
            subject_window.sl2_distance.Content =   "";
            subject_window.sl2_reviews.Content =    "";
            subject_window.sl2_price.Content =      "";
            subject_window.sl2_rating.Source = new BitmapImage(getImageURI(0));

            subject_window.sl3_name.Content =         "";
            subject_window.sl3_cuisine.Content =      "";
            subject_window.sl3_distance.Content =     "";
            subject_window.sl3_reviews.Content =      "";
            subject_window.sl3_price.Content =        "";
            subject_window.sl3_rating.Source = new BitmapImage(getImageURI(0));

            subject_window.sl4_name.Content =       "";
            subject_window.sl4_cuisine.Content =    "";
            subject_window.sl4_distance.Content =   "";
            subject_window.sl4_reviews.Content =    "";
            subject_window.sl4_price.Content =      "";
            subject_window.sl4_rating.Source = new BitmapImage(getImageURI(0));

            subject_window.sl5_name.Content =       "";
            subject_window.sl5_cuisine.Content =    "";
            subject_window.sl5_distance.Content =   "";
            subject_window.sl5_reviews.Content =    "";
            subject_window.sl5_price.Content =      "";
            subject_window.sl5_rating.Source = new BitmapImage(getImageURI(0));           
        }

        private void jump_box_btn_Click(object sender, RoutedEventArgs e)
        {
            int cmb_index = jump_box.SelectedIndex + 1;
            _Trial = cmb_index;

            subject_window.r_now_listening.Visibility = Visibility.Hidden;
            subject_window.radio_title.Visibility = Visibility.Hidden;
            clearNav();
            wiz_select.IsHitTestVisible = false;

            updateTrial(cmb_index, _Curr_Task);
            
            if(_Curr_Trial[0].Task.Contains("Radio"))
            {
                updateTrial(cmb_index, _Curr_Task);
                displayLIST(_Curr_Trial);
                subject_window.subj_tab.SelectedIndex = 1;
            }
            checkPromptAudio();


            if (_Curr_Trial[0].Task.Contains("Nav"))
            {
                Wiz_Tab.SelectedIndex = 0;
                subject_window.subj_tab.SelectedIndex = 0;

                if (_Curr_Trial[0].Mode.Contains("Audio"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        subject_window.audio_only.Visibility = Visibility.Visible;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        subject_window.audio_only.Visibility = Visibility.Hidden;
                    }));
                }
            }
            else if (_Curr_Trial[0].Task.Contains("Radio"))   //Radio Mode
            {
                //updateTrial(cmb_index, _Curr_Task);
                //displayLIST(_Curr_Trial);
                //subject_window.subj_tab.SelectedIndex = 1;
            }
            else if (_Curr_Trial[0].Task.Contains("TDT"))
            {
                startTDTcountdown();
            }
            wiz_btn_start.IsEnabled = false;
            wiz_btn_none.IsEnabled = true;
            wiz_btn_continue.IsEnabled = true;

            _StartTime = DateTime.Now;
            logData("Jump to trial "+cmb_index.ToString(), false, false);

            _Trial = cmb_index;
        }
            
        

        
        
        

        

        
  


        
        

        
       
    }
}
