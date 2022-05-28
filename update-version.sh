#!/bin/bash

if [[ $# -ne 1 ]]; then
   echo Specify new version number
fi

currFolder="$(pwd)"
find $currFolder -name "*.csproj" -type f -exec $currFolder/update-csproj-version.sh {} $1 \;
