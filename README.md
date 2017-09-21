# GitCleanup
Tiny tool to cleanup git repository. It deletes merged and orphan branches.
The tool searches for branches merged and left for at least a week (default value) and those which have some orphan development left for a longer period (6 months). 

Keep your repository clean.
Usage is trivial:

## Ntlm Auth
```
GitCleanup -d "path to local repo" -a Ntlm -e "Release/.*" "PROD$" "develop" -f orphans.txt    
```
Checks repo and outputs candidates to orphans.txt

## Basic Auth & delete remotes
```
GitCleanup -d "path to local repo" -u vasya -p Hello123 -e "Release/.*" "PROD$" "develop" -r Full    
```
Checks repo and removes local and remote branches. Basic auth will be used.

## Usage
```
GitCleanup usage: -d <directory> -u <user> -p <password>
Extra options:
   -d, --Directory  : Path to the local repository.
   -b, --Batch      : Delete remotes in batches. It s faster but fails sometime.
   -m, --Merged     : Maximum days since merged. 7 days by default
   -o, --Orphan     : Maximum days since orphan (has incomplete development). 180 days by default.
   -a, --AuthType   : Authentication type (Basic|Ntlm). Basic by default.
   -u, --UserName   : User name.
   -p, --Password   : Password.
   -r, --RunMode    : (List|Local|Full) run mode. Default is List.
   -f, --OutputFile : Output orphans to a file.
   -e, --Mask       : Set of regular expressions to mark branches for exclusion from processing.
```
