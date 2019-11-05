#!/bin/bash

aptUpdated=0

# make, gcc, patch, etc. get installed if not available
which gcc > /dev/null
if [[ $? -ne 0 ]]; then
    if [ $aptUpdated -eq 0 ]]; then
        apt update
        aptUpdated=1
    fi

    apt install -y build-essential
fi

which rename > /dev/null
if [[ $? -ne 0 ]]; then
    if [ $aptUpdated -eq 0 ]]; then
        apt update
        aptUpdated=1
    fi

    apt install -y rename
fi

echo core > /proc/sys/kernel/core_pattern
