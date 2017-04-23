using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelWhimsy
{
    public class TimeWatcher
    {
        DateTime start;
        DateTime end;
        double spanSeconds;

        public TimeWatcher()
        {
            Init(DateTime.Now, DateTime.Now.AddYears(100));
        }

        public TimeWatcher(double intervalSeconds)
        {
            Init(DateTime.Now, DateTime.Now.AddSeconds(intervalSeconds));
        }

        public TimeWatcher(DateTime endTime)
        {
            Init(DateTime.Now, endTime);
        }

        void Init(DateTime startTime, DateTime endTime)
        {
            start = startTime;
            end = endTime;
            spanSeconds = (end - start).TotalSeconds;
        }

        public void Reset(double newIntervalSeconds)
        {
            Init(DateTime.Now, DateTime.Now.AddSeconds(newIntervalSeconds));
        }

        public void Reset()
        {
            Init(DateTime.Now, DateTime.Now.AddSeconds(spanSeconds));
        }

        public static bool operator >(TimeWatcher first, TimeWatcher second)
        {
            return first.start > second.start;
        }

        public static bool operator <(TimeWatcher first, TimeWatcher second)
        {
            return first.start < second.start;
        }

        public static bool operator ==(TimeWatcher first, TimeWatcher second)
        {
            object firstObject = first as object;
            object secondObject = second as object;
            if (firstObject == null && secondObject == null) return true;
            if (firstObject == null) return false;
            return first.Equals(second);
        }

        public static bool operator !=(TimeWatcher first, TimeWatcher second)
        {
            return !first.Equals(second);
        }

        public override int GetHashCode()
        {
            return start.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TimeWatcher otherWatcher = obj as TimeWatcher;
            if (otherWatcher == null) return false;
            return this.start == otherWatcher.start && this.end == otherWatcher.end;
        }

        public float ElapsedSeconds
        {
            get
            {
                double returnValue = (DateTime.Now - start).TotalSeconds;
                return (float)returnValue;
            }
        }

        public bool Expired
        {
            get { return ElapsedSeconds > spanSeconds; }
        }

        public double SecondsLeft
        {
            get
            {
                double returnValue = spanSeconds - ElapsedSeconds;
                if (returnValue < 0) returnValue = 0;
                return returnValue;
            }
        }

        public double FractionLeft
        {
            get
            {
                return SecondsLeft / spanSeconds;
            }
        }
    }

}
