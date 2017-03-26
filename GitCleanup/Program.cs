using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitCleanup
{
    public class CleanUpSettings
    {
        public bool LocalOnly { get; set; }
        public int DaysOld { get; set; }
        public int NumberOfChanges { get; set; }
    }

    internal class Program
    {
        private static readonly FetchOptions FetchOptions = new FetchOptions { CredentialsProvider = GetCredentials };
        private static readonly PushOptions PushOptions = new PushOptions { CredentialsProvider = GetCredentials };

        private static Credentials GetCredentials(string url, string usernameFromUrl,
            SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials
            {
                Username = "vyacheslav.mikhaylov"
            };
        }

        private static void Main(string[] args)
        {
            //var path = @"c:\Work\Monex\fxdb2";
            //var path = @"c:\Work\Monex\MonexAPI";
            var path = @"c:\Work\Research\GitCleanup";
            try
            {
                var settings = new CleanUpSettings()
                {
                    DaysOld = 30,
                    LocalOnly = true,
                    NumberOfChanges = 100
                };

                using (var repo = new Repository(path))
                {
                    ProcessRepository(repo, settings);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        private static void ProcessRepository(Repository repo, CleanUpSettings settings)
        {
            var remote = GetRemote(repo, settings);
            var branches = ScanBranches(repo);
            var orphans = FilterBranches(branches, settings);

            if (orphans.Count > 0)
            {
                RemoveBranches(repo, remote, orphans, settings);
            }

            Console.WriteLine("Done");
        }

        private static Remote GetRemote(Repository repo, CleanUpSettings settings)
        {
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote != null)
            {
                Console.WriteLine($"Remote: {remote.Name}");
                Fetch(repo, remote);
            }
            else
            {
                if (!settings.LocalOnly)
                {
                    throw new ApplicationException("no remote repository found");
                }
                Console.WriteLine($"Local mode");
            }
            return remote;
        }

        private static void Fetch(Repository repo, Remote remote)
        {
            Console.WriteLine($"Refreshing repo");
            Commands.Fetch(repo, remote.Name, new string[0], FetchOptions, null);
        }

        private static List<BranchInfo> ScanBranches(Repository repo)
        {
            var cnt = repo.Branches.Count();
            var infos = new List<BranchInfo>();
            var n = 0;

            Console.WriteLine($"Found {cnt} branches");
            foreach (var branch in repo.Branches)
            {
                Console.Write($"Scanning branches: {++n} {branch.FriendlyName.PadRightOrLimit(80)}\r");
                var bi = new BranchInfo
                {
                    Name = branch.FriendlyName,
                    LastUpdate = branch.Commits.First().Author.When,
                    NumberOfCommits = branch.Commits.Count(),
                    NumberOfChangedFiles = 0
                };
                var cmt = branch.Commits.Take(100).ToList();
                for (int i = 0; i < cmt.Count() - 1; i++)
                {
                    var tree1 = cmt[i].Tree;
                    var tree2 = cmt[i + 1].Tree;
                    var diff1 = repo.Diff;
                    //Patch
                    //TreeChanges
                    //PatchStats
                    var x = diff1.Compare<TreeChanges>(tree2, tree1);
                    if (x.Count > 0)
                    {
                        Console.WriteLine($"Added:{x.Added.Count()} Deleted:{x.Deleted.Count()} Modified:{x.Modified.Count()} Renamed:{x.Renamed.Count()} {cmt[i].Author.When} ");
                    }
                    //if (x.LinesAdded == 0)
                    //{
                    //    Console.WriteLine($"zero");
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"+{x.LinesAdded} -{x.LinesDeleted}");
                    //}
                }
                //ScanTree();
       
                infos.Add(bi);
            }

            Console.WriteLine($"Scanning branches: {"completed".PadRightOrLimit(80)}");
            return infos;
        }

        private static List<BranchInfo> FilterBranches(List<BranchInfo> branches, CleanUpSettings settings)
        {
            var now = DateTime.Now;
            return branches.Where(x => (now - x.LastUpdate).Days > settings.DaysOld).ToList();
        }

        private static void RemoveBranches(Repository repo, Remote remote, List<BranchInfo> orphans, CleanUpSettings settings)
        {
            Console.WriteLine($"Found {orphans.Count()} orphan branches");
            Console.WriteLine("Cleaning up...");
            var n = 0;
            foreach (var bi in orphans)
            {
                Console.Write($"Removing branches: {++n} {bi.Name.PadRightOrLimit(80)}\r");
                repo.Branches.Remove(bi.Name);
                //now delete the branches on the remote
                if (!settings.LocalOnly)
                {
                    repo.Network.Push(remote, $":{bi.Name}", PushOptions);
                }
            }

            Console.WriteLine($"Removing branches: {"completed".PadRightOrLimit(80)}");
        }

        private static void ScanTree(Tree tree)
        {
            foreach (var node in tree)
            {
                if (node.TargetType == TreeEntryTargetType.Tree)
                {
                    ScanTree((Tree)node.Target);
                }
                else
                {
                    Console.WriteLine($"{node.TargetType}: {node.Path}");
                    var blob = node.Target as Blob;
                }
            }
        }
    }
}