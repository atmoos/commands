using System;


namespace commands.progress
{
    public struct State
    {
        public String Process { get; }
        public Double Progress { get; }
        internal State(String process, Double progress)
        {
            Process = process;
            Progress = progress;
        }
    }
}