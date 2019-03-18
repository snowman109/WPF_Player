using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace Common.WPF
{
    /// <summary>
    /// RollingText.xaml 的交互逻辑
    /// </summary>
    public partial class RollingText : UserControl,INotifyPropertyChanged
    {
        public RollingText()
        {
            InitializeComponent();
            
        }
        #region 绑定属性
        private string text = "";
        /// <summary>
        /// 滚动字幕的内容
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; OnPropertyChanged("Text"); sb.Stop();sb.Begin(); }
        }
        private int fSize = 10;
        /// <summary>
        /// 字幕的字体
        /// </summary>
        public int FSize
        {
            get { return fSize; }
            set { fSize = value; OnPropertyChanged("FSize"); }
        }
        private SolidColorBrush fColor = new SolidColorBrush(Colors.Blue);
        public SolidColorBrush FColor
        {
            get { return fColor; }
            set { this.fColor = value; OnPropertyChanged("FColor"); }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        Storyboard sb = new Storyboard();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            double w=this.ActualWidth;
            ThicknessAnimation ani = new ThicknessAnimation();
            ani.From = new Thickness(0, 0, 0, 0);
            ani.To = new Thickness(w, 0, -w, 0);
            ani.Duration = TimeSpan.FromMilliseconds(5000);
            Storyboard.SetTarget(ani, lable);
            Storyboard.SetTargetProperty(ani, new PropertyPath(Label.MarginProperty));
            sb.Children.Add(ani);
            sb.Completed += sb_Completed;
            sb.BeginTime = TimeSpan.FromMilliseconds(1000);
           
            sb.Begin();
            
            this.DataContext = this;            
        }

        void sb_Completed(object sender, EventArgs e)
        {
            sb.Begin();
        }
    }
}
