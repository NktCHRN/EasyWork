using Business.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tests.Comparers
{
    internal class UserOnProjectModelEqualityComparer : IEqualityComparer<UserOnProjectModel>
    {
        public bool Equals(UserOnProjectModel? x, UserOnProjectModel? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.UserId == y.UserId && x.ProjectId == y.ProjectId && x.IsManager == y.IsManager;
        }

        public int GetHashCode([DisallowNull] UserOnProjectModel obj)
        {
            return HashCode.Combine(obj.UserId, obj.ProjectId, obj.IsManager);
        }
    }
}
