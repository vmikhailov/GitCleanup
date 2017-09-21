# GitCleanup
Tiny tool to cleanup git repository. Delete merged and orphan branches.
The tool searches for branches merged and left for at least a week (default value) and those which have some orphan development left for a longer period (6 months). 

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

Best regards,
Slava.
