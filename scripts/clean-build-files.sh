#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")
cd $rootDir

find -type d -name obj -exec rm -fr {} \; &>/dev/null
find -type d -name bin -exec rm -fr {} \; &>/dev/null
find -type d -name testdatatmp -exec rm -fr {} \; &>/dev/null

cd $currFolder
