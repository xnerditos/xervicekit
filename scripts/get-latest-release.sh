#! /bin/bash

git fetch --tags
if [[ "$?" -ne 0 ]]; then 
    echo "Error in fetching tags from remote"
    exit 1
fi
git describe --tags
