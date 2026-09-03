This sample exists purely to demonstrate ZipadeeMaxVolumeSize: it splits its
output archive into multiple numbered volumes instead of one file.

data/large-asset.bin is a synthetic file generated only to make the archive
large enough to actually span more than one volume - it isn't meaningful
content, just bulk.

With ZipadeeMaxVolumeSize set well below the archive's total size, Zip
produces ConsoleVolumes.zip.001, ConsoleVolumes.zip.002, etc. instead of a
single ConsoleVolumes.zip.
