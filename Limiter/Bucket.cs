using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limtr.Lib {
    /// <summary>
    /// Represents the setup values for a bucket.
    /// </summary>
    public class Bucket {
        //public Bucket(string appKey, string bucket = null, long hitLimit = 10, TimeSpan limitInterval = default(TimeSpan)) 
        //: this(appKey, bucket, hitLimit, limitInterval, null, null, null)
        //{
        //}

        public Bucket(
            string appKey,
            string name = null,
            long hitLimit = 10,
            TimeSpan limitInterval = default(TimeSpan),
            long? throttleLimit = null,
            TimeSpan? throttleInterval = null,
            TimeSpan? throttleDelay = null) 
        {
            if (appKey == null) throw new ArgumentNullException("appKey");
            if (string.IsNullOrWhiteSpace(appKey)) throw new ArgumentException("The appKey must have a non-whitespace value", appKey);
            if (name == null) name = "default";
            if (throttleLimit.HasValue) {
                if (throttleLimit <= 0) throw new ArgumentOutOfRangeException("throttleLimit");
                if (!throttleInterval.HasValue) throw new ArgumentException("throttleInterval must be supplied if throttleLimit is supplied.", "throttleInterval");
                if (!throttleDelay.HasValue) throw new ArgumentException("throttleDelay must be supplied if throttleLimit is supplied.", "throttleDelay");
                if (throttleInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("throttleInterval");
            }
            AppKey = appKey;
            Name = name;
            HitLimit = hitLimit;
            LimitInterval = limitInterval;
            ThrottleLimit = throttleLimit;
            ThrottleInterval= throttleInterval;
            ThrottleDelay=throttleDelay;

        }

        public string AppKey { get; set; }
        public string Name { get; set; }
        public long HitLimit { get; set; }
        public TimeSpan LimitInterval { get; set; }
        public long? ThrottleLimit { get; set; }
        public TimeSpan? ThrottleInterval { get; set; }
        public TimeSpan? ThrottleDelay { get; set; }

        public bool Throttles {
            get {
                return ThrottleLimit.HasValue;
            }
        }

        public string KeyPrefix {
            get {
                return string.Format("buckets:{0}:{1}", AppKey, Name); 
            }
        }

        public string HitKeyFor(string operationKey) {
            return string.Format("hits:{0}:{1}:{2}", AppKey, Name, operationKey);
        }
    }
}
