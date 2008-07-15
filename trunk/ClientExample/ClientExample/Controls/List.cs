using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using FlowLib.Events;

namespace ClientExample.Controls
{
    public partial class List : UserControl
    {
        protected int intControlsHeight = 0;
        protected delegate void ChangeItem(object obj);

        public ExpandableItems Items
        {
            get;
            protected set;
        }

        public List()
        {
            InitializeComponent();
            Items = new ExpandableItems();
            Items.Added += new EventHandler<FmdcEventArgs<int, ExpandablePanel>>(Items_Added);
            Items.Removed += new EventHandler<FmdcEventArgs<int, ExpandablePanel>>(Items_Removed);
        }

        void Items_Added(object sender, FmdcEventArgs<int, ExpandablePanel> e)
        {
            ExpandablePanel panel = e.Data;
            //ExpandablePanel panel = obj as ExpandablePanel;
            panel.Top = intControlsHeight;
            intControlsHeight += panel.Height;
            panel.ExpandedChanged += new EventHandler<FmdcEventArgs<int, object>>(panel_ExpandedChanged);
            flowLayoutPanel1.Controls.Add(panel);
            UpdateScrollbarMax();
            //Invoke(new ChangeItem(OnAdd), panel);
        }

        void OnAdd(object obj)
        {
            ExpandablePanel panel = obj as ExpandablePanel;
            panel.Top = intControlsHeight;
            intControlsHeight += panel.Height;
            panel.ExpandedChanged += new EventHandler<FmdcEventArgs<int, object>>(panel_ExpandedChanged);
            flowLayoutPanel1.Controls.Add(panel);
            UpdateScrollbarMax();
        }

        void OnRemove(object obj)
        {
            ExpandablePanel panel = obj as ExpandablePanel;
            flowLayoutPanel1.Controls.Remove(panel);
            intControlsHeight -= panel.Height;
            UpdateScrollbarMax();
        }

        void panel_ExpandedChanged(object sender, FmdcEventArgs<int, object> e)
        {
            ExpandablePanel panel = sender as ExpandablePanel;
            if (panel == null)
                return;
            int value = panel.Height - e.Action;
            intControlsHeight += value + 10;
            //if (panel.Expanded)
            //{
            //    intControlsHeight -= e.Action - panel.Height;
            //}
            //else
            //    intControlsHeight += panel.Height - e.Action;
            UpdateScrollbarMax();
        }

        void Items_Removed(object sender, FmdcEventArgs<int, ExpandablePanel> e)
        {
            e.Data.ExpandedChanged -= panel_ExpandedChanged;
            ExpandablePanel panel = e.Data;
            flowLayoutPanel1.Controls.Remove(panel);
            intControlsHeight -= panel.Height;
            UpdateScrollbarMax();

            //Invoke(new ChangeItem(OnRemove), e.Data);
        }
        protected void UpdateScrollbarMax()
        {
            vScrollBar1.Minimum = 1;
            vScrollBar1.Maximum = intControlsHeight - this.Height;
            flowLayoutPanel1.Height = intControlsHeight;
            vScrollBar1.Enabled = (flowLayoutPanel1.Height > this.Height);
                
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int value = e.OldValue - e.NewValue;
            flowLayoutPanel1.Top += value;
        }

        public class ExpandableItems// : List<ExpandablePanel>
        {
            public event EventHandler<FmdcEventArgs<int, ExpandablePanel>> Added;
            public event EventHandler<FmdcEventArgs<int, ExpandablePanel>> Removed;

            public ExpandableItems()
            {
                Added = new EventHandler<FmdcEventArgs<int, ExpandablePanel>>(ExpandableItems_Added);
                Removed = new EventHandler<FmdcEventArgs<int, ExpandablePanel>>(ExpandableItems_Removed);
            }

            void ExpandableItems_Removed(object sender, FmdcEventArgs<int, ExpandablePanel> e) { }
            void ExpandableItems_Added(object sender, FmdcEventArgs<int, ExpandablePanel> e) { }

            public new void Add(ExpandablePanel pnl)
            {
                //base.Add(pnl);
                Added(this, new FmdcEventArgs<int, ExpandablePanel>(1, pnl));
            }

            public new void AddRange(IEnumerable<ExpandablePanel> collection)
            {
                throw new NotImplementedException();
            }

            public new void Remove(ExpandablePanel pnl)
            {
                //base.Remove(pnl);
                Removed(this, new FmdcEventArgs<int, ExpandablePanel>(1, pnl)); 
            }

            public new void RemoveAll(Predicate<ExpandablePanel> match)
            {
                throw new NotImplementedException();
            }

            public new void RemoveAt(int pos)
            {
                throw new NotImplementedException();
            }

            public new void RemoveRange(int index, int count)
            {
                throw new NotImplementedException();
            }

            public new void Clear()
            {
                //int intCount = Count;
                //base.Clear();
                //Removed(this, new FmdcEventArgs<int, ExpandablePanel>(intCount));
                Removed(this, new FmdcEventArgs<int, ExpandablePanel>(0));
            }
        }
    }
}
