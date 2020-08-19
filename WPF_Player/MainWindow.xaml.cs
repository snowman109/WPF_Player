using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing;
using System.Windows.Threading;
using Echevil;
using NLog;

namespace WPF_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenu notifyiconMnu;
        private DispatcherTimer myTimer = null;
        private Boolean move1 = true;
        private DirectoryInfo info;
        readonly Dictionary<string, short> hotKeyDic = new Dictionary<string, short>();
        private int count = 0;
        private int MAXBUFFERTIME = 10;
        NeateaseAPI.API api = new NeateaseAPI.API();
        MyPlayer player = new MyPlayer();
        MediaPlayer playerhandle = null;
        PlayerStatus currentStatus = PlayerStatus.NerverStart;
        ObservableCollection<Song> playList = new ObservableCollection<Song>(); // 播放列表
        Dictionary<String, String> list = new Dictionary<String, String>(); // 歌单id和名字
        List<Song> songsOfList = new List<Song>(); // 当前歌单歌曲
        BindingSource bsSong = new BindingSource();
        string currentListId = null;
        private int currentIndex;
        string mode;
        String span;
        private List<string> playMode = new List<string>(); // 0顺序播放，1随机播放，2单曲循环
        string defaultPath = "D://timerconfig//";
        string defaultUid = "517377194";
        string folderPath;
        Logger logger = NLog.LogManager.GetLogger("wpf_player");
        private NetworkAdapter[] adapters;
        string currentPath = System.Environment.CurrentDirectory + "\\setting.ini";
        private NetworkMonitor monitor;
        public MainWindow()
        {
            logger.Info("程序启动，开始初始化：");
            InitializeComponent();
            this.Loaded += image_Loaded;
            this.Loaded += Window_Loaded;
            this.Loaded += (sender, e) =>
            {
                hotkey();
            };
            logger.Info("热键初始化成功！");
            playMode.Add("order");
            playMode.Add("shuffle");
            playMode.Add("once");
            mode = playMode[0];
            this.Loaded += FormMain_Load;
            logger.Info("网络部件初始化成功！");
            this.ShowInTaskbar = false;
            myTimer = new DispatcherTimer();  //实例化定时器
            //设置定时器属性
            myTimer.Interval = new TimeSpan(0, 0, 1);  //创建时分秒
            myTimer.Tick += new EventHandler(Timer_Tick);
            // 启动定时器
            Read();
            logger.Info("壁纸图片读取成功！");
            initArgs();
            logger.Info("参数初始化成功！");
            WinPosition();
            Initializenotifyicon();
            logger.Info("设置初始化成功！");
            myTimer.Start();
            logger.Info("计时器初始化成功！");
        }

        private void initArgs()
        {
            playerhandle = player.GetPlayerHandle();
            playerhandle.MediaFailed += playerhandle_MediaFailed;
            player.playEvent += player_playEvent;
            span = "0:0:0";
            if (File.Exists(defaultPath + "save.json"))
            {
                StreamReader sr = new StreamReader(defaultPath + "save.json", Encoding.UTF8);
                String line;
                StringBuilder sb = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                JObject save = JObject.Parse(sb.ToString());
                sr.Close();
                if (!File.Exists(defaultPath + "listId.ini"))
                {
                    GetList(defaultPath);
                }
                sr = new StreamReader(defaultPath + "listId.ini", Encoding.UTF8);
                currentListId = sr.ReadLine();
                sr.Close();
                Boolean success = true;
                if (currentListId.Equals(save["currentListId"].ToString()))
                {
                    logger.Info("读取保存文件ing");
                    list = save["list"].ToObject<Dictionary<string, string>>();
                    if (list == null || list.Count == 0)
                    {
                        logger.Error("读取保存内容失败，歌单初始化失败，歌单为空");
                        success = false;
                    }
                    playList = save["playList"].ToObject<ObservableCollection<Song>>();
                    if (playList == null || playList.Count == 0)
                    {
                        success = false;
                        logger.Error("读取保存内容失败，播放列表初始化失败，播放列表为空");
                    }
                    songsOfList = save["songsOfList"].ToObject<List<Song>>();
                    if (songsOfList == null || songsOfList.Count == 0)
                    {
                        success = false;
                        logger.Error("读取保存内容失败，歌单详情初始化失败，歌单详情为空");
                    }
                    currentIndex = int.Parse(save["currentIndex"].ToString());
                    if (currentIndex >= playList.Count)
                    {
                        currentIndex = 0;
                        logger.Error("读取保存内容失败，当前播放索引初始化失败，长度超过列表长度，已初始化为0");
                    }
                    span = save["position"].ToString();
                    if (span == null || span.Length == 0)
                    {
                        span = "00:00:00.0";
                    }
                }
                else
                {
                    logger.Error("读取保存内容失败，保存的歌单id和设置的歌单id不同");
                    success = false;
                }
                if (!success)
                {
                    list.Clear();
                    playList.Clear();
                    songsOfList.Clear();
                    GetList(defaultPath);
                    GetMP3Files(defaultPath, currentListId).ToList().ForEach(x => playList.Add(x));
                    currentIndex = 0;
                    span = "00:00:00.0";
                    logger.Info("读取保存内容失败，正在初始化配置");
                }
                mode = save["mode"].ToString();
                if (mode == null || mode.Length == 0)
                {
                    mode = playMode[0];
                }
                logger.Info("当前歌单为:" + list[currentListId] + ";当前歌曲为:" + playList[currentIndex].Name + ";当前播放进度为:" + span + ";当前播放模式为:" + mode);
                string _move1 = save["move1"].ToString();
                if (_move1 == null || _move1.Length == 0)
                {
                    _move1 = "true";
                }
                move1 = bool.Parse(_move1);
            }
            else
            {
                GetList(defaultPath);
                GetMP3Files(defaultPath, currentListId).ToList().ForEach(x => playList.Add(x));
                mode = playMode[0];
                move1 = true;
            }
            bsSong.DataSource = songsOfList;
            this.listBox.ItemsSource = bsSong;
            listBox.DisplayMemberPath = "Name";
            listBox.SelectedValuePath = "Location";
            showMusic();
        }
        private void showMusic()
        {
            Song song;
            if (currentIndex >= playList.Count)
            {
                song = playList[0];
            }
            else
            {
                song = playList[currentIndex];
            }
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = song;
            playerhandle.Open(new Uri(song.Location));
            player.Pause();
            currentStatus = PlayerStatus.Pause;
            this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPlay"]);
            var now = DateTime.Now;
            while (!playerhandle.NaturalDuration.HasTimeSpan)
            {
                if (DateTime.Now.Subtract(now).TotalSeconds > 10)
                {
                    break;
                }
            }
            String[] times = span.Split(':');
            playerhandle.Position = new TimeSpan(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2].Split('.')[0]));
            if (playerhandle != null && playerhandle.NaturalDuration.HasTimeSpan)
            {
                this.lable.Content = string.Format("{0}/{1}", playerhandle.Position.ToString("mm\\:ss"), playerhandle.NaturalDuration.TimeSpan.ToString("mm\\:ss"));
            }

        }

        //API
        [DllImport("user32")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32")]
        private static extern IntPtr FindWindowEx(IntPtr par1, IntPtr par2, String par3, String par4);
        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        // 注册热键
        private void hotkey()
        {
            var wpfHwnd = new WindowInteropHelper(this).Handle;
            var hWndSource = HwndSource.FromHwnd(wpfHwnd);
            //添加处理程序
            if (hWndSource != null) hWndSource.AddHook(MainWindowProc);
            try
            {
                hotKeyDic.Add("Ctrl-P", Win32.GlobalAddAtom("Ctrl-P"));
                hotKeyDic.Add("Ctrl-Next", Win32.GlobalAddAtom("Ctrl-Next"));
                hotKeyDic.Add("Ctrl-Pre", Win32.GlobalAddAtom("Ctrl-Pre"));
                Win32.RegisterHotKey(wpfHwnd, hotKeyDic["Ctrl-P"], Win32.KeyModifiers.Ctrl | Win32.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.P);
                Win32.RegisterHotKey(wpfHwnd, hotKeyDic["Ctrl-Next"], Win32.KeyModifiers.Ctrl | Win32.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.Right);
                Win32.RegisterHotKey(wpfHwnd, hotKeyDic["Ctrl-Pre"], Win32.KeyModifiers.Ctrl | Win32.KeyModifiers.Alt, (int)System.Windows.Forms.Keys.Left);
            }
            catch (System.ArgumentException)
            {

            }
                
        }
        /// <summary>
        /// 将程序嵌入桌面
        /// </summary>
        /// <returns></returns>
        private IntPtr GetDesktopPtr()//寻找桌面的句柄
        {
            // 情况一
            IntPtr hwndWorkerW = IntPtr.Zero;
            IntPtr hShellDefView = IntPtr.Zero;
            IntPtr hwndDesktop = IntPtr.Zero;
            IntPtr hProgMan = FindWindow("Progman", "Program Manager");
            if (hProgMan != IntPtr.Zero)
            {
                hShellDefView = FindWindowEx(hProgMan, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hShellDefView != IntPtr.Zero)
                {
                    hwndDesktop = FindWindowEx(hShellDefView, IntPtr.Zero, "SysListView32", null);
                }
            }
            if (hwndDesktop != IntPtr.Zero) return hwndDesktop;

            // 情况二
            //必须存在桌面窗口层次
            while (hwndDesktop == IntPtr.Zero)
            {
                //获得WorkerW类的窗口
                hwndWorkerW = FindWindowEx(IntPtr.Zero, hwndWorkerW, "WorkerW", null);
                if (hwndWorkerW == IntPtr.Zero) break;
                hShellDefView = FindWindowEx(hwndWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hShellDefView == IntPtr.Zero) continue;
                hwndDesktop = FindWindowEx(hShellDefView, IntPtr.Zero, "SysListView32", null);
            }
            return hwndDesktop;
        }

        private void FormMain_Load(object sender, System.EventArgs e)
        {
            monitor = new NetworkMonitor();
            this.adapters = monitor.Adapters;
        }

        private void TimerCounter_Tick()
        {
            string down = "kB/s";
            string up = "KB/s";
            double downloadSpeed = 0;
            double uploadSpeed = 0;
            if (adapters == null)
            {
                return;
            }
            for (int i = 0; i < adapters.Length; i++)
            {
                monitor.StartMonitoring();
                //Console.WriteLine(adapters[i].DownloadSpeedKbps);
                downloadSpeed += adapters[i].DownloadSpeedKbps;
                uploadSpeed += adapters[i].UploadSpeedKbps;
            }
            //// 转换成KB
            //downloadSpeed /= 8;
            //uploadSpeed /= 8;
            // 转换成MB
            if (downloadSpeed > 1024)
            {
                downloadSpeed /= 1024;
                down = "MB/s";
            }
            if (uploadSpeed > 1024)
            {
                uploadSpeed /= 1024;
                up = "MB/s";
            }
            this.Download.Content = "↓ " + downloadSpeed.ToString("0.00") + down;
            this.Upload.Content = "↑ " + uploadSpeed.ToString("0.00") + up;
            //this.LableDownloadValue.Text = String.Format("{0:n} kbps", adapter.DownloadSpeedKbps);
            //this.LabelUploadValue.Text = String.Format("{0:n} kbps", adapter.UploadSpeedKbps);
        }
        /// <summary>
        /// 响应快捷键事件
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WmHotkey:
                    {
                        int sid = wParam.ToInt32();
                        if (sid == hotKeyDic["Ctrl-P"])
                        {
                            Play();
                        }
                        else if (sid == hotKeyDic["Ctrl-Next"])
                        {
                            PlayNext();
                        }
                        else if (sid == hotKeyDic["Ctrl-Pre"])
                        {
                            PlayPrevious();
                        }
                        handled = true;
                        break;
                    }
            }

            return IntPtr.Zero;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetParent(new WindowInteropHelper(this).Handle, GetDesktopPtr());

        }
        //private void acquireList(String folderName)
        //{
        //    String uId = "366579709";
        //    if (!Directory.Exists(folderName)) {
        //        Directory.CreateDirectory(folderName);
        //        throw new Exception("文件夹:" + folderName + "不存在");
        //    }
        //    if (File.Exists(folderName + "userId.ini"))
        //    {
        //        StreamReader sr = new StreamReader(folderName + "userId.ini", Encoding.Default);
        //        if (sr.ReadLine() != null)
        //        {
        //            if (sr.ReadLine().Trim() != "")
        //            {
        //                uId = sr.ReadLine().Trim();
        //            }
        //        }    
        //    }
        //    else
        //    {
        //        StreamWriter sw = System.IO.File.CreateText(folderName + "userId.ini");
        //        sw.WriteLine(uId);
        //    }
        //    System.Diagnostics.Process.Start("java.EXE", "-jar NeteaseApi.jar "+uId);
        //}
        private void GetList(String folderName)
        {
            if (!File.Exists(folderName + "playlist.json"))
            {
                System.Windows.MessageBox.Show("请提前运行小工具获得歌单,如已运行请查看D盘是否存在timerconfig配置文件夹", "获取歌单失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (System.IO.StreamReader file = System.IO.File.OpenText(folderName + "playlist.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    foreach (JProperty jProperty in o.Properties())
                    {
                        //Console.WriteLine(jProperty.Name.ToString()+":"+ jProperty.Value.ToString());
                        list.Add(jProperty.Name.ToString(), jProperty.Value.ToString());
                        if (jProperty.Value.ToString().Contains("喜欢的音乐"))
                        {
                            currentListId = jProperty.Name.ToString();
                        }
                    }
                    if (currentListId == null)
                    {
                        currentListId = list.Keys.ToList()[0];
                    }
                }
            }
            if (File.Exists(folderName + "listId.ini"))
            {
                StreamReader sr = new StreamReader(folderName + "listId.ini", Encoding.Default);
                String lid = sr.ReadLine();
                sr.Close();
                if (lid != null)
                {
                    if (lid.Trim() != "")
                    {
                        currentListId = lid.Trim();
                    }
                    else
                    {
                        StreamWriter sw = System.IO.File.CreateText(folderName + "listId.ini");
                        sw.WriteLine(currentListId);
                    }
                }
                else
                {
                    StreamWriter sw = System.IO.File.CreateText(folderName + "listId.ini");
                    sw.WriteLine(currentListId);
                    sw.Close();
                }
            }
            else
            {
                StreamWriter sw = System.IO.File.CreateText(folderName + "listId.ini");
                sw.WriteLine(currentListId);
                sw.Close();
            }
        }
        void player_playEvent(object sender)
        {

            if (!playerhandle.NaturalDuration.HasTimeSpan)
            {
                count++;
                if (count > MAXBUFFERTIME)
                {
                    count = 0;
                    logger.Info("播放" + playList[currentIndex].Name + "失败，没有版权");
                    PlayNext();
                }
                return;
            }
            count = 0;
            double PlayedTime = playerhandle.Position.TotalSeconds;

            double totalTime = playerhandle.NaturalDuration.TimeSpan.TotalSeconds;
            //Console.WriteLine("这是总时间" + playerhandle.Source);
            this.lable.Content = string.Format("{0}/{1}", playerhandle.Position.ToString("mm\\:ss"), playerhandle.NaturalDuration.TimeSpan.ToString("mm\\:ss"));
            if (PlayedTime == totalTime)
            {
                // 不是单曲循环时才下一首开始
                if (!mode.Equals(playMode[2]))
                {
                    PlayNext();
                }
                else
                {
                    PlayThis();
                }
                return;
            }

            //    this.trackBar.MaxValue = totalTime;
            //    this.trackBar.CurrentValue = PlayedTime;
            //    lastValue = (int)this.trackBar.CurrentValue;
            //}
        }

        private void PlayThis()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            player.Play();
            logger.Info("开始播放:" + player.CurrentSong.Name);
            currentStatus = PlayerStatus.Start;
        }

        void playerhandle_MediaFailed(object sender, ExceptionEventArgs e)
        {

        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Play()
        {
            //1,暂停播放
            //2,从一首新歌播放
            if (currentStatus == PlayerStatus.Pause)
            {
                Song song = this.listBox.SelectedItem as Song;
                currentIndex = this.listBox.SelectedIndex;
                if (song != player.CurrentSong)
                    player.CurrentSong = song;
                player.Play();
                logger.Info("继续播放:" + song.Name);
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause_new"]);
            }
            else if (currentStatus == PlayerStatus.NerverStart)
            {
                Song song;
                if (this.listBox.SelectedIndex < 0)
                {
                    if (playList.Count > 0)
                    {
                        song = playList[0];
                        currentIndex = 0;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    song = this.listBox.SelectedItem as Song;
                    currentIndex = this.listBox.SelectedIndex;
                }
                this.listBox.SelectedIndex = currentIndex;
                player.CurrentSong = song;
                player.Play();
                logger.Info("开始播放:" + song.Name);
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause_new"]);
            }
            else if (currentStatus == PlayerStatus.Start)
            {
                player.Pause(); 
                logger.Info("暂停:" + player.CurrentSong.Name);
                currentStatus = PlayerStatus.Pause;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPlay"]);
            }
        }
        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayPrevious();
        }

        void PlayPrevious()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            acquireIndex(Const.PLAY_PRE);
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            logger.Info("播放上一曲:" + player.CurrentSong.Name);
            player.Play();
            currentStatus = PlayerStatus.Start;
        }
        void PlayNext()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            acquireIndex(Const.PLAY_NEXT);
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            logger.Info("播放下一曲:" + player.CurrentSong.Name);
            player.Play();
            currentStatus = PlayerStatus.Start;

        }

        // 获得下一首音乐的index
        private void acquireIndex(String nOrP)
        {
            // 随机播放
            if (mode.Equals(playMode[1]))
            {
                currentIndex = new Random(Guid.NewGuid().GetHashCode()).Next(0, songsOfList.Count - 1);
            }
            else if (mode.Equals(playMode[0]) || mode.Equals(playMode[2])) // 顺序播放
            {
                if (nOrP.Equals(Const.PLAY_NEXT))
                {
                    currentIndex++;
                    if (currentIndex > playList.Count - 1)
                        currentIndex = 0;
                }
                else if (nOrP.Equals(Const.PLAY_PRE))
                {
                    currentIndex--;
                    if (currentIndex < 0)
                        currentIndex = playList.Count - 1;
                }
            }
            // 单曲循环不在这里控制
        }
        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayNext();
        }
        enum PlayerStatus
        {
            NerverStart = 1,
            Start,
            Pause,
            Stop
        }

        private List<Song> GetMP3Files(string folderName, string listID)
        {
            //Console.WriteLine("当前listId:" + listID);
            if (!File.Exists(folderName + "songs.json"))
            {
                System.Windows.MessageBox.Show("请提前运行小工具获得歌单,如已运行请查看D盘是否存在timerconfig配置文件夹", "获取歌曲失败", MessageBoxButton.OK, MessageBoxImage.Error);
                //return new List<Song>(); 
                //throw new Exception("文件夹:" + folderName + "不存在！");
            }
            songsOfList.Clear();
            using (System.IO.StreamReader file = System.IO.File.OpenText(folderName + "songs.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    JToken songs = o.GetValue(listID);
                    foreach (JToken child in songs.Children())
                    {
                        songsOfList.Add(new Song() { Location = "http://music.163.com/song/media/outer/url?id=" + child["id"].ToString(), Name = child["name"].ToString() });
                    }
                }
            }
            return songsOfList;
        }

        private void ListBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            var value = listBox.SelectedValue;
            if (value != null)
            {
                currentIndex = this.listBox.SelectedIndex;
                player.SongPath = value.ToString();
                player.CurrentSong = playList[currentIndex];
                player.Play();
                logger.Info("开始播放:" + player.CurrentSong.Name);
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause"]);
            }
        }

        private void trackBar_PlayProcessChanged(double obj)
        {
            if (currentStatus == PlayerStatus.Start || currentStatus == PlayerStatus.Pause)
            {
                playerhandle.Position = TimeSpan.FromSeconds(obj);
            }
        }

        private void Timer_Tick(Object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            this.time.Content = transTime(now.Hour.ToString()) + ":" + transTime(now.Minute.ToString()) + ":" + transTime(now.Second.ToString());
            TimerCounter_Tick();
            // 每半分钟存一下状态并且注册一下热键
            if (now.Second % 30 == 0)
            {
                hotkey();
                saveArgs();
            }
        }

        // 将2改成02
        private String transTime(String time)
        {
            return time.Length == 1 ? ("0" + time) : time;
        }
        private int getDays(DateTime now)
        {
            int days = 0;
            int[] dayOfYear = getDayOfYear(now);
            days += dayOfYear[now.Month - 1] - now.Day;
            for (int i = now.Month; i < 12; i++)
            {
                if (i == 11)
                {
                    days += 22;
                }
                else
                {
                    days += dayOfYear[i];
                }
            }
            return days;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (move1) { this.DragMove(); }
        }
        int pictureNums = 0;
        private void Grid_MainTitle_MouseDown(object sender, MouseButtonEventArgs e)

        {
            //this.move = !move;
            FileInfo[] files = info.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(s => s.FullName.EndsWith(".png") || s.FullName.EndsWith(".jpg") || s.FullName.EndsWith(".jpeg") || s.FullName.EndsWith(".bmp")).ToArray();
            for (int i = pictureNums; i < files.Length; i++)
            {
                FileInfo f = files[i];
                pictureNums = i + 1;
                if (pictureNums >= files.Length)
                {
                    pictureNums = 0;
                }
                if (SetDestPicture(f.FullName))
                {
                    logger.Info("更换壁纸成功");
                    break;
                }
            }
        }
        //string[] words = new string[6];
        //string[] explans = new string[6];
        public void init()
        {
            Dictionary<String, String> set;
            if (!File.Exists(currentPath))
            {
                set = defaultSets();
                StreamWriter writer = File.CreateText(currentPath);
                writer.WriteLine(JObject.FromObject(set));
                writer.Flush();
                writer.Close();
            }
            else
            {
                StreamReader reader = new StreamReader(currentPath, Encoding.UTF8);
                String line;
                StringBuilder sb = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                set = JObject.Parse(sb.ToString()).ToObject<Dictionary<String, String>>();
            }
            folderPath = set["path"] == null ? defaultPath : set["path"];
            string uidPath = folderPath + "uid.ini";
            string playListPath = folderPath + "playlist.json";
            string songsListPath = folderPath + "songs.json";
            string listIdPath = folderPath + "listid.ini";
            if (!File.Exists(uidPath))
            {
                StreamWriter writer = File.CreateText(uidPath);
                writer.WriteLine(set["uid"] == null ? defaultUid : set["uid"]);
                writer.Flush();
                writer.Close();
            }
            if (!File.Exists(playListPath) || !File.Exists(listIdPath))
            {
                _getList();
            }
            else if (!File.Exists(songsListPath))
            {
                _getSongs();
            }
        }
        /**
         * 查看D盘有没有picture文件夹，该文件夹用于存放壁纸。
         */
        public void Read()
        {
            if (!Directory.Exists(@"D:\picture\"))
            {
                info = System.IO.Directory.CreateDirectory(@"D:\picture\");
            }
            else
            {
                info = new DirectoryInfo(@"D:\picture\");
                //Console.WriteLine("yes");
            }

        }
        private void _getList()
        {
            api.getUserListAsync(folderPath);

        }
        private void _getSongs()
        {
            api.getSongs(folderPath);
        }
        private Dictionary<String, String> defaultSets()
        {
            Dictionary<String, String> set = new Dictionary<string, string>();
            set.Add("uid", defaultUid);
            set.Add("path", defaultPath);
            return set;
        }
        private int[] getDayOfYear(DateTime now)
        {
            if (DateTime.IsLeapYear(now.Year))
            {
                int[] dayOfMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                return dayOfMonth;
            }
            else
            {
                int[] dayOfMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                return dayOfMonth;
            }
        }

        public void WinPosition()
        {
            double ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;//WPF
            double ScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//WPF
            this.Top = this.Height * 1.4;
            this.Left = ScreenWidth - this.Width - 1;
            //this.Top = 0;
            //this.Left = 0;
        }
        private void Initializenotifyicon()
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            Stream stream = assem.GetManifestResourceStream(
             "WPF_Player." + "Tray.ico");

            TrayIcon = new NotifyIcon();
            TrayIcon.Icon = new Icon(stream);

            TrayIcon.Text = "Timer " + "Copyright: Snowman";
            TrayIcon.Visible = true;
            TrayIcon.Click += new System.EventHandler(this.click);

            //tray menu
            System.Windows.Forms.MenuItem[] mnuItms = new System.Windows.Forms.MenuItem[11];
            mnuItms[0] = new System.Windows.Forms.MenuItem();
            mnuItms[0].Text = "Aways Top";
            mnuItms[0].Click += new System.EventHandler(this.TopAllmost);
            mnuItms[0].Checked = !this.Topmost;

            mnuItms[1] = new System.Windows.Forms.MenuItem("-");

            mnuItms[2] = new System.Windows.Forms.MenuItem();
            mnuItms[2].Text = "Locked";
            mnuItms[2].Click += new System.EventHandler(this.Locked);
            mnuItms[2].Checked = !move1;

            mnuItms[3] = new System.Windows.Forms.MenuItem("-");

            mnuItms[4] = new System.Windows.Forms.MenuItem();
            mnuItms[4].Text = "Settings...";
            mnuItms[4].Click += new System.EventHandler(this.Settings);

            mnuItms[5] = new System.Windows.Forms.MenuItem("-");

            mnuItms[6] = new System.Windows.Forms.MenuItem();
            mnuItms[6].Text = "Reload";
            mnuItms[6].Click += new System.EventHandler(this.RelodMusic);

            mnuItms[7] = new System.Windows.Forms.MenuItem("-");

            mnuItms[8] = new System.Windows.Forms.MenuItem();
            mnuItms[8].Text = mode;
            mnuItms[8].Click += new System.EventHandler(this.ChangePlayMode);

            mnuItms[9] = new System.Windows.Forms.MenuItem("-");

            mnuItms[10] = new System.Windows.Forms.MenuItem();
            mnuItms[10].Text = "Exit";
            mnuItms[10].Click += new System.EventHandler(this.ExitSelect);
            mnuItms[10].DefaultItem = true;



            notifyiconMnu = new System.Windows.Forms.ContextMenu(mnuItms);
            TrayIcon.ContextMenu = notifyiconMnu;
        }

        private void ChangePlayMode(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = (System.Windows.Forms.MenuItem)sender;
            int index = playMode.IndexOf(item.Text);
            item.Text = playMode[(index + 1) % playMode.Count()];
            mode = item.Text;
            logger.Info("更换播放模式为:" + mode);
        }

        private void Locked(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = (System.Windows.Forms.MenuItem)sender;
            bool check = item.Checked;
            item.Checked = !check;
            this.move1 = !item.Checked;
            if (move1)
            {
                logger.Info("窗口解锁成功");
            }
            else
            {
                logger.Info("窗口锁定成功");
            }
        }

        private void Settings(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.EXE", defaultPath + "listId.ini");
        }

        public void click(object sender, System.EventArgs e)
        {
            this.Activate();
        }
        public void TopAllmost(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = (System.Windows.Forms.MenuItem)sender;
            bool check = item.Checked;
            item.Checked = !check;
            this.Topmost = !item.Checked;
            if (this.Topmost)
            {
                logger.Info("窗口取消置顶");
            }
            else
            {
                logger.Info("窗口置顶成功");
            }
        }
        public void ExitSelect(object sender, System.EventArgs e)
        {
            TrayIcon.Visible = false;
            saveArgs();
            logger.Info("保存参数成功!");
            this.Close();
            logger.Info("关闭成功!");
            
        }

        private void saveArgs()
        {
            JObject save = new JObject();
            save.Add("currentIndex", JToken.FromObject(currentIndex));
            save.Add("mode", JToken.FromObject(mode));
            save.Add("currentListId", JToken.FromObject(currentListId));
            save.Add("move1", JToken.FromObject(move1));
            save.Add("list", JToken.FromObject(list));
            save.Add("songsOfList", JToken.FromObject(songsOfList));
            save.Add("playList", JToken.FromObject(playList));
            save.Add("position", playerhandle.Position);
            StreamWriter sw = System.IO.File.CreateText(defaultPath + "save.json");
            sw.WriteLine(save.ToString());
            sw.Flush();
            sw.Close();
        }

        public void RelodMusic(object sender, System.EventArgs e)
        {
            StreamReader listIdReader = new StreamReader(defaultPath + "listId.ini");
            String lid = listIdReader.ReadLine();
            listIdReader.Close();
            // 如果读出来是空，则把现在的写入
            if (lid == null || lid.Trim() == "")
            {
                logger.Info("重载音乐，但listId为空");
                StreamWriter sw = System.IO.File.CreateText(defaultPath + "listId.ini");
                sw.WriteLine(currentListId);
                sw.Flush();
                sw.Close();
            }
            // 如果两个不相等 先暂停，全部清空。
            else if (lid != currentListId)
            {
                if (currentStatus == PlayerStatus.Start)
                {
                    Play();
                }
                this.currentStatus = PlayerStatus.NerverStart;
                this.span = "0:0:0";
                this.playList.Clear();
                currentListId = lid;
                GetMP3Files(defaultPath, currentListId).ToList().ForEach(x => playList.Add(x));
                bsSong.ResetBindings(false);
                currentIndex = 0;
                showMusic();
                saveArgs();
                logger.Info("重载音乐，已将歌单更新为" + list[lid]);
            }
            else
            {
                logger.Info("重载音乐，但歌单无变化");
            }
        }
        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion

        private void image_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_NOZORDER = 0x0004;
        const int WM_ACTIVATEAPP = 0x001C;
        const int WM_ACTIVATE = 0x0006;
        const int WM_SETFOCUS = 0x0007;
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const int WM_WINDOWPOSCHANGING = 0x0046;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
           IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern IntPtr BeginDeferWindowPos(int nNumWindows);
        [DllImport("user32.dll")]
        static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);


        //private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == WM_SETFOCUS)
        //    {
        //        IntPtr hWnd2 = new WindowInteropHelper(this).Handle;
        //        SetWindowPos(hWnd2, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        //        handled = true;
        //    }
        //    return IntPtr.Zero;
        //}

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
        //    HwndSource src = HwndSource.FromHwnd(windowHandle);
        //    src.RemoveHook(new HwndSourceHook(this.WndProc));
        //}

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(
                    int uAction,
                    int uParam,
                    string lpvParam,
                    int fuWinIni
                    );

        /// <summary>
        /// 设置背景图片
        /// </summary>
        /// <param name="picture">图片路径</param>
        private Boolean SetDestPicture(string picture)
        {
            if (File.Exists(picture))
            {
                if (System.IO.Path.GetExtension(picture).ToLower() != "bmp" || System.IO.Path.GetExtension(picture).ToLower() != "jpg" || System.IO.Path.GetExtension(picture).ToLower() != "png" || System.IO.Path.GetExtension(picture).ToLower() != "jpeg")
                {
                    // 其它格式文件先转换为bmp再设置
                    //string tempFile = @"D:\test.bmp";
                    //System.Drawing.Image image = System.Drawing.Image.FromFile(picture);
                    //image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Bmp);
                    //picture = tempFile;
                    SystemParametersInfo(20, 0, picture, 0x2);
                    return true;
                }
            }
            return false;
        }
    }
}
