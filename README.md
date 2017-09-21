# GitCleanup
Tiny tool to cleanup git repository. Delete merged and orphan branches

Usage is trivial:

##Ntlm Auth
'''
GitCleanup -d "path to local repo" -a Ntlm -e "Release/.*" "PROD$" "develop" -f orphans.txt    
'''
Checks repo and outputs candidate to orphans.txt

##Basic Auth & delete remotes
'''
GitCleanup -d "path to local repo" -u vasya -p Hello123 -e "Release/.*" "PROD$" "develop" -r Full    
'''
Checks repo and removes local and remote branches. Basic auth will be used.

Best regards,
Slava.
