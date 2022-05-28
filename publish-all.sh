#!/bin/bash


currFolder="$(pwd)"
target="publish.sh"

ls XKit.Lib.* -d | while read -r proj
do 
  cd $proj
  echo Running $proj
  ./$target
  cd $currFolder
done

cd $currFolder
