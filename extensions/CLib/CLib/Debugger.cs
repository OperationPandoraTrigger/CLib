﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CLib
{
    public partial class Debugger : Form
    {
        public Debugger()
        {
            InitializeComponent();
#if Debug
            this.Show()
#else 
            this.Hide();
#endif
        }

        public void Log(object obj)
        {
            if (this.rtb_log.InvokeRequired)
            {
                this.rtb_log.Invoke(new Action(delegate {
                    this.Log(obj);
                }));
                return;
            }

            this.rtb_log.AppendText(obj.ToString() + "\n");
        }

        private void rtb_log_TextChanged(object sender, EventArgs e)
        {
            //this.rtb_log.SelectionStart = this.rtb_log.Text.Length;
            //this.rtb_log.ScrollToCaret();
        }

        public void Toggle()
        {
            if (this.rtb_log.InvokeRequired)
            {
                this.rtb_log.Invoke(new Action(delegate {
                    this.Toggle();
                }));
                return;
            }
            if (this.Visible)
                this.Hide();
            else
                this.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.rtb_log.InvokeRequired)
            {
                this.rtb_log.Invoke(new Action(delegate {
                    this.OnFormClosing(e);
                }));
                return;
            }

            if (e.CloseReason != CloseReason.UserClosing)
            {
                Dispose(true);
                Application.Exit();
            }
            else
            {
                this.Hide();
            }
        }



    }
}
