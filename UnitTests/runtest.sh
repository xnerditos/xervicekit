#!/bin/bash
# Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")
# Absolute path this script is in
scriptDir=$(dirname "$scriptPath")

if [ $# -eq 0 ]; then
  dotnet test "$scriptDir" /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
else 
  dotnet test "$scriptDir" --filter "$1" /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
fi
