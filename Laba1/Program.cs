using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Laba1
{
    class Process
    {
        public int number;
        public int time;
        public int[] resourses;
        public int[] child;
        public int[] parents;
        public bool done;
    };

    class Program
    {
        static void Main(string[] args)
        {
            Thread[] Streams = new Thread[8];

            for(int i=0; i<Streams.Length; i++)
            {
                Streams[i] = new Thread(GenerateChains);
                Streams[i].Start();
            }
        }

        static void GenerateChains()
        {
            for (int y = 0; y <= 10; y++)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                string[] text = File.ReadAllLines("X1_1.txt");
                int[] resourses = ProcessParams(text[1]);
                int limit = 10;
                Random rnd = new Random();
                List<Process> processes = new List<Process>();

                //Заполнение списка процессов
                for (int i = 2; i < 124; i++)
                {
                    int[] res = new int[4];
                    int[] paramsOfProcess = ProcessParams(text[i]);
                    int[] childProc = new int[paramsOfProcess.Length - 6];

                    for (int k = 0; k < childProc.Length; k++)
                        childProc[k] = paramsOfProcess[k + 6];

                    for (int j = 0; j < 4; j++)
                        res[j] = paramsOfProcess[j + 1];

                    Process process = new Process();
                    process.number = i - 1;
                    process.time = paramsOfProcess[0];
                    process.done = false;
                    process.resourses = res;
                    process.child = childProc;
                    process.parents = new int[0];

                    processes.Add(process);
                }

                //Заполнение массива родителей для каждого процесса
                foreach (Process p in processes)
                {
                    for (int x = 0; x < p.child.Length; x++)
                    {
                        Process found = processes.Find(l => l.number == p.child[x]);
                        Array.Resize(ref found.parents, found.parents.Length + 1);
                        found.parents[found.parents.Length - 1] = p.number;
                    }
                }

                //chain - список звеньев цепи, stack - список с процессами для обработки
                List<int> chain = new List<int>();
                List<int> stack = new List<int>();

                //Добавление 1-го процесса в chain
                chain.Add(processes[0].number);
                processes[0].done = true;

                //Добавление дочерних процессов 1-го процесса
                for (int c = 0; c < processes[0].child.Length; c++)
                    stack.Add(processes[0].child[c]);

                //генерация цепи
                List<int> newChain = new List<int>();
                newChain = Calc(rnd, processes, stack, chain, resourses, limit);

                Console.WriteLine(string.Join(",", newChain) +
                    "\r\n" + "Суммарное время цепи: " + sw.Elapsed);
            }
        }

        //Парсер
        static int[] ProcessParams(string proc)
        {
            Regex regex = new Regex(@"\d+");
            MatchCollection matches = regex.Matches(proc);
            int[] paramsOfProcess = new int[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                int x;
                int.TryParse(matches[i].Value, out x);
                paramsOfProcess[i] = x;
            }
            return paramsOfProcess;
        }

        //Проверка на возможность расчета процесса
        static bool PotentialCalc(Process p, int[] resourses, int limit)
        {
            bool potential;
            if (p.time <= limit)
            {
                int[] resOfProc = new int[4];
                    for (int i = 0; i < 4; i++)
                        resOfProc[i] = p.resourses[i];
                    for(int j=0; j<4; j++)
                    {
                        bool s = resOfProc[j] <= resourses[j];
                        if (s == false)
                        {
                            potential = false;
                            break;
                        }
                    }
                potential = true;
            }
            else
               potential = false;
            
            return potential;    
        }

        static List<int> CalcProc(List<Process> processes, int j, Process p, int limit, int[] resourses, List<int> chain, List<int> stack)
        {
            if (PotentialCalc(p, resourses, limit) == true)
            {
                limit -= p.time;
                for (int b = 0; b < p.resourses.Length; b++)
                    resourses[b] -= p.resourses[b];
                chain.Add(p.number);
                p.done = true;
                if (p.child.Length == 0)
                    stack.Remove(p.number);
                else
                {
                    for (int c = 0; c < p.child.Length; c++)
                    {
                        if(stack.Find(i=> i==p.child[c])==0 && chain.Find(i=>i==p.child[c])==0)
                            stack.Add(p.child[c]);
                    }
                    stack.Remove(p.number);
                }
                return chain;
            }
            else
            {
                Process lastProc = processes.Find(i => i.number == chain[j - 1]);
                Thread.Sleep(100 * lastProc.time);
                limit += lastProc.time;
                for (int x = 0; x < resourses.Length; x++)
                    resourses[x] += lastProc.resourses[x];
                return CalcProc(processes, j, p, limit, resourses, chain, stack);
            }
        }

        static List<int> Calc(Random rnd, List<Process> processes, List<int> stack, List<int> chain, int[] resourses, int limit)
        {
            for (int j = 1; j < 122; j++)
            {
                int lastElem = chain[j - 1];

                int x = GenerateDigit(rnd, stack.Count);
                Process p = processes.Find(i => i.number == stack[x]);

                chain = CalcProc(processes, j, p, limit, resourses, chain, stack);
            }

            Console.WriteLine(string.Join("|", stack));

            return chain;
        }

        static int GenerateDigit(Random rnd, int max)
        {
            return rnd.Next(max);
        }
    }
}