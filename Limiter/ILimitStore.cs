using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SzLimiter {
    public interface ILimitStore {
        bool Allows(string _appKey, string limitKey);
        bool IsAllowed(string _appKey, string limitKey);
    }
}
