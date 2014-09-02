#!/bin/sh
cd "${0%/*}"
dd if=/dev/zero of=/tmp/BeesNTrees.dmg bs=1M count=2
mkfs.hfsplus -v BeesNTrees /tmp/BeesNTrees.dmg
mount -o loop /tmp/BeesNTrees.dmg ../Temp/mnt
cp -dpR ../MacApp/BeesNTrees ../Temp/mnt/BeesNTrees.app
cp -dpR ../MacApp/Applications ../Temp/mnt/Applications
umount ../Temp/mnt
