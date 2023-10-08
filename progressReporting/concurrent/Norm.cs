using System;
using System.Collections.Generic;

namespace progressReporting.concurrent;

public delegate TProgress Norm<TProgress>(in TProgress update, in IEnumerable<TProgress> state);

public static class Norm
{
    public static TProgress Max<TProgress>(in TProgress update, in IEnumerable<TProgress> state)
        where TProgress : IComparable<TProgress>
    {
        var norm = update;
        foreach(var candidate in state) {
            if(norm.CompareTo(candidate) < 0) {
                norm = candidate;
            }
        }
        return norm;
    }
    public static TProgress Min<TProgress>(in TProgress update, in IEnumerable<TProgress> state)
        where TProgress : IComparable<TProgress>
    {
        var norm = update;
        foreach(var candidate in state) {
            if(candidate.CompareTo(norm) < 0) {
                norm = candidate;
            }
        }
        return norm;
    }
}