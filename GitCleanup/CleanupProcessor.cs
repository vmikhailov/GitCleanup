using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace GitCleanup
{
    internal class CleanupProcessor
    {
        public static void Run(CleanupSettings settings)
        {
            try
            {
                using (var repo = new Repository(settings.Path))
                {
                    ProcessRepository(repo, settings);
                }
            }
            catch (Exception e) when(e.Message.Contains("too many redirects"))
            {
                Console.WriteLine($"Error: Authentication error");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        private static void ProcessRepository(Repository repo, CleanupSettings settings)
        {
            var remote = GetRemote(repo, settings);
            var branches = ScanBranches(repo);
            var orphans = FindAllOrphanBranches(branches, settings);

            if (orphans.Count > 0)
            {
                if (settings.OutFileName != null)
                {
                    using (var file = File.CreateText(settings.OutFileName))
                    {
                        ListBranches(orphans, file);
                    }
                }

                if (settings.RunMode == RunMode.List)
                {
                    if (settings.OutFileName == null)
                    {
                        ListBranches(orphans, Console.Out);
                    }
                }
                else
                {
                    RemoveBranches(repo, remote, orphans, settings);
                }
            }

            Console.WriteLine("Done");
        }

        private static Remote GetRemote(Repository repo, CleanupSettings settings)
        {
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (settings.RunMode == RunMode.Local)
            {
                Console.WriteLine($"Local mode. No fetch");
                return null;
            }

            if (remote != null)
            {
                Console.WriteLine($"Remote: {remote.Name}");
                Fetch(repo, remote, settings);
            }
            else
            {
                throw new ApplicationException("no remote repository found");
            }
            return remote;
        }

        private static void Fetch(Repository repo, Remote remote, CleanupSettings settings)
        {
            Console.WriteLine($"Refreshing repo");
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider =
                    CredentialsFactory.CreateHandler(settings.AuthType, settings.UserName, settings.Password),
                Prune = true
            };
            Commands.Fetch(repo, remote.Name, new string[0], fetchOptions, null);
        }

        private static List<BranchInfo> ScanBranches(Repository repo)
        {
            var cnt = repo.Branches.Count();
            var infos = new List<BranchInfo>();
            var cc = repo.Commits
                         .SelectMany(commit => commit.Parents.Select(x => x.Id),
                                     (commit, parent) => new KeyValuePair<ObjectId, ObjectId>(parent, commit.Id))
                         .ToList()
                         .ToLookup(x => x.Key, x => x.Value);
            var n = 0;
            Console.WriteLine($"Found {cnt} branches");
            var chars = Console.WindowWidth - 20;
            foreach (var branch in repo.Branches)
            {
                Console.Write($"Scanning: {++n} {branch.FriendlyName.PadRightOrLimit(chars)}\r");
                var bi = new BranchInfo
                {
                    LocalName = branch.CanonicalName,
                    RemoteName = branch.UpstreamBranchCanonicalName,
                    LastUpdate = branch.Tip.Author.When,
                    Author = branch.Tip.Author.Name,
                    Tip = branch.Tip,
                    Merged = cc[branch.Tip.Id].Any()
                };
                infos.Add(bi);
            }

            Console.WriteLine($"Scanning completed".PadRightOrLimit(chars));
            return infos;
        }

        private static List<BranchInfo> FindAllOrphanBranches(IEnumerable<BranchInfo> branches,
            CleanupSettings settings)
        {
            var now = DateTime.Now;

            var daysMerged = settings.MaxDaysSinceMerged;
            var daysOrphan = settings.MaxDaysSinceOrphan;

            var filtered = branches.Where(x =>
                                              (now - x.LastUpdate).Days >= daysMerged && x.Merged ||
                                              (now - x.LastUpdate).Days > daysOrphan && !x.Merged);

            //var masks = settings.ExclusionMasks?.Select(x => new Regex(WildCardToRegular(x))).ToList();
            var masks = settings.ExclusionMasks?.Select(x => new Regex(x)).ToList();

            if (masks != null)
            {
                filtered = filtered.Where(x => masks.All(y => !y.IsMatch(x.RemoteName)));
            }
            return filtered.Distinct().OrderBy(x => x.RemoteName).ToList();
        }

        private static void ListBranches(List<BranchInfo> orphans, TextWriter writer)
        {
            writer.WriteLine($"List of orphan branches");
            foreach (var bi in orphans.Where(x => !x.Merged).OrderBy(x => x.RemoteName))
            {
                writer.WriteLine($"{bi.RemoteName}");
            }

            writer.WriteLine($"\nList of merged branches");
            foreach (var bi in orphans.Where(x => x.Merged).OrderBy(x => x.RemoteName))
            {
                writer.WriteLine($"{bi.RemoteName}");
            }
        }

        private static void RemoveBranches(Repository repo, Remote remote, List<BranchInfo> orphans,
            CleanupSettings settings)
        {
            Console.WriteLine($"Found {orphans.Count()} orphan branches");
            Console.WriteLine("Cleaning up...");
            var pushOptions = new PushOptions
            {
                CredentialsProvider =
                    CredentialsFactory.CreateHandler(settings.AuthType, settings.UserName, settings.Password),
                OnPushStatusError = errors => Console.WriteLine($"{errors.Reference} - {errors.Message}.")
            };

            var chars = Console.WindowWidth - 20;
            foreach (var bi in orphans)
            {
                Console.Write($"Removing local: {bi.RemoteName.PadRightOrLimit(chars)}\r");
                repo.Branches.Remove(bi.LocalName);
            }
            Console.WriteLine($"Removing locals comleted".PadRightOrLimit(chars));

            if (settings.RunMode == RunMode.Full)
            {
                //now delete the branches on the remote
                var n = 0;
                chars = Console.WindowWidth - 30;
                foreach (var bis in orphans.Paged(settings.BatchSize))
                {
                    var remoteBranches = bis.Select(x => $":{x.RemoteName}").ToList();
                    repo.Network.Push(remote, pushRefSpecs: remoteBranches, pushOptions: pushOptions);
                    foreach (var bi in bis)
                    {
                        Console.Write($"Removing remote: {++n} {bi.RemoteName.PadRightOrLimit(chars)}\r");
                    }
                }
                Console.WriteLine($"Removing remotes completed".PadRightOrLimit(chars));
            }
        }
    }
}