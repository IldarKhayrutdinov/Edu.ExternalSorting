SET config=net46
rem SET config=net472
rem set config=netcoreapp2.2\win-x64

.\bin\Release\%config%\ExternalSort.Net.exe C:\\Work\\Job\\input.txt --buffer_size=1048576 --n=4 --debug=false --regenerate=true --generate_file_size=14857060 >> exec_out.log