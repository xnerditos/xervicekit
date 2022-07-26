#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")

if [[ $# -ne 1 ]]; then
   echo Specify new version number
   exit 1;
fi

version="$1"
shift

cd $rootDir

switch to master
currentBranch=$(git rev-parse --abbrev-ref HEAD)
git checkout master &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error in switching to 'master'"
    exit 1
fi
git diff-index --quiet HEAD --
if [[ "$?" -ne 0 ]]; then 
    echo "Error.  You cannot have any pending changes while creating a release."
    exit 1
fi
git pull
if [[ "$?" -ne 0 ]]; then 
    echo "Error pulling 'master'"
    exit 1
fi

function updatecsproj() {
    local csproj=$1
    xmlstarlet edit -L -u "/Project/PropertyGroup/Version" -v "$version" $csproj
    xmlstarlet edit -L -u "/Project/PropertyGroup/AssemblyVersion" -v "$version.0" $csproj
}

# update the version
find $rootDir -name "*.csproj" -type f | while read file; do updatecsproj "$file"; done;
git diff-index --quiet HEAD --
if [[ "$?" -ne 1 ]]; then 
    echo "Error.  Updating the csproj versions failed for some reason!"
    exit 1
fi
git add -A
git commit -m "Create release $version"

tag="version-$version"
echo $tag
git tag "$tag" &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error in creating release tag '$tag' in local repo"
    exit 1
fi
echo "Pushing release commit to remote"
git push &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error pushing release commit to remote"
    exit 1
fi

# push the tag
echo "Pushing release tag '$tag' to remote"
git push origin $tag &>/dev/null
if [[ "$?" -ne 0 ]]; then 
    echo "Error pushing tag '$tag' to remote"
    exit 1
fi

# publish
ls XKit.Lib.* -d | while read -r proj
do 
  cd $proj
  echo Publishing $proj
  ./publish.sh
done

git checkout "$currentBranch" &>/dev/null
cd $currFolder
echo "Done! :-0"

