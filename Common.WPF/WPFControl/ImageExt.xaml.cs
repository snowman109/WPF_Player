using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace Common.WPF
{
    /// <summary>
    /// Interaction logic for ImageExt.xaml
    /// </summary>
    public partial class ImageExt : UserControl, INotifyPropertyChanged
    {
        //上次鼠标位置
        Point previousMousePoint = new Point();
        //鼠标左键按下并移动
        bool isLeftButtonDown = false;
        /// <summary>
        /// 图片移动事件
        /// </summary>
        /// <param name="xchanged">x坐标该变量</param>
        /// <param name="ychanged">y坐标该变量</param>
        /// <param name="scale">缩放倍数</param>
        public delegate void MoveHandler(double x, double y);
        public event MoveHandler MoveEvent;

        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="offsetX">x偏移量</param>
        /// <param name="offsetY">y偏移量</param>
        /// <param name="scale">缩放倍数</param>
        public delegate void ZoomHandler(double offsetX,double offsetY, double scale);
        public event ZoomHandler ZoomEvent;

        #region 属性

        private string imgPath;
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath
        {
            get { return imgPath; }
            set { imgPath = value; OnPropertyChanged("ImagePath"); }
        }

        private double imgScale = 1.0;
        /// <summary>
        /// 图片缩放比例，最小为1
        /// </summary>
        public double ImageScale
        {
            get { return imgScale; }
            set { imgScale = value; }
        }

        /// <summary>
        /// 图片实时宽度
        /// </summary>
        public double ImageWidth
        {
            get { return this.image.ActualWidth; }
        }
        /// <summary>
        /// 图片实时高度
        /// </summary>
        public double ImageHeight
        {
            get
            {
                return this.image.ActualHeight;
            }
        }

        public double ImageOffsetX
        {
            get
            {
                TransformGroup group = contentControl.FindResource("ImageCompareResources") as TransformGroup;
                TranslateTransform transformXY = group.Children[1] as TranslateTransform;
                if (transformXY != null)
                    return transformXY.X;
                return 0;
            }
        }

        public double ImageOffsetY
        {
            get
            {
                TransformGroup group = contentControl.FindResource("ImageCompareResources") as TransformGroup;
                TranslateTransform transformXY = group.Children[1] as TranslateTransform;
                if (transformXY != null)
                    return transformXY.Y;
                return 0;
            }
        }

        #endregion

        #region 构造器
        public ImageExt()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        private void contentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLeftButtonDown = true;
            this.Cursor = Cursors.Hand;
            this.previousMousePoint = e.GetPosition(contentControl);
        }

        private void contentControl_MouseMove(object sender, MouseEventArgs e)
        {
            ContentControl content = sender as ContentControl;

            if (content == null)
            {
                return;
            }
            if (isLeftButtonDown)//e.LeftButton == MouseButtonState.Pressed
            {
                this.DoImageMove(content, e.GetPosition(content));
            }
        }

        private void contentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            isLeftButtonDown = false;
        }

        private void contentControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            isLeftButtonDown = false;
        }

        private void contentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ContentControl content = sender as ContentControl;

            if (content == null)
            {
                return;
            }
            TransformGroup group = contentControl.FindResource("ImageCompareResources") as TransformGroup;
            Point point = e.GetPosition(content);
            double scale = e.Delta * 0.001;
            ZoomImage(group, point, scale);
        }

        private void contentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TransformGroup group = contentControl.FindResource("ImageCompareResources") as TransformGroup;
            TranslateTransform transform = group.Children[1] as TranslateTransform;
            //调整x方向偏移量
            ResizeOffSetX(transform);
            //调整y方向偏移量
            ResizeOffSetY(transform);
        }

        private void DoImageMove(ContentControl content, Point position)
        {
            TransformGroup group = contentControl.FindResource("ImageCompareResources") as TransformGroup;
            TranslateTransform transform = group.Children[1] as TranslateTransform;
            //未移动前的偏移量
            double previousX = transform.X;
            double previousY = transform.Y;

            if (contentControl.ActualWidth < image.ActualWidth * ImageScale)
            {
                if (position.X > previousMousePoint.X)//向右移动
                {
                    //图片和容器左边界重合时图片的偏移量
                    double leftX = -(contentControl.ActualWidth - image.ActualWidth) / 2;
                    transform.X += position.X - this.previousMousePoint.X;
                    transform.X = transform.X > leftX ? leftX : transform.X;
                }
                else//向左移动
                {
                    //图片和容器右边界重合时图片的偏移量
                    double rightX = (image.ActualWidth - contentControl.ActualWidth) / 2 + contentControl.ActualWidth - image.ActualWidth * ImageScale;
                    transform.X += position.X - this.previousMousePoint.X;
                    transform.X = transform.X < rightX ? rightX : transform.X;
                }
            }
            if (contentControl.ActualHeight < image.ActualHeight * ImageScale)
            {
                if (position.Y >= previousMousePoint.Y)//向下移动
                {
                    //图片和容器上边界重合时图片的偏移量
                    double upY = -(contentControl.ActualHeight - image.ActualHeight) / 2;
                    transform.Y += position.Y - this.previousMousePoint.Y;
                    transform.Y = transform.Y > upY ? upY : transform.Y;
                }
                else     //向上移动                            
                {
                    //图片和容器下边界重叠时图片的偏移量
                    double downY = ((image.ActualHeight - contentControl.ActualHeight) / 2) + (contentControl.ActualHeight - image.ActualHeight * ImageScale);
                    transform.Y += position.Y - this.previousMousePoint.Y;
                    transform.Y = transform.Y < downY ? downY : transform.Y;
                }
            }

            if (MoveEvent != null)
            {
                MoveEvent(transform.X - previousX, transform.Y - previousY);
            }

            this.previousMousePoint = position;
        }

        private void ZoomImage(TransformGroup group, Point point, double scale)
        {
            Point pointToContent = group.Inverse.Transform(point);
            ScaleTransform transform = group.Children[0] as ScaleTransform;
            TranslateTransform transformXY = group.Children[1] as TranslateTransform;
            if (transform.ScaleX + scale < 1)
            {
                return;
            }

            //缩放
            ImageScale = transform.ScaleX += scale;
            transform.ScaleY += scale;
            TranslateTransform transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);

            if (image.ActualWidth * ImageScale < contentControl.ActualWidth)//图片宽度小于容器的
            {
                transformXY.X = -(ImageScale - 1) * image.ActualWidth / 2;
            }
            if (image.ActualHeight * ImageScale < contentControl.ActualHeight)//图片高度小于容器的
            {
                transformXY.Y = -(ImageScale - 1) * image.ActualHeight / 2;
            }
            //调整位置
            if(scale<0)
            {
                //调整水平方向
                ResizeOffSetX(transformXY);
                //调整垂直方向
                ResizeOffSetY(transformXY);
            }
            //触发事件
            if(ZoomEvent!=null)
            {
                ZoomEvent(transformXY.X, transformXY.Y, ImageScale);
            }
            
        }

        /// <summary>
        /// 调整垂直方向的偏移量
        /// </summary>
        private void ResizeOffSetY(TranslateTransform transform)
        {
            //从上边越界时y的偏移量
            double upY = -(contentControl.ActualHeight - image.ActualHeight) / 2;
            //从下面越界时y的偏移量
            double downY = -(contentControl.ActualHeight - image.ActualHeight) / 2 + (contentControl.ActualHeight - image.ActualHeight * ImageScale);
            if (image.ActualHeight * ImageScale < contentControl.ActualHeight)//图片当前高度小于容器的
            {
                transform.Y = -(ImageScale - 1) * image.ActualHeight / 2;
            }
            else if (transform.Y > upY)
            {
                transform.Y = upY;
            }
            else if (transform.Y < downY)
            {
                transform.Y = downY;
            }
        }

        /// <summary>
        /// 调整垂直方向的偏移量
        /// </summary>
        /// <param name="transform"></param>
        private void ResizeOffSetX(TranslateTransform transform)
        {
            //从左边越界时x的偏移量
            double leftX = -(contentControl.ActualWidth - image.ActualWidth) / 2;
            //从右边越界时x的偏移量
            double rightX = -((contentControl.ActualWidth - image.ActualWidth) / 2) + (contentControl.ActualWidth - image.ActualWidth * ImageScale);
            if (image.ActualWidth * ImageScale < contentControl.ActualWidth)//图片宽度小于容器的
            {
                transform.X = -(ImageScale - 1) * image.ActualWidth / 2;
            }
            else if (transform.X > leftX)
            {
                transform.X = leftX;
            }
            else if (transform.X < rightX)
            {
                transform.X = rightX;
            }
        }

        #region INotifyPropertyChanged接口

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
