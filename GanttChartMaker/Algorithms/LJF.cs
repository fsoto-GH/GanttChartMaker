﻿using GanttChartMaker.Algorithms.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GanttChartMaker.Algorithms;

class LJF : IAlgorithm, IPreemptive
{
    private List<Process> processes;

    public string Name => "Longest Job First";

    public bool CanBePreemptive()
    {
        return true;
    }

    public bool ConsidersPriority()
    {
        return false;
    }

    public bool NeedsQuantum()
    {
        return false;
    }

    public void SetData(List<Process> processes)
    {
        this.processes = processes;
    }

    public bool Preemptive { get; set; } = false;

    public List<Process> DoAlgorithm(bool withArrivalTime)
    {
        /* Sorts to find the first job to execute.
         * Then resort to obtain the most up-to-time longest job.
         */
        if (withArrivalTime && Preemptive)
        {
            List<Process> ordered = new List<Process>(), temp = new List<Process>();
            foreach (Process p in processes)
            {
                temp.Add(p);
            }
            double timer = 0;
            temp = temp.OrderBy(p => p.ArrivalTime).ThenByDescending(p => p.BurstTime).ThenBy(p => p.Name).ToList();
            while (temp.Count != 0)
            {
                if (timer < temp[0].ArrivalTime)
                {
                    ordered.Add(new Process("IDLE", temp[0].ArrivalTime - timer));
                    timer = temp[0].ArrivalTime;
                }
                /* Completion time is only assigned iff they are finishing. 
                 * This is important as the main avoids processes that have a completion time of zero.
                 * This avoids having to do extra work to fish out jobs that have been interrupted.
                 */
                if (temp[0].BurstTime <= 1)
                {
                    timer += temp[0].BurstTime;
                    temp[0].CompletionTime = timer;
                    if (ordered.Count > 0 && ordered[ordered.Count - 1].Name == temp[0].Name)
                    {
                        ordered[ordered.Count - 1].BurstTime += 1;
                        ordered[ordered.Count - 1].CompletionTime = temp[0].CompletionTime;
                    }
                    else
                    {
                        ordered.Add(temp[0]);
                    }
                    temp.RemoveAt(0);
                }
                else
                {
                    timer += 1;
                    temp[0].BurstTime -= 1;
                    Process adjusted = new Process(temp[0], 1);
                    if (ordered.Count > 0 && ordered[ordered.Count - 1].Name == adjusted.Name)
                    {
                        ordered[ordered.Count - 1].BurstTime += 1;
                    }
                    else
                    {
                        ordered.Add(adjusted);
                    }
                }
                // sort
                int lowestIndex = 0;
                for (int i = 0; i < temp.Count; i++)
                {
                    lowestIndex = i;
                    for (int j = i; j < temp.Count; j++)
                    {
                        bool bothArriven = temp[lowestIndex].ArrivalTime <= timer && temp[j].ArrivalTime <= timer;
                        if (bothArriven && temp[j].BurstTime > temp[lowestIndex].BurstTime)
                        {
                            lowestIndex = j;
                        }
                        else if (bothArriven && temp[j].BurstTime == temp[lowestIndex].BurstTime && temp[j].ArrivalTime < temp[lowestIndex].ArrivalTime)
                        {
                            lowestIndex = j;
                        }
                        else if (bothArriven && temp[j].BurstTime == temp[lowestIndex].BurstTime && temp[j].ArrivalTime == temp[lowestIndex].ArrivalTime && temp[j].Name.CompareTo(temp[lowestIndex].Name) < 0)
                        {
                            lowestIndex = j;
                        }
                    }
                    Process swap = temp[lowestIndex];
                    temp[lowestIndex] = temp[i];
                    temp[i] = swap;
                }
            }
            return ordered;
        }
        else if (withArrivalTime)
        /* Sorts to find first job to execute.
        * Job completes and a sort is done to find the next longest job.
        */
        {
            List<Process> ordered = new List<Process>(), temp = new List<Process>(processes);
            int lowestIndex;
            double timer = 0;
            temp = temp.OrderBy(p => p.ArrivalTime).ThenByDescending(p => p.BurstTime).ThenBy(p => p.Name).ToList();
            while (temp.Count != 0)
            {
                if (timer < temp[0].ArrivalTime)
                {
                    ordered.Add(new Process("IDLE", temp[0].ArrivalTime - timer));
                    timer = temp[0].ArrivalTime;
                }
                ordered.Add(temp[0]);
                timer += temp[0].BurstTime;
                temp[0].CompletionTime = timer;
                temp.RemoveAt(0);
                // sort
                for (int i = 0; i < temp.Count; i++)
                {
                    lowestIndex = i;
                    for (int j = i; j < temp.Count; j++)
                    {
                        bool bothArriven = temp[lowestIndex].ArrivalTime <= timer && temp[j].ArrivalTime <= timer;
                        if (bothArriven && temp[j].BurstTime > temp[lowestIndex].BurstTime)
                        {
                            lowestIndex = j;
                        }
                        else if (bothArriven && temp[j].BurstTime == temp[lowestIndex].BurstTime && temp[j].ArrivalTime < temp[lowestIndex].ArrivalTime)
                        {
                            lowestIndex = j;
                        }
                        else if (bothArriven && temp[j].BurstTime == temp[lowestIndex].BurstTime && temp[j].ArrivalTime == temp[lowestIndex].ArrivalTime && temp[j].Name.CompareTo(temp[lowestIndex].Name) < 0)
                        {
                            lowestIndex = j;
                        }
                    }
                    Process swap = temp[lowestIndex];
                    temp[lowestIndex] = temp[i];
                    temp[i] = swap;
                }
            }
            return ordered;
        }
        else if (!withArrivalTime && Preemptive)
        /* Unlike SJF, this process has an edge case where processes can be interrupted as their burst time lessens
         * and becomes less than another processes'
        */
        {
            List<Process> ordered = new List<Process>(), temp = new List<Process>();
            foreach (Process p in processes)
            {
                p.setProperties(p.Priority, p.BurstTime, 0);
                temp.Add(p);
            }
            double timer = 0;
            temp = temp.OrderByDescending(p => p.BurstTime).ThenBy(p => p.Name).ToList();
            while (temp.Count != 0)
            {
                if (temp[0].BurstTime <= 1)
                {
                    timer += temp[0].BurstTime;
                    temp[0].CompletionTime = timer;
                    if (ordered.Count > 0 && ordered[ordered.Count - 1].Name == temp[0].Name)
                    {
                        ordered[ordered.Count - 1].BurstTime += 1;
                        ordered[ordered.Count - 1].CompletionTime = temp[0].CompletionTime;
                    }
                    else
                    {
                        ordered.Add(temp[0]);
                    }
                    temp.RemoveAt(0);
                }
                else
                {
                    timer += 1;
                    temp[0].BurstTime -= 1;
                    Process adjusted = new Process(temp[0], 1);
                    if (ordered.Count > 0 && ordered[ordered.Count - 1].Name == adjusted.Name)
                    {
                        ordered[ordered.Count - 1].BurstTime += 1;
                    }
                    else
                    {
                        ordered.Add(adjusted);
                    }
                }
                // sort
                int lowestIndex = 0;
                for (int i = 0; i < temp.Count; i++)
                {
                    lowestIndex = i;
                    for (int j = i; j < temp.Count; j++)
                    {
                        if (temp[j].BurstTime > temp[lowestIndex].BurstTime)
                        {
                            lowestIndex = j;
                        }
                        else if (temp[j].BurstTime == temp[lowestIndex].BurstTime && temp[j].Name.CompareTo(temp[lowestIndex].Name) < 0)
                        {
                            lowestIndex = j;
                        }
                    }
                    Process swap = temp[lowestIndex];
                    temp[lowestIndex] = temp[i];
                    temp[i] = swap;
                }
            }
            return ordered;
        }
        else
        {
            /* NOTE! This method covers both the preemptive and non-preemptive when no arrival time is considered (aka zero).
             * This is because the sort is done by burst time, so that implies that no other process beyond the one in execution 
             * can have a shorter burst time.
             * This sorts by burst time and executes in order.
             */
            double timer = 0;
            processes = processes.OrderByDescending(p => p.BurstTime).ThenBy(p => p.Name).ToList();
            foreach (Process p in processes)
            {
                timer += p.BurstTime;
                p.CompletionTime = timer;
            }
            return processes.OrderByDescending(p => p.BurstTime).ThenBy(p => p.Name).ToList();
        }
    }

    public string OnHoverDescription() => "This algorithm takes the longest job available and executes it first.";
}