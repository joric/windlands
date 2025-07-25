@echo off

FOR %%i IN (jungle desert mountain) DO (
	ECHO Processing: %%i
	python stitch_tiles.py tiles/%%i %%i.png 32768 4096
	python gentiles.py -t jpg -w 512 %%i.png 0-6 ../tiles/%%i/

)

rem start "" "D:\Projects\github\windlands\trunk\index.html"
