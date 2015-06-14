using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SzLimiter {
    public interface ILimitStore {
        bool Limit(string _appKey, string limitKey);
        void SetLimit(int hitsPerMinute);
        int GetLimit();
    }
}
