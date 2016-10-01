// Pararun
//
// Copyright (C) 2015,2016 Hideki Gotoh ( k896951 )
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pararun
{
    class Program
    {
        static int longexecspan = 5 * 60 * 1000;  // 5 min

        static void Main(string[] args)
        {
            DateTime pSt = DateTime.Now;
            DateTime pEt;
            Action[] ta;
            Task[] tary = null;

            jobCollector cj = new jobCollector(args);

            if (0 == cj.getJobsTotal)
            {
                help();
                Environment.Exit(0);
            }

            ta = new Action[cj.getThreadsTotal];
            if (true == cj.getUserTaskClassFlag)
            {
                tary = new Task[cj.getThreadsTotal];
            }

            Console.WriteLine("{0},{1,4}, Thread reuse mode: {2}", DateTime.Now, "----", cj.getReuseThreadFlag);
            Console.WriteLine("{0},{1,4}, Use Task Class: {2}", DateTime.Now, "----", cj.getUserTaskClassFlag);

            Console.WriteLine("{0},{1,4}, s queue is use {2} threads", DateTime.Now, "----", cj.getSlimCount);
            Console.WriteLine("{0},{1,4}, f queue is use {2} threads", DateTime.Now, "----", cj.getFatCount);
            Console.WriteLine("{0},{1,4}, h queue is use {2} threads", DateTime.Now, "----", cj.getHeavyCount);
            Console.WriteLine("{0},{1,4}, total {2} threads use, {3} jobs enqueued.", DateTime.Now, "----", cj.getThreadsTotal, cj.getJobsTotal);

            if (true == cj.getUserTaskClassFlag)
            {
                Console.WriteLine("{0},{1,4}, Start    threads", DateTime.Now, "----");
            }

            for (int i = cj.getThreadsTotal - 1; i > -1; i--)
            {
                int ii = i;

                jobCollector.useQueue uq = cj.threadIdxToQueue(ii);

                ta[ii] = new Action(() =>
                {
                    Process pa = new Process();

                    while (true)
                    {
                        try
                        {
                            DateTime et;
                            DateTime st;
                            String cmdline = cj.dequeue(uq);

                            if (null == cmdline)
                            {
                                Console.WriteLine("{0},{1,4}, Queue is empty.", DateTime.Now, cj.getThreadName(ii));
                                break;
                            }
                            pa.StartInfo.FileName = "CMD.EXE";
                            pa.StartInfo.Arguments = "/C;" + "\"" + cmdline + "\"";
                            pa.StartInfo.UseShellExecute = false;
                            pa.StartInfo.CreateNoWindow = true;
                            pa.StartInfo.ErrorDialog = false;

                            st = DateTime.Now;
                            Console.WriteLine("{0},{1,4}, Start    {2}", st, cj.getThreadName(ii), cmdline);
                            pa.Start();

                            pa.WaitForExit(longexecspan);
                            if (false == pa.HasExited)
                            {
                                Console.WriteLine("{0},{1,4}, LongRun  {2}", DateTime.Now, cj.getThreadName(ii), cmdline);
                                pa.WaitForExit();
                            }
                            et = DateTime.Now;
                            cj.setRetCodeCount(pa.ExitCode);

                            Console.WriteLine("{0},{1,4}, Finish   {2}, {3}, rcd={4}", et, cj.getThreadName(ii), cmdline, et - st, pa.ExitCode);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0},{1,4}, Error    {2}", DateTime.Now, cj.getThreadName(ii), e.Message + " " + e.StackTrace);
                            break;
                        }
                    }

                    pa.Close();
                });

                if (true == cj.getUserTaskClassFlag)
                {
                    tary[i] = new Task(ta[i]);
                    tary[i].Start();
                }

            }

            if (true == cj.getUserTaskClassFlag)
            {
                Task.WaitAll(tary);
            }
            else
            {
                Console.WriteLine("{0},{1,4}, Start    threads", DateTime.Now, "----");
                Parallel.Invoke(ta);
            }

            pEt = DateTime.Now;
            Console.WriteLine("{0},{1,4}, End      threads, {2}", pEt, "----", pEt - pSt);

            SortedDictionary<int, int> retlist = cj.getRetcdDic;
            if (retlist.Count != 0)
            {
                foreach (int retcd in retlist.Keys)
                {
                    Console.WriteLine("{0},{1,4}, Retcode {2,4} : {3,5} jobs.", pEt, "----", retcd, retlist[retcd]);
                }
            }

        }

        static public void help()
        {
            Console.WriteLine("pararun [-ut] [-nr] -qs count folder [folder ...] [-qf count folder [folder ...]] [-qh count folder [folder ...]]");
            Console.WriteLine("");
            Console.WriteLine("    -ut : Use \"Task Class\"");
            Console.WriteLine("    -nr : Not reuse free threads.");
            Console.WriteLine("    -qs : Use \"s\" queue.");
            Console.WriteLine("    -qf : Use \"f\" queue.");
            Console.WriteLine("    -qh : Use \"h\" queue.");
            Console.WriteLine("  count : Number of the used thread.");
            Console.WriteLine(" folder : Batch job stock folder.");
        }
    }

    class jobCollector
    {
        Regex regJobs = new Regex(@".*\.(:?[Cc][Mm][Dd]|[Bb][Aa][Tt]|[Ee][Xx][Ee])$");
        Regex regOThreads = new Regex(@"\d+");

        Int32 threadCountSlim = 4;
        Int32 threadCountFat = 0;
        Int32 threadCountHeavy = 0;
        Boolean notReuseThreadFlas = false;
        Boolean useTaskClassFlag = false;

        Queue jobQueueSlim;
        Queue jobQueueFat;
        Queue jobQueueHeavy;
        Queue[] jqAry;

        SortedDictionary<int, int> retcodeCollecter = new SortedDictionary<int, int>();

        public enum useQueue
        {
            slim,
            fat,
            heavy
        }

        public Int32 getSlimCount
        {
            get
            {
                return threadCountSlim;
            }
        }
        public Int32 getFatCount
        {
            get
            {
                return threadCountFat;
            }
        }
        public Int32 getHeavyCount
        {
            get
            {
                return threadCountHeavy;
            }
        }

        public jobCollector(String[] paramStrs)
        {
            jobQueueSlim = Queue.Synchronized(new Queue());
            jobQueueFat = Queue.Synchronized(new Queue());
            jobQueueHeavy = Queue.Synchronized(new Queue());

            jqAry = new Queue[] { jobQueueSlim,  jobQueueFat,  jobQueueHeavy,
                                   jobQueueFat,   jobQueueSlim, jobQueueHeavy,
                                   jobQueueHeavy, jobQueueSlim, jobQueueFat   };

            Int32 pLen = paramStrs.Length;
            Queue refQueue = jobQueueSlim;

            for (Int32 idx = 0; idx < pLen; idx++)
            {
                switch (paramStrs[idx])
                {
                    case @"-ut":
                        useTaskClassFlag = true;
                        continue;

                    case @"-nr":
                        notReuseThreadFlas = true;
                        continue;

                    case @"-qs":
                        if (idx < (pLen - 1))
                        {
                            threadCountSlim = setThreads(paramStrs[idx + 1], threadCountSlim);
                            refQueue = jobQueueSlim;
                            idx++;
                            continue;
                        }
                        break;

                    case @"-qf":
                        if (idx < (pLen - 1))
                        {
                            threadCountFat = setThreads(paramStrs[idx + 1], threadCountFat);
                            if (0 != threadCountFat) refQueue = jobQueueFat;
                            idx++;
                            continue;
                        }
                        break;

                    case @"-qh":
                        if (idx < (pLen - 1))
                        {
                            threadCountHeavy = setThreads(paramStrs[idx + 1], threadCountHeavy);
                            if (0 != threadCountHeavy) refQueue = jobQueueHeavy;
                            idx++;
                            continue;
                        }
                        break;
                }

                try
                {
                    foreach (String item in Directory.GetFiles(paramStrs[idx], "*", SearchOption.AllDirectories).Where(f => regJobs.IsMatch(f)).ToArray())
                    {
                        refQueue.Enqueue(item);
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message);
                }
            }
        }

        private Int32 setThreads(String s, Int32 tn)
        {
            Int32 ans = tn;

            if (regOThreads.IsMatch(s))
            {
                ans = Int32.Parse(s);
                if (0 >= ans) ans = tn;
            }
            return ans;
        }

        public String getThreadName(Int32 tNum)
        {
            String ans = "";

            if ((threadCountHeavy != 0) && (tNum >= (threadCountSlim + threadCountFat)))
            {
                ans = String.Format("h{0}", tNum - (threadCountSlim + threadCountFat));
            }
            else if ((threadCountFat != 0) && (tNum >= threadCountSlim))
            {
                ans = String.Format("f{0}", tNum - threadCountSlim);
            }
            else
            {
                ans = String.Format("s{0}", tNum);
            }
            return ans;
        }
        public Int32 getThreadsTotal
        {
            get
            {
                return threadCountSlim + threadCountFat + threadCountHeavy;
            }
        }
        public Int32 getJobsTotal
        {
            get
            {
                return jobQueueSlim.Count + jobQueueFat.Count + jobQueueHeavy.Count;
            }
        }
        public Boolean getReuseThreadFlag
        {
            get
            {
                return !notReuseThreadFlas;
            }
        }
        public Boolean getUserTaskClassFlag
        {
            get
            {
                return useTaskClassFlag;
            }
        }
        public SortedDictionary<int, int> getRetcdDic
        {
            get
            {
                return retcodeCollecter;
            }
        }

        public useQueue threadIdxToQueue(Int32 idx)
        {
            useQueue ans = useQueue.slim;

            if ((0 != threadCountFat) && (0 != threadCountHeavy))
            {
                if ((threadCountSlim <= idx) && (idx < (threadCountSlim + threadCountFat)))
                    ans = useQueue.fat;
                else if ((threadCountSlim + threadCountFat) <= idx)
                    ans = useQueue.heavy;
            }
            if ((0 != threadCountFat) && (0 == threadCountHeavy))
            {
                if (threadCountSlim <= idx)
                    ans = useQueue.fat;
            }
            if ((0 == threadCountFat) && (0 != threadCountHeavy))
            {
                if (threadCountSlim <= idx)
                    ans = useQueue.heavy;
            }

            return ans;
        }
        public String dequeue(useQueue q)
        {
            String ansStr = null;
            Int32 jqAryIdx = 0;

            switch (notReuseThreadFlas)
            {
                case false:
                    switch (q)
                    {
                        default:
                        case useQueue.slim:
                            jqAryIdx = 0;
                            break;

                        case useQueue.fat:
                            jqAryIdx = 3;
                            break;

                        case useQueue.heavy:
                            jqAryIdx = 6;
                            break;
                    }

                    lock (jqAry)
                    {
                        if (0 != jqAry[jqAryIdx + 0].Count)
                            ansStr = jqAry[jqAryIdx + 0].Dequeue() as String;
                        else if (0 != jqAry[jqAryIdx + 1].Count)
                            ansStr = jqAry[jqAryIdx + 1].Dequeue() as String;
                        else if (0 != jqAry[jqAryIdx + 2].Count)
                            ansStr = jqAry[jqAryIdx + 2].Dequeue() as String;
                    }

                    break;

                case true:
                    try
                    {
                        switch (q)
                        {
                            default:
                            case useQueue.slim:
                                ansStr = jobQueueSlim.Dequeue() as String;
                                break;

                            case useQueue.fat:
                                ansStr = jobQueueFat.Dequeue() as String;
                                break;

                            case useQueue.heavy:
                                ansStr = jobQueueHeavy.Dequeue() as String;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        ansStr = null;
                    }
                    break;
            }
            return ansStr;
        }
        public void setRetCodeCount(int rcd)
        {
            lock (retcodeCollecter)
            {
                if (retcodeCollecter.ContainsKey(rcd))
                {
                    retcodeCollecter[rcd] += 1;
                }
                else
                {
                    retcodeCollecter[rcd] = 1;
                }
            }
        }
    }
}