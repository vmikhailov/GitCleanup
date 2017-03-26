using System;

namespace GitCleanup
{
    public class BranchInfo
    {
        public string Name { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public int NumberOfCommits { get; set; }
        public int NumberOfChangedFiles { get; set; }
    }
}