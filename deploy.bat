@echo off
set hh=%time:~-11,2%
set /a hh=%hh%+100
set hh=%hh:~1%
set dateseed=%date:~6,4%%date:~3,2%%date:~0,2%.%hh%%time:~3,2%

if not exist \\zoo\share\software\OutlookReminder\resources goto offline

move \\zoo\share\software\OutlookReminder\Resources\reminder.dll \\zoo\share\software\OutlookReminder\Old\reminder.%dateseed%.dll
if errorlevel 1 goto die
echo Archived old to \\zoo\share\software\OutlookReminder\Old\reminder.%dateseed%.dll
move /y e:\home\BetterOutlookReminder\bin\Debug\BetterOutlookReminder.exe \\zoo\share\software\OutlookReminder\resources\reminder.dll

echo Done!

goto exit

:offline
echo Unable to access \\zoo\share
goto exit

:die
echo failed

:exit
