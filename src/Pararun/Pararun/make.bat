@echo off
set CMD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set OPT1=/noconfig /nowarn:1701,1702,2008 /nostdlib+ /platform:anycpu /warn:4 /filealign:512 /optimize+ /target:exe /utf8output
set OPT2=/reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.CSharp.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.DataSetExtensions.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll"

%CMD% %OPT1% %OPT2% /out:pararun.exe  Pararun.cs

