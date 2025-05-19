using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Jobs.BaseClasses.Execution;

namespace Foerster.Models.JobManagement
{
    public interface ITaskResult
    {
        string TaskName { get; }
        ExecutionStatus Status { get; }
        ExecutionResult Result { get; }
        HImage OutputImage { get; }
        // Region used for display purposes, the regions are merged with union2.
        HRegion? DisplayRegions { get; }
        List<int>? FaultyRegionIds { get; }
    }
}
