#!/bin/bash

if [ $# -lt 2 ]; then
  echo "Please specify the test name and the new service name"
  exit 1
fi

# Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")
# folder this script is in
scriptDir=$(dirname "$scriptPath")
# template folder
templateDir="$scriptDir/systest-template/Svc1"

testName="${1%/}"
serviceName="$2"
targetDir="$scriptDir/$testName/$serviceName"
mkdir -p $targetDir
namespace="$testName"
namespaceLower=$(echo $namespace | tr '[:upper:]' '[:lower:]')

echo Adding test $serviceName to project $testName in $targetDir

for file in $(find "$templateDir/" -type f)
do
    if [[ "$file" != *"/bin/"* ]] && [[ "$file" != *"/obj/"* ]] && [[ "$file" != *"-systest-"* ]]; then 
        tfile=${file/$templateDir/$targetDir}
        trfile=${tfile/Svc1/$serviceName}
        echo $trfile
        newDir=$(dirname "$trfile")
        mkdir -p $newDir
        sed -e "s|Svc1|$serviceName|g" -e "s|_NAMESPACE|$namespace|g" -e "s|_namespace|$namespaceLower|g" $file > $trfile 
    fi
done 
