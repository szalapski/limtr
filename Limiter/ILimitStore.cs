﻿using System;
using System.Collections.Generic;

namespace Limtr.Lib {
    public interface ILimitStore {
        bool Allows(string appKey, string bucketName, string operationKey);
        bool IsAllowed(string appKey, string bucketName, string operationKey);
        void Setup(Bucket bucket);
        Bucket LoadBucket(string appKey, string name);
        IEnumerable<Bucket> LoadBuckets(string appKey);
    }
}