using System.Collections.Generic;

namespace GitCleanup
{
    public class CleanupSettings
    {
        public string Path { get; set; }
        public RunMode RunMode { get; set; }
        public string OutFileName { get; set; }
        public AuthType AuthType { get; set; }
        public int MaxDaysSinceMerged { get; set; }
        public int MaxDaysSinceOrphan { get; set; }
        public int BatchSize { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> ExclusionMasks { get; set; }
    }
}