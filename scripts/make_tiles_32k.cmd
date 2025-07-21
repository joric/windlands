@echo off

python stitch_tiles.py C:/temp/tiles stitched_32k.png 32768 4096
py gentiles.py -t jpg -w 512 stitched_32k.png 0-6 ../tiles/jungle/

start "" "D:\Projects\github\windlands\trunk\index.html"
