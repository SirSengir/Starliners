#!/bin/bash
cd "${0%/*}"
DYLD_FALLBACK_LIBRARY_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib:/lib:/usr/lib mono BLauncher.exe
