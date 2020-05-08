namespace WpfAnimatedGif
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Resources;
    using WpfAnimatedGif.Decoding;

    internal static class ImageBehavior
    {
        public static readonly DependencyProperty AnimatedSourceProperty = DependencyProperty.RegisterAttached("AnimatedSource", typeof(ImageSource), typeof(ImageBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(ImageBehavior.AnimatedSourceChanged)));
        public static readonly DependencyProperty RepeatBehaviorProperty;
        public static readonly DependencyProperty AnimateInDesignModeProperty;
        public static readonly DependencyProperty AutoStartProperty;
        private static readonly DependencyPropertyKey AnimationControllerPropertyKey;
        private static readonly DependencyPropertyKey IsAnimationLoadedPropertyKey;
        public static readonly DependencyProperty IsAnimationLoadedProperty;
        public static readonly RoutedEvent AnimationLoadedEvent;
        public static readonly RoutedEvent AnimationCompletedEvent;

        static ImageBehavior()
        {
            RepeatBehavior defaultValue = new RepeatBehavior();
            RepeatBehaviorProperty = DependencyProperty.RegisterAttached("RepeatBehavior", typeof(RepeatBehavior), typeof(ImageBehavior), new UIPropertyMetadata(defaultValue, new PropertyChangedCallback(ImageBehavior.RepeatBehaviorChanged)));
            AnimateInDesignModeProperty = DependencyProperty.RegisterAttached("AnimateInDesignMode", typeof(bool), typeof(ImageBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(ImageBehavior.AnimateInDesignModeChanged)));
            AutoStartProperty = DependencyProperty.RegisterAttached("AutoStart", typeof(bool), typeof(ImageBehavior), new PropertyMetadata(true));
            AnimationControllerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("AnimationController", typeof(ImageAnimationController), typeof(ImageBehavior), new PropertyMetadata(null));
            IsAnimationLoadedPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsAnimationLoaded", typeof(bool), typeof(ImageBehavior), new PropertyMetadata(false));
            IsAnimationLoadedProperty = IsAnimationLoadedPropertyKey.DependencyProperty;
            AnimationLoadedEvent = EventManager.RegisterRoutedEvent("AnimationLoaded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageBehavior));
            AnimationCompletedEvent = EventManager.RegisterRoutedEvent("AnimationCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageBehavior));
        }

        public static void AddAnimationCompletedHandler(Image d, RoutedEventHandler handler)
        {
            UIElement element = d;
            if (element != null)
            {
                element.AddHandler(AnimationCompletedEvent, handler);
            }
        }

        public static void AddAnimationLoadedHandler(Image image, RoutedEventHandler handler)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            image.AddHandler(AnimationLoadedEvent, handler);
        }

        private static void AnimatedSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Image image = o as Image;
            if (image != null)
            {
                ImageSource oldValue = e.OldValue as ImageSource;
                ImageSource newValue = e.NewValue as ImageSource;
                if (oldValue != null)
                {
                    image.Loaded -= new RoutedEventHandler(ImageBehavior.ImageControlLoaded);
                    image.Unloaded -= new RoutedEventHandler(ImageBehavior.ImageControlUnloaded);
                    AnimationCache.DecrementReferenceCount(oldValue, GetRepeatBehavior(image));
                    ImageAnimationController animationController = GetAnimationController(image);
                    if (animationController != null)
                    {
                        animationController.Dispose();
                    }
                    image.Source = null;
                }
                if (newValue != null)
                {
                    image.Loaded += new RoutedEventHandler(ImageBehavior.ImageControlLoaded);
                    image.Unloaded += new RoutedEventHandler(ImageBehavior.ImageControlUnloaded);
                    if (image.IsLoaded)
                    {
                        InitAnimationOrImage(image);
                    }
                }
            }
        }

        private static void AnimateInDesignModeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Image image = o as Image;
            if (image != null)
            {
                bool newValue = (bool) e.NewValue;
                if ((GetAnimatedSource(image) != null) && image.IsLoaded)
                {
                    if (newValue)
                    {
                        InitAnimationOrImage(image);
                    }
                    else
                    {
                        image.BeginAnimation(Image.SourceProperty, null);
                    }
                }
            }
        }

        private static bool CanReadNativeMetadata(BitmapDecoder decoder)
        {
            try
            {
                return !ReferenceEquals(decoder.Metadata, null);
            }
            catch
            {
                return false;
            }
        }

        private static BitmapSource ClearArea(BitmapSource frame, FrameMetadata metadata)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                Rect rect = new Rect(0.0, 0.0, (double) frame.PixelWidth, (double) frame.PixelHeight);
                Rect rect2 = new Rect((double) metadata.Left, (double) metadata.Top, (double) metadata.Width, (double) metadata.Height);
                context.PushClip(Geometry.Combine(new RectangleGeometry(rect), new RectangleGeometry(rect2), GeometryCombineMode.Exclude, null));
                context.DrawImage(frame, rect);
            }
            RenderTargetBitmap bitmap = new RenderTargetBitmap(frame.PixelWidth, frame.PixelHeight, frame.DpiX, frame.DpiY, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            if (bitmap.CanFreeze && !bitmap.IsFrozen)
            {
                bitmap.Freeze();
            }
            return bitmap;
        }

        private static GifFile DecodeGifFile(Uri uri)
        {
            Stream stream = null;
            if (uri.Scheme != PackUriHelper.UriSchemePack)
            {
                stream = new WebClient().OpenRead(uri);
            }
            else
            {
                StreamResourceInfo info = (uri.Authority != "siteoforigin:,,,") ? Application.GetResourceStream(uri) : Application.GetRemoteStream(uri);
                if (info != null)
                {
                    stream = info.Stream;
                }
            }
            if (stream == null)
            {
                return null;
            }
            using (stream)
            {
                return GifFile.ReadGifFile(stream, true);
            }
        }

        private static RepeatBehavior GetActualRepeatBehavior(Image imageControl, BitmapDecoder decoder, GifFile gifMetadata)
        {
            RepeatBehavior repeatBehavior = GetRepeatBehavior(imageControl);
            RepeatBehavior behavior2 = new RepeatBehavior();
            if (repeatBehavior != behavior2)
            {
                return repeatBehavior;
            }
            int num = (gifMetadata == null) ? GetRepeatCount(decoder) : gifMetadata.RepeatCount;
            return ((num != 0) ? new RepeatBehavior((double) num) : RepeatBehavior.Forever);
        }

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static ImageSource GetAnimatedSource(Image obj) => 
            ((ImageSource) obj.GetValue(AnimatedSourceProperty));

        public static bool GetAnimateInDesignMode(DependencyObject obj) => 
            ((bool) obj.GetValue(AnimateInDesignModeProperty));

        private static ObjectAnimationUsingKeyFrames GetAnimation(Image imageControl, BitmapSource source)
        {
            ObjectAnimationUsingKeyFrames animation = AnimationCache.GetAnimation(source, GetRepeatBehavior(imageControl));
            if (animation == null)
            {
                GifFile file;
                GifBitmapDecoder decoder = GetDecoder(source, out file) as GifBitmapDecoder;
                if ((decoder == null) || (decoder.Frames.Count <= 1))
                {
                    return null;
                }
                Int32Size fullSize = GetFullSize(decoder, file);
                int frameIndex = 0;
                animation = new ObjectAnimationUsingKeyFrames();
                TimeSpan zero = TimeSpan.Zero;
                BitmapSource baseFrame = null;
                foreach (BitmapFrame frame in decoder.Frames)
                {
                    FrameMetadata metadata = GetFrameMetadata(decoder, file, frameIndex);
                    BitmapSource source3 = MakeFrame(fullSize, frame, metadata, baseFrame);
                    DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame(source3, zero);
                    animation.KeyFrames.Add(keyFrame);
                    zero += metadata.Delay;
                    FrameDisposalMethod disposalMethod = metadata.DisposalMethod;
                    switch (disposalMethod)
                    {
                        case FrameDisposalMethod.None:
                        case FrameDisposalMethod.DoNotDispose:
                            baseFrame = source3;
                            break;

                        case FrameDisposalMethod.RestoreBackground:
                            baseFrame = !IsFullFrame(metadata, fullSize) ? ClearArea(source3, metadata) : null;
                            break;

                        default:
                            break;
                    }
                    frameIndex++;
                }
                animation.Duration = zero;
                animation.RepeatBehavior = GetActualRepeatBehavior(imageControl, decoder, file);
                AnimationCache.AddAnimation(source, GetRepeatBehavior(imageControl), animation);
                AnimationCache.IncrementReferenceCount(source, GetRepeatBehavior(imageControl));
            }
            return animation;
        }

        public static ImageAnimationController GetAnimationController(Image imageControl) => 
            ((ImageAnimationController) imageControl.GetValue(AnimationControllerPropertyKey.DependencyProperty));

        private static BitmapMetadata GetApplicationExtension(BitmapDecoder decoder, string application)
        {
            int num = 0;
            string query = "/appext";
            for (BitmapMetadata metadata = decoder.Metadata.GetQueryOrNull<BitmapMetadata>(query); metadata != null; metadata = decoder.Metadata.GetQueryOrNull<BitmapMetadata>(query))
            {
                byte[] queryOrNull = metadata.GetQueryOrNull<byte[]>("/Application");
                if ((queryOrNull != null) && (Encoding.ASCII.GetString(queryOrNull) == application))
                {
                    return metadata;
                }
                query = $"/[{++num}]appext";
            }
            return null;
        }

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetAutoStart(Image obj) => 
            ((bool) obj.GetValue(AutoStartProperty));

        private static BitmapDecoder GetDecoder(BitmapSource image, out GifFile gifFile)
        {
            gifFile = null;
            BitmapDecoder decoder = null;
            Stream bitmapStream = null;
            Uri result = null;
            BitmapCreateOptions none = BitmapCreateOptions.None;
            BitmapImage image2 = image as BitmapImage;
            if (image2 == null)
            {
                BitmapFrame frame = image as BitmapFrame;
                if (frame != null)
                {
                    decoder = frame.Decoder;
                    Uri.TryCreate(frame.BaseUri, frame.ToString(), out result);
                }
            }
            else
            {
                none = image2.CreateOptions;
                if (image2.StreamSource != null)
                {
                    bitmapStream = image2.StreamSource;
                }
                else if (image2.UriSource != null)
                {
                    result = image2.UriSource;
                    if ((image2.BaseUri != null) && !result.IsAbsoluteUri)
                    {
                        result = new Uri(image2.BaseUri, result);
                    }
                }
            }
            if (decoder == null)
            {
                if (bitmapStream != null)
                {
                    bitmapStream.Position = 0L;
                    decoder = BitmapDecoder.Create(bitmapStream, none, BitmapCacheOption.OnLoad);
                }
                else if ((result != null) && result.IsAbsoluteUri)
                {
                    decoder = BitmapDecoder.Create(result, none, BitmapCacheOption.OnLoad);
                }
            }
            if ((decoder is GifBitmapDecoder) && !CanReadNativeMetadata(decoder))
            {
                if (bitmapStream != null)
                {
                    bitmapStream.Position = 0L;
                    gifFile = GifFile.ReadGifFile(bitmapStream, true);
                }
                else if (result != null)
                {
                    gifFile = DecodeGifFile(result);
                }
            }
            return decoder;
        }

        private static FrameMetadata GetFrameMetadata(BitmapFrame frame)
        {
            BitmapMetadata metadata = (BitmapMetadata) frame.Metadata;
            TimeSpan span = TimeSpan.FromMilliseconds(100.0);
            int num = metadata.GetQueryOrDefault<int>("/grctlext/Delay", 10);
            if (num != 0)
            {
                span = TimeSpan.FromMilliseconds((double) (num * 10));
            }
            return new FrameMetadata { 
                Left = metadata.GetQueryOrDefault<int>("/imgdesc/Left", 0),
                Top = metadata.GetQueryOrDefault<int>("/imgdesc/Top", 0),
                Width = metadata.GetQueryOrDefault<int>("/imgdesc/Width", frame.PixelWidth),
                Height = metadata.GetQueryOrDefault<int>("/imgdesc/Height", frame.PixelHeight),
                Delay = span,
                DisposalMethod = metadata.GetQueryOrDefault<int>("/grctlext/Disposal", 0)
            };
        }

        private static FrameMetadata GetFrameMetadata(GifFrame gifMetadata)
        {
            GifImageDescriptor descriptor = gifMetadata.Descriptor;
            FrameMetadata metadata = new FrameMetadata {
                Left = descriptor.Left,
                Top = descriptor.Top,
                Width = descriptor.Width,
                Height = descriptor.Height,
                Delay = TimeSpan.FromMilliseconds(100.0),
                DisposalMethod = FrameDisposalMethod.None
            };
            GifGraphicControlExtension extension = gifMetadata.Extensions.OfType<GifGraphicControlExtension>().FirstOrDefault<GifGraphicControlExtension>();
            if (extension != null)
            {
                if (extension.Delay != 0)
                {
                    metadata.Delay = TimeSpan.FromMilliseconds((double) extension.Delay);
                }
                metadata.DisposalMethod = (FrameDisposalMethod) extension.DisposalMethod;
            }
            return metadata;
        }

        private static FrameMetadata GetFrameMetadata(BitmapDecoder decoder, GifFile gifMetadata, int frameIndex) => 
            (((gifMetadata == null) || (gifMetadata.Frames.Count <= frameIndex)) ? GetFrameMetadata(decoder.Frames[frameIndex]) : GetFrameMetadata(gifMetadata.Frames[frameIndex]));

        private static Int32Size GetFullSize(BitmapDecoder decoder, GifFile gifMetadata)
        {
            if (gifMetadata == null)
            {
                return new Int32Size(decoder.Metadata.GetQueryOrDefault<int>("/logscrdesc/Width", 0), decoder.Metadata.GetQueryOrDefault<int>("/logscrdesc/Height", 0));
            }
            GifLogicalScreenDescriptor logicalScreenDescriptor = gifMetadata.Header.LogicalScreenDescriptor;
            return new Int32Size(logicalScreenDescriptor.Width, logicalScreenDescriptor.Height);
        }

        public static bool GetIsAnimationLoaded(Image image) => 
            ((bool) image.GetValue(IsAnimationLoadedProperty));

        private static T GetQueryOrDefault<T>(this BitmapMetadata metadata, string query, T defaultValue) => 
            (!metadata.ContainsQuery(query) ? defaultValue : ((T) Convert.ChangeType(metadata.GetQuery(query), typeof(T))));

        private static T GetQueryOrNull<T>(this BitmapMetadata metadata, string query) where T: class
        {
            if (metadata.ContainsQuery(query))
            {
                return (metadata.GetQuery(query) as T);
            }
            return default(T);
        }

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static RepeatBehavior GetRepeatBehavior(Image obj) => 
            ((RepeatBehavior) obj.GetValue(RepeatBehaviorProperty));

        private static int GetRepeatCount(BitmapDecoder decoder)
        {
            BitmapMetadata applicationExtension = GetApplicationExtension(decoder, "NETSCAPE2.0");
            if (applicationExtension != null)
            {
                byte[] queryOrNull = applicationExtension.GetQueryOrNull<byte[]>("/Data");
                if ((queryOrNull != null) && (queryOrNull.Length >= 4))
                {
                    return BitConverter.ToUInt16(queryOrNull, 2);
                }
            }
            return 1;
        }

        private static void ImageControlLoaded(object sender, RoutedEventArgs e)
        {
            Image imageControl = sender as Image;
            if (imageControl != null)
            {
                InitAnimationOrImage(imageControl);
            }
        }

        private static void ImageControlUnloaded(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            if (image != null)
            {
                ImageSource animatedSource = GetAnimatedSource(image);
                if (animatedSource != null)
                {
                    AnimationCache.DecrementReferenceCount(animatedSource, GetRepeatBehavior(image));
                }
                ImageAnimationController animationController = GetAnimationController(image);
                if (animationController != null)
                {
                    animationController.Dispose();
                }
            }
        }

        private static void InitAnimationOrImage(Image imageControl)
        {
            ImageAnimationController animationController = GetAnimationController(imageControl);
            if (animationController != null)
            {
                animationController.Dispose();
            }
            SetAnimationController(imageControl, null);
            SetIsAnimationLoaded(imageControl, false);
            BitmapSource source = GetAnimatedSource(imageControl) as BitmapSource;
            bool animateInDesignMode = GetAnimateInDesignMode(imageControl);
            bool flag3 = !DesignerProperties.GetIsInDesignMode(imageControl) || animateInDesignMode;
            bool flag4 = IsLoadingDeferred(source);
            if ((source != null) && (flag3 && !flag4))
            {
                Action action = null;
                if (source.IsDownloading)
                {
                    EventHandler handler = null;
                    handler = delegate (object sender, EventArgs args) {
                        source.DownloadCompleted -= handler;
                        InitAnimationOrImage(imageControl);
                    };
                    source.DownloadCompleted += handler;
                    imageControl.Source = source;
                    return;
                }
                ObjectAnimationUsingKeyFrames animation = GetAnimation(imageControl, source);
                if (animation != null)
                {
                    if (animation.KeyFrames.Count <= 0)
                    {
                        imageControl.Source = source;
                    }
                    else
                    {
                        if (action == null)
                        {
                            action = () => imageControl.Source = (ImageSource) animation.KeyFrames[0].Value;
                        }
                        TryTwice(action);
                    }
                    animationController = new ImageAnimationController(imageControl, animation, GetAutoStart(imageControl));
                    SetAnimationController(imageControl, animationController);
                    SetIsAnimationLoaded(imageControl, true);
                    imageControl.RaiseEvent(new RoutedEventArgs(AnimationLoadedEvent, imageControl));
                    return;
                }
            }
            imageControl.Source = source;
            if (source != null)
            {
                SetIsAnimationLoaded(imageControl, true);
                imageControl.RaiseEvent(new RoutedEventArgs(AnimationLoadedEvent, imageControl));
            }
        }

        private static bool IsFullFrame(FrameMetadata metadata, Int32Size fullSize) => 
            ((metadata.Left == 0) && ((metadata.Top == 0) && ((metadata.Width == fullSize.Width) && (metadata.Height == fullSize.Height))));

        private static bool IsLoadingDeferred(BitmapSource source)
        {
            BitmapImage image = source as BitmapImage;
            return ((image != null) ? ((image.UriSource != null) && (!image.UriSource.IsAbsoluteUri && (image.BaseUri == null))) : false);
        }

        private static BitmapSource MakeFrame(Int32Size fullSize, BitmapSource rawFrame, FrameMetadata metadata, BitmapSource baseFrame)
        {
            if ((baseFrame == null) && IsFullFrame(metadata, fullSize))
            {
                return rawFrame;
            }
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                if (baseFrame != null)
                {
                    Rect rect = new Rect(0.0, 0.0, (double) fullSize.Width, (double) fullSize.Height);
                    context.DrawImage(baseFrame, rect);
                }
                Rect rectangle = new Rect((double) metadata.Left, (double) metadata.Top, (double) metadata.Width, (double) metadata.Height);
                context.DrawImage(rawFrame, rectangle);
            }
            RenderTargetBitmap bitmap = new RenderTargetBitmap(fullSize.Width, fullSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            if (bitmap.CanFreeze && !bitmap.IsFrozen)
            {
                bitmap.Freeze();
            }
            return bitmap;
        }

        public static void RemoveAnimationCompletedHandler(Image d, RoutedEventHandler handler)
        {
            UIElement element = d;
            if (element != null)
            {
                element.RemoveHandler(AnimationCompletedEvent, handler);
            }
        }

        public static void RemoveAnimationLoadedHandler(Image image, RoutedEventHandler handler)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            image.RemoveHandler(AnimationLoadedEvent, handler);
        }

        private static void RepeatBehaviorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Image image = o as Image;
            if (image != null)
            {
                ImageSource animatedSource = GetAnimatedSource(image);
                if (animatedSource != null)
                {
                    if (!Equals(e.OldValue, e.NewValue))
                    {
                        AnimationCache.DecrementReferenceCount(animatedSource, (RepeatBehavior) e.OldValue);
                    }
                    if (image.IsLoaded)
                    {
                        InitAnimationOrImage(image);
                    }
                }
            }
        }

        public static void SetAnimatedSource(Image obj, ImageSource value)
        {
            obj.SetValue(AnimatedSourceProperty, value);
        }

        public static void SetAnimateInDesignMode(DependencyObject obj, bool value)
        {
            obj.SetValue(AnimateInDesignModeProperty, value);
        }

        private static void SetAnimationController(DependencyObject obj, ImageAnimationController value)
        {
            obj.SetValue(AnimationControllerPropertyKey, value);
        }

        public static void SetAutoStart(Image obj, bool value)
        {
            obj.SetValue(AutoStartProperty, value);
        }

        private static void SetIsAnimationLoaded(Image image, bool value)
        {
            image.SetValue(IsAnimationLoadedPropertyKey, value);
        }

        public static void SetRepeatBehavior(Image obj, RepeatBehavior value)
        {
            obj.SetValue(RepeatBehaviorProperty, value);
        }

        private static void TryTwice(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                action();
            }
        }

        private enum FrameDisposalMethod
        {
            None,
            DoNotDispose,
            RestoreBackground,
            RestorePrevious
        }

        private class FrameMetadata
        {
            public int Left { get; set; }

            public int Top { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public TimeSpan Delay { get; set; }

            public ImageBehavior.FrameDisposalMethod DisposalMethod { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Int32Size
        {
            public Int32Size(int width, int height)
            {
                this = new ImageBehavior.Int32Size();
                this.Width = width;
                this.Height = height;
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
        }
    }
}

