# Pararun.exe
Windows用の、プログラム(バッチファイル）群を並列実行するコマンド。  
Batch file parallel execution command for Windows.

# 概要
スレッドを使って指定ディレクトリにあるプログラムファイルを並列実行するコマンドです。  
対象プログラムの種類は "*.bat"、"*.cmd"、"*.exe" です。

指定ディレクトリにある実行ファイルパスはキュー(FIFO)に格納されます。
生成されたスレッド群がこのキューから実行ファイルパスを取り出して実行します。
スレッドは並列に実行されるので、プログラムも並列実行されることになります。

全てのスレッドがキューから実行ファイルパスを読み出せなくなったら終了となります。 


# Pararun.exeのビルド
make.batを用意したので、Visual Studioが無くても.NET Frameworkを導入していればコンパイルできます。たぶん。  
csc.exeのパスだけ自分の環境様に書き換えしてください。

## Visual Studio 2015のコンパイラを使わない場合
    
    H:\Parallelrun\Pararun>type make.bat
    @echo off
    set CMD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
    rem set CMD="C:\Program Files (x86)\MSBuild\14.0\Bin\csc.exe"
    
    set OPT1=/noconfig /nowarn:1701,1702,2008 /nostdlib+ /platform:anycpu /warn:4 /filealign:512 /optimize+ /target:exe /utf8output
    set OPT2=/reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.CSharp.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.DataSetExtensions.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll"
    
    %CMD% %OPT1% %OPT2% /out:pararun.exe  Pararun.cs
    
    
    H:\Parallelrun\Pararun>make
    Microsoft (R) Visual C# Compiler version 4.6.1586.0
    for C# 5
    Copyright (C) Microsoft Corporation. All rights reserved.
    
    This compiler is provided as part of the Microsoft (R) .NET Framework, but only supports language versions up to C# 5, which is no longer the latest version. For compilers that support newer versions of the C# programming language, see http://go.microsoft.com/fwlink/?LinkID=533240
    
    H:\Parallelrun\Pararun>
    H:\Parallelrun\Pararun>pararun
    pararun [-ut] [-nr] -qs count folder [folder ...] [-qf count folder [folder ...]] [-qh count folder [folder ...]]
    
        -ut : Use "Task Class"
        -nr : Not reuse free threads.
        -qs : Use "s" queue.
        -qf : Use "f" queue.
        -qh : Use "h" queue.
      count : Number of the used thread.
     folder : Batch job stock folder.
    
    H:\Parallelrun\Pararun>
    

## Visual Studio 2015のコンパイラを使った場合
    
    H:\Parallelrun\Pararun>type make.bat
    @echo off
    rem set CMD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
    set CMD="C:\Program Files (x86)\MSBuild\14.0\Bin\csc.exe"
    
    set OPT1=/noconfig /nowarn:1701,1702,2008 /nostdlib+ /platform:anycpu /warn:4 /filealign:512 /optimize+ /target:exe /utf8output
    set OPT2=/reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.CSharp.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Data.DataSetExtensions.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll"
    
    %CMD% %OPT1% %OPT2% /out:pararun.exe  Pararun.cs
    
    
    H:\Parallelrun\Pararun>make
    Microsoft (R) Visual C# Compiler 繝舌・繧ｸ繝ｧ繝ｳ 1.2.0.60317
    Copyright (C) Microsoft Corporation. All rights reserved.
    
    H:\Parallelrun\Pararun>pararun
    pararun [-ut] [-nr] -qs count folder [folder ...] [-qf count folder [folder ...]] [-qh count folder [folder ...]]
    
        -ut : Use "Task Class"
        -nr : Not reuse free threads.
        -qs : Use "s" queue.
        -qf : Use "f" queue.
        -qh : Use "h" queue.
      count : Number of the used thread.
     folder : Batch job stock folder.
    
    H:\Parallelrun\Pararun>
    

# オプション等説明
    H:\JOB>pararun
    pararun [-ut] [-nr] -qs count folder [folder ...] [-qf count folder [folder ...]] [-qh count folder [folder ...]]
    
        -ut : Use "Task Class"
        -nr : Not reuse free threads.
        -qs : Use "s" queue.
        -qf : Use "f" queue.
        -qh : Use "h" queue.
      count : Number of the used thread.
     folder : Batch job stock folder.
    
    H:\JOB>

"-ut" は.NETのTASKクラスを使ったスレッド処理を指定します。通常は指定する必要はありません。こちらの方が速いという環境があったので用意しました。

"-nr" は各キューの処理で使用したスレッドを他キューの処理で使用させたくない場合に指定します。

"-qs"、"-qf"、"-qh" でそれぞれ sキュー、fキュー、hキューについてスレッド数(並列数)と処理対象ディレクトリを指定します。  
"-qs"、"-qf"、"-qh"が指定されなかった場合は、"-qs 4" が仮定されsキューを4スレッド(4並列)で処理することになります。

## 単一キューでの実行
sキューはデフォルトのキューで必ず使用されます。オプションが全く指定されていない場合、sキューに実行ファイルパスが格納され4スレッドで実行が行われます。

例えば以下の指定
    
    pararun .\job1 .\job2 .\job3
    
は、

* ディレクトリ .\job1、.\job2、.\job3 にある *.CMD、*.BAT、*.EXE、を sキュー に格納
* sキューの実行ファイルパスを4スレッド(4並列)で実行

を意味します。指定するディレクトリ名の最後に‟￥”をつけないでください。  
明示的にsキュー処理用のスレッド数を指定する場合、例えば、
    
    pararun -qs 6 .\job1 .\job2 .\job3`
    
の指定は、

* ディレクトリ .\job1、.\job2、.\job2 にある *.CMD、*.BAT、*.EXE、を sキュー に格納
* sキューの実行ファイルパスを6スレッド(6並列)で実行

を意味します。

## 複数キューでの実行

キューは3つまで利用できます。例えば、
    
    pararun .\job2 .\job1 -qf 3 .\job3 -qh 1 .\job4
    
の指定は、

* ディレクトリ .\job2、.\job1 にある *.CMD、*.BAT、*.EXE、を sキュー に格納
* ディレクトリ .\job3 にある *.CMD、*.BAT、*.EXE、を fキュー に格納
* ディレクトリ .\job4 にある *.CMD、*.BAT、*.EXE、を hキュー に格納
* sキューを4スレッド(4並列)で実行
* fキューを3スレッド(3並列)で実行
* hキューを1スレッド(1並列)で実行

を意味しますし、以下の指定、
    
    pararun -qs 8 .\job2 .\job1 -qf 3 .\job3 -qh 1 .\job4
    
の場合は、

* ディレクトリ .\job2、.\job1 にある *.CMD、*.BAT、*.EXE、を sキュー に格納
* ディレクトリ .\job3 にある *.CMD、*.BAT、*.EXE、を fキュー に格納
* ディレクトリ .\job4 にある *.CMD、*.BAT、*.EXE、を hキュー に格納
* sキューを8スレッド(8並列)で実行
* fキューを3スレッド(3並列)で実行
* hキューを1スレッド(1並列)で実行

を意味します。

## スレッドの再使用
"-nr" が指定されていなければ、処理対象キューが空になったスレッドは、他キューの処理を始めます。

* sキューの内容が全て実行されたのち、
 * まだfキューに処理されていない実行ファイルパスがあると、sキューで使っていたスレッドはfキューの処理を始めます。
 * まだhキューに処理されていない実行ファイルパスがあると、sキューで使っていたスレッドはhキューの処理を始めます。

* fキューの内容が全て実行されたのち、
 * まだsキューに処理されていない実行ファイルパスがあると、fキューで使っていたスレッドをsキューの処理を始めます。
 * まだhキューに処理されていない実行ファイルパスがあると、fキューで使っていたスレッドをhキューの処理を始めます。

* hキューの内容が全て実行されたのち、
 * まだsキューに処理されていない実行ファイルパスがあると、hキューで使っていたスレッドをsキューの処理を始めます。
 * まだfキューに処理されていない実行ファイルパスがあると、hキューで使っていたスレッドをfキューの処理を始めます。

# 実行例

以下は実行例です。

ディレクトリ H:\JOBにPARARUN.EXEがあり、H:\JOB\job1、H:\JOB\job2、H:\JOB\job3、のディレクトリがあります。  
ディレクトリ job1 には a1.bat, a2.bat, a3.bat が格納されています。  
ディレクトリ job2 には b1.bat, b2.bat, b3.bat が格納されています。  
ディレクトリ job3 には c1.bat, c2.bat, c3.bat が格納されています。  

時刻はローカルタイムです。  
タイムゾーンが日本に設定されているOSで実行したのであれば、2016/06/22 5:30:32 は素直に日本時間の 2016/06/22 5:30:32 になります。

s0～1がsキュー、f0～1がfキュー、h0がhキューの処理を行っているスレッドのスレッド名です。  
各スレッドで実行中の処理時間が5分以上かかっている場合、‟LongRun”のメッセージが出ます。  
スレッドが処理を終了すると、処理終了時のリターンコードを rcd=xx の形でログに出力しています。

全スレッドで‟Queue is empty.”のメッセージが出ると全ての実行ファイルパスが処理されたことになります。  

この例では5分35秒で9個のバッチファイル処理が完了したことになります。  
もし、すべてのバッチファイルを1つずつ直列に実行した場合、8分4秒ほどかかる計算になります。  
スレッドの数を増やせば並列度が増して一番処理時間のかかっている job3\b3.bat の05分29秒 まで短縮ができることになりますが、コマンド自体のオーバーヘッドなどもあるので程々で。

    
    H:\JOB>pararun -qs 2 .\job1 -qf 2 .\job2 -qh 1 .\job3
    
    2016/06/22 5:30:32,----, Thread reuse mode: True
    2016/06/22 5:30:32,----, Use Task Class: False
    2016/06/22 5:30:32,----, s queue is use 2 threads
    2016/06/22 5:30:32,----, f queue is use 2 threads
    2016/06/22 5:30:32,----, h queue is use 1 threads
    2016/06/22 5:30:32,----, total 5 threads use, 9 jobs enqueued.
    2016/06/22 5:30:32,----, Start    threads
    2016/06/22 5:30:32,  s0, Start    .\job1\a1.bat
    2016/06/22 5:30:32,  s1, Start    .\job1\b1.bat
    2016/06/22 5:30:32,  f1, Start    .\job2\a2.bat
    2016/06/22 5:30:32,  h0, Start    .\job3\a3.bat
    2016/06/22 5:30:32,  f0, Start    .\job2\b2.bat
    2016/06/22 5:30:35,  f0, Finish   .\job2\b2.bat, 00:00:03.0739841, rcd=13
    2016/06/22 5:30:35,  f0, Start    .\job2\c2.bat
    2016/06/22 5:30:35,  f1, Finish   .\job2\a2.bat, 00:00:03.0994884, rcd=0
    2016/06/22 5:30:35,  f1, Start    .\job1\c1.bat
    2016/06/22 5:30:37,  s0, Finish   .\job1\a1.bat, 00:00:05.1208972, rcd=0
    2016/06/22 5:30:37,  s0, Start    .\job3\b3.bat
    2016/06/22 5:30:38,  f0, Finish   .\job2\c2.bat, 00:00:03.0000954, rcd=0
    2016/06/22 5:30:38,  f0, Start    .\job3\c3.bat
    2016/06/22 5:30:40,  f1, Finish   .\job1\c1.bat, 00:00:04.9450942, rcd=0
    2016/06/22 5:30:40,  f1, Queue is empty.
    2016/06/22 5:31:02,  s1, Finish   .\job1\b1.bat, 00:00:30.1207724, rcd=0
    2016/06/22 5:31:02,  s1, Queue is empty.
    2016/06/22 5:31:22,  h0, Finish   .\job3\a3.bat, 00:00:50.0874377, rcd=0
    2016/06/22 5:31:22,  h0, Queue is empty.
    2016/06/22 5:31:33,  f0, Finish   .\job3\c3.bat, 00:00:55.0596714, rcd=0
    2016/06/22 5:31:33,  f0, Queue is empty.
    2016/06/22 5:35:37,  s0, LongRun  .\job3\b3.bat
    2016/06/22 5:36:07,  s0, Finish   .\job3\b3.bat, 00:05:29.9256878, rcd=0
    2016/06/22 5:36:07,  s0, Queue is empty.
    2016/06/22 5:36:07,----, End      threads, 00:05:35.1250946
    2016/06/22 5:36:07,----, Retcode    0 :     8 jobs.
    2016/06/22 5:36:07,----, Retcode   13 :     1 jobs.
    H:\JOB>
    
