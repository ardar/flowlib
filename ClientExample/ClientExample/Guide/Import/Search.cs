using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ClientExample.Utils;

namespace ClientExample.Guide.Import
{
    public partial class Search : Form
    {
        int intDots = 0;
        bool isUpdating = false;
        Timer timerUpdateStatus = new Timer();
        //System.Collections.Generic.Queue<string> list = new Queue<string>();
        List<string> list = new List<string>();
        bool isListDirty = false;
        string strLastPath = string.Empty;
        System.Threading.Thread threadFinder = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(OnFinding));
        bool isDone = false;
        Finder finder = null;

        public bool IsDone
        {
            get { return isDone; }
            set { isDone = value; }
        }

        public List<string> List
        {
            get { return list; }
        }

        public string StartDirectory
        {
            get;
            set;
        }

        public Search()
        {
            InitializeComponent();
            timerUpdateStatus.Tick += new EventHandler(timerUpdateStatus_Tick);
            timerUpdateStatus.Interval = 500;
            timerUpdateStatus.Start();

            threadFinder.IsBackground = true;
            threadFinder.Start(this);

            finder = new Finder();
            finder.Update += new FlowLib.Events.FmdcEventHandler(finder_Update);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            finder.Stop = true;
            finder = null;
            base.OnClosing(e);
        }

        void finder_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch ((Finder.Events)e.Action)
            {
                case Finder.Events.MatchFound:
                    lock (list)
                    {
                        list.Add((string)e.Data);
                        isListDirty = true;
                        e.Handled = true;
                        //list.Enqueue((string)e.Data);
                    }
                    break;
            }
        }

        void timerUpdateStatus_Tick(object sender, EventArgs e)
        {
            if (isUpdating)
                return;
            isUpdating = true;
            string str = "Searching";
            for (int i = 0; i < intDots; i++)
                str += ".";
            Text = str;
            if (++intDots == 4)
                intDots = 0;

            if (isListDirty)
            {
                isListDirty = false;
                label2.Text = string.Format("Found {0} potential settings files.", list.Count);
            }

            if (!IsDone)
            {
                if (finder != null)
                {
                    lock (finder)
                    {
                        string path = finder.CurrentDir;
                        if (path != null && path.Length > 60)
                            path = path.Substring(0, 20) + " ... "  + path.Substring(path.Length - 40);
                        label1.Text = path;
                    }
                }
            }
            else
            {
                //label1.Text = SelectedPath;
                timerUpdateStatus.Stop();
                Close();
            }
            isUpdating = false;
        }

        private void Search_Load(object sender, EventArgs e)
        {

        }

        static private void OnFinding(object objS)
        {
            Search searchWindow = objS as Search;
            if (searchWindow != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(searchWindow.StartDirectory))
                    {
                        System.IO.DriveInfo[] drivs = System.IO.DriveInfo.GetDrives();
                        foreach (System.IO.DriveInfo drive in drivs)
                        {
                            if (drive.DriveType == System.IO.DriveType.Fixed)
                            {
                                if (searchWindow.finder.FindProgram(drive.RootDirectory, "Favorites.xml", 5))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        searchWindow.finder.FindProgram(new System.IO.DirectoryInfo(searchWindow.StartDirectory), "Favorites.xml", 3);
                    }
                }
                catch { }
                searchWindow.IsDone = true;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            IsDone = true;
            Close();
        }
    }
}
