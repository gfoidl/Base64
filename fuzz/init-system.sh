#!/bin/bash

aptUpdated=0

# make, gcc, patch, etc. get installed if not available
if ! command -v gcc > /dev/null
then
    if [[ $aptUpdated -eq 0 ]]; then
        apt update
        aptUpdated=1
    fi

    apt install -y build-essential
fi

if ! command -v rename > /dev/null
then
    if [[ $aptUpdated -eq 0 ]]; then
        apt update
        aptUpdated=1
    fi

    apt install -y rename
fi

echo core > /proc/sys/kernel/core_pattern
