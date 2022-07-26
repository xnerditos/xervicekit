#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")

# push the tag
currentBranch=$(git rev-parse --abbrev-ref HEAD)
git checkout master &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error in switching to 'master'"
    exit 1
fi
tag="version-$(./current-version.sh)"
echo $tag

git tag "$tag" &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error in creating release tag '$tag' in local repo"
    exit 1
fi
echo "Pushing release tag '$tag' to remote"
git push origin $tag &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error pushing tag '$tag' to remote"
    exit 1
fi
git checkout "$currentBranch" &>/dev/null
echo "Done! :-0"

$scriptDir/publish-all.sh
