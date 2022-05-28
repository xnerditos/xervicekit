#!/bin/bash

if [ $# -eq 0 ]; then
  echo "Please specify the new test name"
  exit 1
fi

newProjLower=$(echo $newProj | tr '[:upper:]' '[:lower:]')

# Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")
# folder this script is in
scriptDir=$(dirname "$scriptPath")
# template folder
templateDir="$scriptDir/systest-template"

newProj="${1%/}"
targetDir="$scriptDir/$newProj"
namespace="$newProj"
namespaceLower=$(echo $namespace | tr '[:upper:]' '[:lower:]')

echo Creating test project in $targetDir

for file in $(find "$templateDir/" -type f)
do
    if [[ "$file" != *"/bin/"* ]] && [[ "$file" != *"/obj/"* ]] && [[ "$file" != *"-systest-"* ]]; then 
        tfile=${file/$templateDir/$targetDir}
        trfile=${tfile/_NAMESPACE/$namespace}
        newDir=$(dirname "$trfile")
        mkdir -p $newDir
        sed -e "s|_NAMESPACE|$namespace|g" -e "s|_namespace|$namespaceLower|g" $file > $trfile 
    fi
done 
