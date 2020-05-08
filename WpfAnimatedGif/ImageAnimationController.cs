namespace WpfAnimatedGif
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    internal class ImageAnimationController : IDisposable
    {
        private static readonly DependencyPropertyDescriptor _sourceDescriptor = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
        private readonly Image _image;
        private readonly ObjectAnimationUsingKeyFrames _animation;
        private readonly AnimationClock _clock;
        private readonly ClockController _clockController;
        private EventHandler CurrentFrameChanged;

        public event EventHandler CurrentFrameChanged
        {
            add
            {
                EventHandler currentFrameChanged = this.CurrentFrameChanged;
                while (true)
                {
                    EventHandler a = currentFrameChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    currentFrameChanged = Interlocked.CompareExchange<EventHandler>(ref this.CurrentFrameChanged, handler3, a);
                    if (ReferenceEquals(currentFrameChanged, a))
                    {
                        return;
                    }
                }
            }
            remove
            {
                EventHandler currentFrameChanged = this.CurrentFrameChanged;
                while (true)
                {
                    EventHandler source = currentFrameChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    currentFrameChanged = Interlocked.CompareExchange<EventHandler>(ref this.CurrentFrameChanged, handler3, source);
                    if (ReferenceEquals(currentFrameChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        internal ImageAnimationController(Image image, ObjectAnimationUsingKeyFrames animation, bool autoStart)
        {
            this._image = image;
            this._animation = animation;
            this._animation.Completed += new EventHandler(this.AnimationCompleted);
            this._clock = this._animation.CreateClock();
            this._clockController = this._clock.Controller;
            _sourceDescriptor.AddValueChanged(image, new EventHandler(this.ImageSourceChanged));
            this._clockController.Pause();
            this._image.ApplyAnimationClock(Image.SourceProperty, this._clock);
            if (autoStart)
            {
                this._clockController.Resume();
            }
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            this._image.RaiseEvent(new RoutedEventArgs(ImageBehavior.AnimationCompletedEvent, this._image));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._image.BeginAnimation(Image.SourceProperty, null);
                this._animation.Completed -= new EventHandler(this.AnimationCompleted);
                _sourceDescriptor.RemoveValueChanged(this._image, new EventHandler(this.ImageSourceChanged));
                this._image.Source = null;
            }
        }

        ~ImageAnimationController()
        {
            this.Dispose(false);
        }

        public void GotoFrame(int index)
        {
            ObjectKeyFrame frame = this._animation.KeyFrames[index];
            this._clockController.Seek(frame.KeyTime.TimeSpan, TimeSeekOrigin.BeginTime);
        }

        private void ImageSourceChanged(object sender, EventArgs e)
        {
            this.OnCurrentFrameChanged();
        }

        private void OnCurrentFrameChanged()
        {
            EventHandler currentFrameChanged = this.CurrentFrameChanged;
            if (currentFrameChanged != null)
            {
                currentFrameChanged(this, EventArgs.Empty);
            }
        }

        public void Pause()
        {
            this._clockController.Pause();
        }

        public void Play()
        {
            switch (this._clock.CurrentState)
            {
                case ClockState.Active:
                    this._clockController.Resume();
                    return;

                case ClockState.Filling:
                case ClockState.Stopped:
                    this._clockController.Begin();
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        public int FrameCount =>
            this._animation.KeyFrames.Count;

        public bool IsPaused =>
            this._clock.IsPaused;

        public bool IsComplete =>
            (this._clock.CurrentState == ClockState.Filling);

        public int CurrentFrame
        {
            get
            {
                TimeSpan? time = this._clock.CurrentTime;
                <152b44db-0b6d-44f9-88e2-1d157602cc28><>f__AnonymousType0<TimeSpan, int> type = this._animation.KeyFrames.Cast<ObjectKeyFrame>().Select<ObjectKeyFrame, <152b44db-0b6d-44f9-88e2-1d157602cc28><>f__AnonymousType0<TimeSpan, int>>((f, i) => new <152b44db-0b6d-44f9-88e2-1d157602cc28><>f__AnonymousType0<TimeSpan, int>(f.KeyTime.TimeSpan, i)).FirstOrDefault<<152b44db-0b6d-44f9-88e2-1d157602cc28><>f__AnonymousType0<TimeSpan, int>>(delegate (<152b44db-0b6d-44f9-88e2-1d157602cc28><>f__AnonymousType0<TimeSpan, int> fi) {
                    TimeSpan span = fi.Time;
                    TimeSpan? nullable = time;
                    return (nullable != null) ? (span >= nullable.GetValueOrDefault()) : false;
                });
                return ((type == null) ? -1 : type.Index);
            }
        }
    }
}

