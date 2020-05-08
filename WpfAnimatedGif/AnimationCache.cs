namespace WpfAnimatedGif
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;

    internal static class AnimationCache
    {
        private static readonly Dictionary<CacheKey, ObjectAnimationUsingKeyFrames> _animationCache = new Dictionary<CacheKey, ObjectAnimationUsingKeyFrames>();
        private static readonly Dictionary<CacheKey, int> _referenceCount = new Dictionary<CacheKey, int>();

        public static void AddAnimation(ImageSource source, RepeatBehavior repeatBehavior, ObjectAnimationUsingKeyFrames animation)
        {
            CacheKey key = new CacheKey(source, repeatBehavior);
            _animationCache[key] = animation;
        }

        public static void DecrementReferenceCount(ImageSource source, RepeatBehavior repeatBehavior)
        {
            int num;
            CacheKey key = new CacheKey(source, repeatBehavior);
            _referenceCount.TryGetValue(key, out num);
            if (num > 0)
            {
                num--;
                _referenceCount[key] = num;
            }
            if (num == 0)
            {
                _animationCache.Remove(key);
                _referenceCount.Remove(key);
            }
        }

        public static ObjectAnimationUsingKeyFrames GetAnimation(ImageSource source, RepeatBehavior repeatBehavior)
        {
            ObjectAnimationUsingKeyFrames frames;
            CacheKey key = new CacheKey(source, repeatBehavior);
            _animationCache.TryGetValue(key, out frames);
            return frames;
        }

        public static void IncrementReferenceCount(ImageSource source, RepeatBehavior repeatBehavior)
        {
            int num;
            CacheKey key = new CacheKey(source, repeatBehavior);
            _referenceCount.TryGetValue(key, out num);
            _referenceCount[key] = num + 1;
        }

        public static void RemoveAnimation(ImageSource source, RepeatBehavior repeatBehavior, ObjectAnimationUsingKeyFrames animation)
        {
            CacheKey key = new CacheKey(source, repeatBehavior);
            _animationCache.Remove(key);
        }

        private class CacheKey
        {
            private readonly ImageSource _source;
            private readonly RepeatBehavior _repeatBehavior;

            public CacheKey(ImageSource source, RepeatBehavior repeatBehavior)
            {
                this._source = source;
                this._repeatBehavior = repeatBehavior;
            }

            public override bool Equals(object obj) => 
                (!ReferenceEquals(null, obj) ? (!ReferenceEquals(this, obj) ? (ReferenceEquals(obj.GetType(), base.GetType()) ? this.Equals((AnimationCache.CacheKey) obj) : false) : true) : false);

            private bool Equals(AnimationCache.CacheKey other) => 
                (ImageEquals(this._source, other._source) && Equals(this._repeatBehavior, other._repeatBehavior));

            public override int GetHashCode() => 
                ((ImageGetHashCode(this._source) * 0x18d) ^ this._repeatBehavior.GetHashCode());

            private static Uri GetUri(ImageSource image)
            {
                BitmapImage image2 = image as BitmapImage;
                if ((image2 != null) && (image2.UriSource != null))
                {
                    if (image2.UriSource.IsAbsoluteUri)
                    {
                        return image2.UriSource;
                    }
                    if (image2.BaseUri != null)
                    {
                        return new Uri(image2.BaseUri, image2.UriSource);
                    }
                }
                BitmapFrame frame = image as BitmapFrame;
                if (frame != null)
                {
                    Uri uri;
                    string uriString = frame.ToString();
                    if ((uriString != frame.GetType().FullName) && Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                    {
                        if (uri.IsAbsoluteUri)
                        {
                            return uri;
                        }
                        if (frame.BaseUri != null)
                        {
                            return new Uri(frame.BaseUri, uri);
                        }
                    }
                }
                return null;
            }

            private static bool ImageEquals(ImageSource x, ImageSource y)
            {
                if (Equals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null) != ReferenceEquals(y, null))
                {
                    return false;
                }
                if (!ReferenceEquals(x.GetType(), y.GetType()))
                {
                    return false;
                }
                Uri uri = GetUri(x);
                Uri uri2 = GetUri(y);
                return ((uri != null) && (uri == uri2));
            }

            private static int ImageGetHashCode(ImageSource image)
            {
                if (image != null)
                {
                    Uri uri = GetUri(image);
                    if (uri != null)
                    {
                        return uri.GetHashCode();
                    }
                }
                return 0;
            }
        }
    }
}

