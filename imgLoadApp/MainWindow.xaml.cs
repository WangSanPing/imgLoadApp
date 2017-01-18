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
using System.Collections.ObjectModel;
using System.Globalization;

namespace imgLoadApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Uri> images = null;

        // 用于线程间同步的对象
        object lockobj = new object();

        public MainWindow()
        {
            InitializeComponent();
            images = new ObservableCollection<Uri>();
            Binding b = new Binding
            {
                Source = images,
                IsAsync = true
            };
            lbImages.SetBinding(ItemsControl.ItemsSourceProperty, b);

            // 这一句很关键，开启集合的异步访问支持
            BindingOperations.EnableCollectionSynchronization(images, lockobj);

            // 异步加载数据
            Task.Run(() =>
            {
                // 代码写在 lock 块中
                lock (lockobj)
                {
                    for (int i = 0; i < 20000; i++)
                    {
                        Uri u = new Uri("0.jpg", UriKind.Relative);
                        images.Add(u);
                    }
                }
            });
        }
    }

    public sealed class UriToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri uri = (Uri)value;
            BitmapImage bmp = new BitmapImage();
            bmp.DecodePixelHeight = 250; // 确定解码高度，宽度不同时设置
            bmp.BeginInit();
            // 延迟，必要时创建
            bmp.CreateOptions = BitmapCreateOptions.DelayCreation;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = uri;
            bmp.EndInit(); //结束初始化
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
