#!/usr/bin/env bash
set -e
set -o pipefail

TOOL=$1
sed -n "s/$TOOL \(.*\)/\1/p" ".tool-versions"
