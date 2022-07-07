using Business.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tests.Comparers
{
    internal class ProjectLimitsModelEqualityComparer : IEqualityComparer<ProjectLimitsModel>
    {
        public bool Equals(ProjectLimitsModel? x, ProjectLimitsModel? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.MaxToDo == y.MaxToDo && x.MaxInProgress == y.MaxInProgress && x.MaxValidate == y.MaxValidate;
        }

        public int GetHashCode([DisallowNull] ProjectLimitsModel obj)
        {
            return HashCode.Combine(obj.MaxToDo, obj.MaxInProgress, obj.MaxValidate);
        }
    }
}
