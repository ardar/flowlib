using System;
using System.Collections.Generic;
using System.IO;

using FlowLib.Interfaces;
using FlowLib.Events;

namespace ClientExample.Utils
{
    public class Finder : IUpdate
    {
        public enum Events
        {
            MatchFound = 0
        }

        public event FmdcEventHandler Update;

        public bool Stop
        {
            get;
            set;
        }

        public string CurrentDir
        {
            get;
            protected set;
        }

        public string ProgramPath
        {
            get;
            protected set;
        }

        public Finder()
        {
            Update = new FmdcEventHandler(Finder_Update);
        }

        void Finder_Update(object sender, FmdcEventArgs e) { }

        public bool FindProgram(DirectoryInfo dir, string name)
        {
            return FindProgram(dir, name, 3);
        }

        public bool FindProgram(DirectoryInfo dir, string name, int deep)
        {
            try
            {
                CurrentDir = dir.FullName;
                Console.WriteLine(CurrentDir);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (Stop)
                        break;
                    if (file.Name.Equals(name))
                    {
                        ProgramPath = file.FullName;
                        FmdcEventArgs e = new FmdcEventArgs((int)Events.MatchFound, file.FullName);
                        Update(this, e);
                        if (!e.Handled)
                            return true;
                    }
                }

                deep--;
                if (!Stop && deep >= 0)
                {
                    DirectoryInfo[] directories = dir.GetDirectories();
                    foreach (DirectoryInfo currentDir in directories)
                    {
                        if (Stop)
                            break;
                        if (FindProgram(currentDir, name, deep))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

    }
}
