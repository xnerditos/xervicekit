#!/bin/bash

currFolder="$(pwd)"  # Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")  # Absolute path this script is in
scriptDir=$(dirname "$scriptPath")
cd $currFolder

echo
echo =============================================================
echo Building $proj in $(pwd)
echo =============================================================
echo
dotnet build

cd $currFolder
