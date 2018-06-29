using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsHider
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<IntPtr, string> _hideforms = new Dictionary<IntPtr, string>();
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            hideWindow();
        }

        private void hideWindow()
        {
            var hWnd = Win32Api.GetForegroundWindow();
            if (!Win32Api.ShowWindow(hWnd, Win32Api.SW_HIDE))
            {
                return;
            }
            var length = Win32Api.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            Win32Api.GetWindowText(hWnd, sb, sb.Capacity);
            _hideforms.Add(hWnd, sb.ToString());
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                ((NotifyIcon) sender).Visible = false;
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon.Visible = true;
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            // 隐藏
            HotKey.RegisterHotKey(Handle, HotKey.HIDE, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Shift, Keys.H);
            // 显示
            HotKey.RegisterHotKey(Handle, HotKey.SHOW, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Shift, Keys.Q);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case HotKey.WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case HotKey.HIDE:    //按下的是Ctrl+Shift+H
                            
                            hideWindow();
                            break;
                        case HotKey.SHOW:    //按下的是Ctrl+Shift+Q
                            //此处填写快捷键响应代码
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, HotKey.HIDE);
            HotKey.UnregisterHotKey(Handle, HotKey.SHOW);

            foreach (var form in _hideforms)
            {
                Win32Api.ShowWindow(form.Key, Win32Api.SW_SHOW);
            }
            _hideforms.Clear();
        }

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"是否确认退出程序？", @"退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            // 关闭所有的线程
            this.Dispose();
            this.Close();
        }
    }
}
