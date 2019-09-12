rem SET config=net46
rem SET config=net472
set config=netcoreapp2.2\win-x64

.\bin\Release\%config%\ExternalSort.Net.exe C:\\Work\\Job\\input.txt --n=4 --regenerate_ --debug_ >> exec_out.log