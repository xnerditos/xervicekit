#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")

if [[ $# -ne 1 ]]; then
   echo Specify new version number
fi

find $rootDir -name "*.csproj" -type f -exec $scriptDir/update-csproj-version.sh {} $1 \;
