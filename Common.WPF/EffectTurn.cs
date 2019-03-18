using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Common.WPF
{
    /// <summary>
    /// 特效窗口
    /// </summary>
    public class EffectTurn
    {
        #region 属性和字段

        /// <summary>
        /// 翻转特效是否不被占用
        /// </summary>
        public bool EffectEnable { get; protected set; }
        public Storyboard Storyboard;
        public Viewport3D Viewport3d;

        private FrameworkElement new_page;
        private TurnType turnType = TurnType.Right;

        #endregion
        #region 初始和构造

        public EffectTurn(Window window)
        {
            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = true;
            window.Background = Brushes.Transparent;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Topmost = true;

            InitializeEffect3D(window);
        }

        private void InitializeEffect3D(Window window)
        {
            #region Viewport
            AxisAngleRotation3D angle = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            ScaleTransform3D scale = new ScaleTransform3D();
            Transform3DGroup t3dg = new Transform3DGroup();
            t3dg.Children.Add(scale);
            t3dg.Children.Add(new RotateTransform3D(angle));

            MeshGeometry3D mg3d = new MeshGeometry3D();
            mg3d.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, 0, 2, 3 });
            mg3d.TextureCoordinates = new PointCollection(new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0), });

            Viewport2DVisual3D Viewport2d = new Viewport2DVisual3D();
            Viewport2d.Geometry = mg3d;
            Viewport2d.Material = new DiffuseMaterial(Brushes.White);
            Viewport2DVisual3D.SetIsVisualHostMaterial(Viewport2d.Material, true);
            Viewport2d.Transform = t3dg;

            Viewport3d = new Viewport3D();
            Viewport3d.Camera = new PerspectiveCamera() { FieldOfView = 40 };
            ModelVisual3D light1 = new ModelVisual3D();
            light1.Content = new DirectionalLight(Colors.White, new Vector3D(1, 1.733, -1));
            ModelVisual3D light2 = new ModelVisual3D();
            light2.Content = new DirectionalLight(Colors.White, new Vector3D(1, -1.733, -1));
            ModelVisual3D light3 = new ModelVisual3D();
            light3.Content = new DirectionalLight(Colors.White, new Vector3D(-2, 0, -1));
            Viewport3d.Children.Add(light1);
            Viewport3d.Children.Add(light2);
            Viewport3d.Children.Add(light3);
            Viewport3d.Children.Add(Viewport2d);
            #endregion
            #region Animation
            window.RegisterName("Viewport2d_Angle", angle);
            PropertyPath path = new PropertyPath(AxisAngleRotation3D.AngleProperty);
            DoubleAnimationUsingKeyFrames daukf = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTargetName(daukf, "Viewport2d_Angle");
            Storyboard.SetTargetProperty(daukf, path);

            SplineDoubleKeyFrame KeyFrame1 = new SplineDoubleKeyFrame(90, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25)), new KeySpline(0.7, 0.5, 0.3, 0.2));
            DiscreteDoubleKeyFrame KeyFrame2 = new DiscreteDoubleKeyFrame(-90, KeyTime.FromPercent(0.5));
            daukf.KeyFrames.Add(KeyFrame1);
            daukf.KeyFrames.Add(KeyFrame2);
            daukf.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)), new KeySpline(0.2, 0.3, 0.5, 0.7)));

            Storyboard = new Storyboard();
            Storyboard.Children.Add(daukf);
            Storyboard.Completed += (sender, e) =>
            {
                try
                {
                    Viewport2d.Visual = null;
                    window.Content = new_page;
                }
                catch { }
                finally { EffectEnable = true; }
            };

            angle.Changed += (sender, e) =>
            {
                if (angle.Angle < -70 && Viewport2d.Visual != this.new_page)
                    try { Viewport2d.Visual = this.new_page; }
                    catch { }

                scale.ScaleX = scale.ScaleY
                = 1 - Math.Abs(Math.Sin(angle.Angle / 180 * Math.PI) / 2.75);
            };
            #endregion
            #region Effect
            setTurnStyle = (isV, isX) =>
            {
                int v = isV ? -90 : 90;
                KeyFrame1.Value = v;
                KeyFrame2.Value = -v;
                int x = isX ? 1 : 0;
                angle.Axis = new Vector3D(x, 1 - x, 0);
            };

            effectBegin = (old_page, new_page) =>
            {
                Viewport3d.Width = old_page.Width;
                Viewport3d.Height = old_page.Height;
                this.new_page = new_page;
                window.Content = Viewport3d;
                Viewport2d.Visual = old_page;
                EffectEnable = false;
                ((PerspectiveCamera)Viewport3d.Camera).Position = new Point3D(0, 0, 2.75 * old_page.Width);
                setPositions(mg3d, old_page.RenderSize);

                Storyboard.Begin(window);
            };

            EffectEnable = true;
            #endregion
        }

        #endregion
        #region 委托和方法
        private Action<bool, bool> setTurnStyle;
        private Action<FrameworkElement, FrameworkElement> effectBegin;

        public void TurnPage(FrameworkElement old_page, FrameworkElement new_page, TurnType turnType)
        {
            //if (old_page.Parent.Equals(this)) throw new Exception("");
            if (EffectEnable)
            {
                if (this.turnType != turnType)
                {
                    switch (turnType)
                    {
                        case TurnType.Up: setTurnStyle(true, true); break;
                        case TurnType.Dowm: setTurnStyle(false, true); break;
                        case TurnType.Left: setTurnStyle(true, false); break;
                        case TurnType.Right: setTurnStyle(false, false); break;
                        default: throw new Exception();
                    }
                    this.turnType = turnType;
                }
                this.effectBegin(old_page, new_page);
            }
        }

        private void setPositions(MeshGeometry3D mg3d, Size s)
        {
            mg3d.Positions = new Point3DCollection(new Point3D[] {
                        new Point3D(-s.Width, s.Height, 0),    //0
                        new Point3D(-s.Width, -s.Height, 0),   //1
                        new Point3D(s.Width, -s.Height, 0),    //2
                        new Point3D(s.Width, s.Height, 0),     //3
                    });
        }

        #endregion
    }

    /// <summary>
    /// 翻转特效的方向
    /// </summary>
    public enum TurnType { Up, Dowm, Left, Right }
}