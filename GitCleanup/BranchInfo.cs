using System;
using LibGit2Sharp;

namespace GitCleanup
{
    public class BranchInfo
    {
        public string RemoteName { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public int NumberOfCommits { get; set; }
        public int NumberOfChangedFiles { get; set; }
        public Commit Tip { get; set; }
        public bool Merged { get; set; }
        public string LocalName { get; internal set; }
        public string Author { get; set; }

        public override int GetHashCode()
        {
            return RemoteName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var bi = obj as BranchInfo;
            return RemoteName.Equals(bi?.RemoteName);
        }
    }
}