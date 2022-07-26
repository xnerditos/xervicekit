#!/bin/bash

echo This script will run all tests if no test name is supplied, or  
echo it will run just one and follow the log output
echo 

# Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")
# Absolute path this script is in
scriptDir=$(dirname "$scriptPath")

if [ $# -eq 0 ]; then
  dotnet test "$scriptDir"
else 
  dotnet test "$scriptDir" --filter "$1"
fi
