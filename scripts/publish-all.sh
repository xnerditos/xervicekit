#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")
cd $rootDir
target="publish.sh"

ls XKit.Lib.* -d | while read -r proj
do 
  cd $proj
  echo Running $proj
  ./$target
done

cd $currFolder
