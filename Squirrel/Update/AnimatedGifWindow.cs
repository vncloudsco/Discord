namespace Squirrel.Update
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shell;
    using WpfAnimatedGif;

    public class AnimatedGifWindow : Window
    {
        public AnimatedGifWindow()
        {
            Image image = new Image();
            BitmapImage image2 = null;
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "background.gif");
            if (File.Exists(path))
            {
                image2 = new BitmapImage();
                image2.BeginInit();
                image2.StreamSource = File.OpenRead(path);
                image2.EndInit();
                ImageBehavior.SetAnimatedSource(image, image2);
                base.Content = image;
                base.Width = image2.Width;
                base.Height = image2.Height;
            }
            base.AllowsTransparency = true;
            base.WindowStyle = WindowStyle.None;
            base.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.ShowInTaskbar = true;
            base.Topmost = true;
            TaskbarItemInfo info1 = new TaskbarItemInfo();
            info1.set_ProgressState(2);
            base.set_TaskbarItemInfo(info1);
            base.Title = "Installing...";
            base.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public static void ShowWindow(TimeSpan initialDelay, CancellationToken token, ProgressSource progressSource)
        {
            AnimatedGifWindow wnd = null;
            Thread thread1 = new Thread(delegate {
                if (!token.IsCancellationRequested)
                {
                    EventHandler<int> <>9__5;
                    Action <>9__4;
                    Action<Task> <>9__2;
                    try
                    {
                        Task.Delay(initialDelay, token).ContinueWith<bool>(t => true).Wait();
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    wnd = new AnimatedGifWindow();
                    wnd.Show();
                    Action<Task> continuationAction = <>9__2;
                    if (<>9__2 == null)
                    {
                        Action<Task> local3 = <>9__2;
                        continuationAction = <>9__2 = delegate (Task t) {
                            if (!t.IsCanceled)
                            {
                                Action <>9__3;
                                Action method = <>9__3;
                                if (<>9__3 == null)
                                {
                                    Action local1 = <>9__3;
                                    method = <>9__3 = () => wnd.Topmost = false;
                                }
                                wnd.Dispatcher.BeginInvoke(method, new object[0]);
                            }
                        };
                    }
                    Task.Delay(TimeSpan.FromSeconds(5.0), token).ContinueWith(continuationAction);
                    Action callback = <>9__4;
                    if (<>9__4 == null)
                    {
                        Action local4 = <>9__4;
                        callback = <>9__4 = () => wnd.Dispatcher.BeginInvoke(new Action(wnd.Close), new object[0]);
                    }
                    token.Register(callback);
                    EventHandler<int> handler3 = <>9__5;
                    if (<>9__5 == null)
                    {
                        EventHandler<int> local5 = <>9__5;
                        handler3 = <>9__5 = (sender, p) => wnd.Dispatcher.BeginInvoke(() => wnd.get_TaskbarItemInfo().set_ProgressValue(((double) p) / 100.0), new object[0]);
                    }
                    EventHandler<int> handler = handler3;
                    progressSource.Progress += handler;
                    try
                    {
                        new Application().Run(wnd);
                    }
                    finally
                    {
                        progressSource.Progress -= handler;
                    }
                }
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AnimatedGifWindow.<>c <>9 = new AnimatedGifWindow.<>c();
            public static Func<Task, bool> <>9__1_1;

            internal bool <ShowWindow>b__1_1(Task t) => 
                true;
        }
    }
}

