using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextBox= System.Windows.Forms.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Reflection.Emit;
using Button = System.Windows.Forms.Button;
using System.Runtime.Remoting.Messaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Diagnostics.Eventing;
using Label = System.Windows.Forms.Label;

namespace Sodoko
{

    public partial class Form1 : Form
    {

        TextBox[,] textBoxList = new TextBox[9, 9];

        TextBox[,] itextBoxList = new TextBox[9, 9];

        List<int>[,] emptyList = new List<int>[9, 9];

        List<int>[,] iemptyList = new List<int>[9, 9];

        int num = -1;

        bool confirm = false;

        Button pl_co_y_te = new Button();
        public Form1()
        {
            InitializeComponent();
            Initialize();
        }
        public void Solve()
        {
            //initialize the background data
            //set all TextBox ,which already has number: ReadOnly = true
            //set all empty TextBox's list of possibilities:list[9]={1,2,3,4,5,6,7,8,9};
            AllFilledBoxReadonly(textBoxList);
            SetIniticialEmptyList();
            //use true List update virtual list
            UpdateList(textBoxList, emptyList, "true to virtual");
            //初始化空白格子的可能性
            foreach (var item in itextBoxList)
            {
                if (item.Text != "")
                {
                    DeleteRelativNummber(item, itextBoxList, iemptyList);
                }
            }
            //开始遍历每一个空iTextBox,如果遍历完数独还是没填完,启动猜数环节
            LoopEmptyTextBox(itextBoxList, iemptyList);

            if (!IfFinished(itextBoxList))
            {
                AllFilledBoxReadonly(itextBoxList);
                FillAllBoxes(itextBoxList, iemptyList);
            }
            //UpdateList(itextBoxList, iemptyList, "virtual to true");
            return;//填完了
        }
        /// <summary>
        /// to == "true to virtual"or"virtual to true"
        /// </summary>
        /// <param name="t_list"></param>
        /// <param name="e_list"></param>
        /// <param name="to"></param>
        public void UpdateList(TextBox[,] t_list, List<int>[,] e_list,string to)
        {
            
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    switch (to)
                    {
                        case "true to virtual":
                            if (t_list[i, j].Text != "")
                            {
                                int n = Convert.ToInt32(t_list[i, j].Text);
                                itextBoxList[i, j].Text = n.ToString();
                            }
                            else
                            {
                                int[] n = new int[e_list[i, j].Count];
                                for (int m = 0; m < e_list[i, j].Count; m++)
                                {
                                    n[m] = e_list[i, j][m];
                                    iemptyList[i, j] = n.ToList();
                                }
                            }
                            break;
                        case "virtual to true":
                            textBoxList = t_list;
                            emptyList = e_list;
                            break;
                        default:break;
                    }
                }
            }
        }
       
        public void FillAllBoxes(TextBox[,] tList, List<int>[,] eList)
        {
            for (int index = 0; index < 81 ; index++)
            {
                if (tList[index / 9, index % 9].ReadOnly == false)
                {
                    int ini = tList[index / 9, index % 9].Text == "" ? -1
                    : Convert.ToInt32(tList[index / 9, index % 9].Text);//得到GetNummber函数nummber的取值
                    num = GetNextNumber(eList[index / 9, index % 9], ini);
                    if (num==-1)
                    {
                        tList[index / 9, index % 9].Text = "";
                        //退回上一个可填的格子
                        index = BoxReturn(index) - 1;
                        continue;
                    }
                    bool ok = IfHandled(tList[index / 9, index % 9], num.ToString(),tList);

                    if (!ok)
                    {
                        //检查是不是最后一个数(是否有别的可能),可简化
                        //1.这是最后一个可能
                        if (eList[index / 9, index % 9].IndexOf(num) == eList[index / 9, index % 9].Count - 1)
                        {
                            tList[index / 9, index % 9].Text = "";
                            //退回上一个可填的格子
                            index = BoxReturn(index) - 1;
                        }
                        else//2.用下一个可能,用之前检查能否填进去.
                        {
                            bool givenummber=true;
                            num = GetNextNumber(eList[index / 9, index % 9], num);
                            while (IfHandled(tList[index / 9, index % 9], num.ToString(),tList)==false)//如果是错误的数,一直换下一个
                            {
                                //检查是不是最后一个数
                                if (eList[index / 9, index % 9].IndexOf(num) == eList[index / 9, index % 9].Count - 1)
                                {
                                    tList[index / 9, index % 9].Text = "";
                                    //退回上一个可填的格子
                                    index = BoxReturn(index) - 1;
                                    givenummber = false;
                                    break;
                                }
                                num = GetNextNumber(eList[index / 9, index % 9], num);
                            }
                            //到达这里有两种方式
                            //1.从上面的break,最后一个数且不合规
                            //2.数字合规
                            //数字不合规的不能赋值
                            if (givenummber==true)
                            {
                                tList[index / 9, index % 9].Text = num.ToString();
                            }
                            
                        }
                    }
                    else
                    {
                        tList[index / 9, index % 9].Text = num.ToString();
                    }
                }
            }
            return;
        }
        
        //根据当前数,取下一个数
        public int GetNextNumber(List<int> ints, int nummber = -1)
        {
            if (nummber == -1)
            {
                return ints[0];
            }
            if (ints.IndexOf(nummber) == ints.Count - 1)
            {
                return -1;
            }
            return ints[ints.IndexOf(nummber) + 1];
        }
        
        // 退回到上一个可填的格子
        public int BoxReturn(int index)
        {
            for (int i = index-1; i >=0; i--)
            {
                int a = i / 9;
                int b = i % 9;
                if (itextBoxList[a,b].ReadOnly==false)
                {
                    return i;
                }
            }
            return -1;
        }
        
        //遍历TextBox
        public void LoopEmptyTextBox(TextBox[,] textBoxList, List<int>[,] emptyList)
        {
            foreach (var item in textBoxList)
            {
                if (item.Text == "")//空格子
                {
                    //判断能不能填,能填填上
                    while (IfFilled(item,textBoxList,emptyList))
                    {
                        DeleteRelativNummber(item, textBoxList, emptyList);
                    }
                }
            }
        }
        public void AllFilledBoxReadonly(TextBox[,] t_list)
        {
            //设置所有题目位为只读
            foreach (var item in t_list)
            {
                if (item.Text != "")
                {
                    item.ReadOnly = true;
                }
            }
        }
        public void SetIniticialEmptyList()
        {
            int[] n = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            foreach (var item in textBoxList)
            {
                if (item.Text == "")
                {
                    List<int> emptyBox = n.ToList();
                    int a = item.TabIndex / 9;//行
                    int b = item.TabIndex % 9;//列
                    emptyList[a, b] = emptyBox;
                }
            }
        }
        
        // 检查在e_list中这个Box能不能解:能,返回TRUE;不能,返回false.
        public bool IfFilled(TextBox item, TextBox[,] tList, List<int>[,] eList)
        {
            int a = item.TabIndex / 9;//行
            int b = item.TabIndex % 9;//列
            if (eList[a, b].Count != 0)
            {
                int[] ints = eList[a, b].ToArray();
                string text = ints[0].ToString();
                if (eList[a, b].Count == 1 && IfHandled(item, text,tList))
                {
                    item.Text = text;
                    return true;
                }
            }
            return false;
        }
        
        // 根据aufgabe初始化界面,但是没解
        
        public void Initialize()
        {

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (((i >= 0 && i <= 2) || (i >= 6 && i <= 8)) && ((j >= 0 && j <= 2) || (j >= 6 && j <= 8)))
                    {

                        textBoxList[i, j] = Create1(i, j, System.Drawing.SystemColors.ActiveBorder);
                        itextBoxList[i, j] = Create2(i, j);
                    }
                    else if ((i >= 3 && i <= 5) && (j >= 3 && j <= 5))
                    {
                        textBoxList[i, j] = Create1(i, j, System.Drawing.SystemColors.ActiveBorder);
                        itextBoxList[i, j] = Create2(i, j);
                    }
                    else
                    {
                        textBoxList[i, j] = Create1(i, j, System.Drawing.SystemColors.ButtonFace);
                        itextBoxList[i, j] = Create2(i, j);
                    }
                }
            }
            //
            Button guide = new Button();
            CreateButton(guide, 0, 200, 400, 50, "Key function description");
            guide.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            guide.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            guide.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            guide.BackColor = System.Drawing.Color.PaleVioletRed;
            //
            Button add_task_help = new Button();
            CreateButton(add_task_help, 0, 250, 400, 50, "1.Add task manully.");
            add_task_help.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            add_task_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            add_task_help.BackColor = System.Drawing.Color.LavenderBlush;
            add_task_help.ForeColor = System.Drawing.SystemColors.GrayText; ;
            //
            Button confirm_Test_help = new Button();
            CreateButton(confirm_Test_help, 0, 300, 400, 150, "2.Click \"confirm my Test\" when you finish add task.\n" + "The solusion will be automatically created in backgroud.");
            confirm_Test_help.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            confirm_Test_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            confirm_Test_help.BackColor = System.Drawing.Color.LavenderBlush;
            confirm_Test_help.ForeColor = System.Drawing.SystemColors.GrayText; 
            //
            Button clear_all_help = new Button();
            CreateButton(clear_all_help, 0, 450, 400, 100, "3.Click \"clear all\" you can clear the map.\n" + "Data of this game will be deleted.");
            clear_all_help.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            clear_all_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            clear_all_help.BackColor = System.Drawing.Color.LavenderBlush;
            clear_all_help.ForeColor = System.Drawing.SystemColors.GrayText;
            //
            Button check_wrong_help = new Button();
            CreateButton(check_wrong_help, 0, 550, 400, 100, "4.In the process of playing, use \"check wrong\" button to check your incorrect numbers.");
            check_wrong_help.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            check_wrong_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            check_wrong_help.BackColor = System.Drawing.Color.LavenderBlush;
            check_wrong_help.ForeColor = System.Drawing.SystemColors.GrayText;
            //
            Button solve_help = new Button();
            CreateButton(solve_help, 0, 650, 400, 100, "5.Click \"solve\" button to get the whole solution.\n"+"Have fun!");
            solve_help.Font = new System.Drawing.Font("MV Boli", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            solve_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            solve_help.BackColor = System.Drawing.Color.LavenderBlush;
            solve_help.ForeColor = System.Drawing.SystemColors.GrayText;
            //button:confirm_Test
            Button confirm_Test = new Button();
            CreateButton(confirm_Test, this.Width / 2 + 290, this.Height / 2 - 360,140,85,"confirm\n" +"my Test");
            confirm_Test.Anchor = AnchorStyles.None;
            confirm_Test.MouseClick += Confirm_Test_MouseClick;
            
            //button :clear the map
            Button clear_all = new Button();
            CreateButton(clear_all, this.Width / 2 + 290, this.Height / 2 - 276, 140, 85, "clear the map");
            clear_all.Anchor = AnchorStyles.None;
            clear_all.MouseClick += Clear_all_MouseClick;
           
            //改错按钮
            Button check_wrong = new Button();
            CreateButton(check_wrong, this.Width / 2 + 290, this.Height / 2 - 192, 140, 85, "check wrong");
            check_wrong.Anchor = AnchorStyles.None;
            check_wrong.MouseClick += Check_wrong_MouseClick;
            //开始解决方案按钮
            Button readytogo = new Button();
            CreateButton(readytogo, this.Width / 2 + 290, this.Height / 2 -108, 140, 85, "solve");
            readytogo.Anchor = AnchorStyles.None;
            readytogo.MouseClick += Readytogo_MouseClick1;
            //
            pl_co_y_te.Size = new Size(353, 125);
            pl_co_y_te.Location = new Point(this.Width / 2 - 175, this.Height / 2 - 65);
            pl_co_y_te.Anchor = AnchorStyles.None;
            pl_co_y_te.BackColor = SystemColors.GradientInactiveCaption;
            pl_co_y_te.Font = new Font("MV Boli", 18.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            pl_co_y_te.ForeColor = SystemColors.GrayText;
            pl_co_y_te.Name = "plct";
            pl_co_y_te.Text = "please confirm your Test!";
            pl_co_y_te.UseVisualStyleBackColor = false;
            pl_co_y_te.Visible = false;
            pl_co_y_te.MouseClick += Pl_co_y_te_MouseClick;
            Controls.Add(pl_co_y_te);
            //
            PictureBox pictureBox1 = new System.Windows.Forms.PictureBox();
            CreatePictureBox(pictureBox1, 0);
            PictureBox pictureBox2 = new System.Windows.Forms.PictureBox();
            CreatePictureBox(pictureBox2, 500);
            PictureBox pictureBox3 = new System.Windows.Forms.PictureBox();
            CreatePictureBox(pictureBox3, 1000);
            PictureBox pictureBox4 = new System.Windows.Forms.PictureBox();
            CreatePictureBox(pictureBox4, 1500);
            //
            Label have_fun= new Label();
           have_fun.AutoSize = true;
           have_fun.Font = new System.Drawing.Font("Mistral", 60F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
           have_fun.ForeColor = System.Drawing.Color.PaleVioletRed;
           have_fun.Location = new System.Drawing.Point(this.Width/2-400,this.Height/2+370);
           have_fun.Name = "label1";
           have_fun.Size = new System.Drawing.Size(643, 150);
            have_fun.Anchor = AnchorStyles.None;
           have_fun.TabIndex = 0;
           have_fun.Text = "Have fun with Sudoko!";
           have_fun.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
           Controls.Add(have_fun);
            //
            Label helps=new Label();

            Controls.Add(helps);
        }
        public void CreatePictureBox(PictureBox pictureBox,int width)
        {
            ((System.ComponentModel.ISupportInitialize)(pictureBox)).BeginInit();
           
            pictureBox.Image = global::Sodoko.Properties.Resources.giphy__1_;
            pictureBox.Location = new System.Drawing.Point(width, 0);
            pictureBox.Size = new System.Drawing.Size(500, 150);
            pictureBox.TabStop = false;
            Controls.Add((System.Windows.Forms.PictureBox)(pictureBox));
            return;
        }
        private void Pl_co_y_te_MouseClick(object sender, MouseEventArgs e)
        {
            pl_co_y_te.Visible = false;
        }

        private void Readytogo_MouseClick1(object sender, MouseEventArgs e)
        {
            if (confirm == false)
            {
                pl_co_y_te.Visible = true;
                pl_co_y_te.BringToFront();
                return;
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (textBoxList[i, j].Text == "" || textBoxList[i, j].Text != itextBoxList[i, j].Text)
                    {
                        textBoxList[i, j].Text = itextBoxList[i, j].Text;
                        textBoxList[i, j].ForeColor = Color.Violet;
                    }

                }
            }
        }

        //use the gived Test to update all list and the window form
        private void Confirm_Test_MouseClick(object sender, MouseEventArgs e)
        {
            Solve();
            confirm = true;
        }
        private void Clear_all_MouseClick(object sender, MouseEventArgs e)
        {
            Clearthemap();
            this.Initialize();
        }
        public void Clearthemap()
        {
            this.Controls.Clear();
            this.InitializeComponent();
            textBoxList = new TextBox[9, 9];
            itextBoxList = new TextBox[9, 9];
            emptyList = new List<int>[9, 9];
            iemptyList = new List<int>[9, 9];
            num = -1;
            confirm = false;
            pl_co_y_te = new Button();
        }
        
        // 显示错误选项,变成红色,不更改
        private void Check_wrong_MouseClick(object sender, MouseEventArgs e)
        {
            if (confirm == false)
            {
                pl_co_y_te.Visible = true;
                pl_co_y_te.BringToFront();
                return;
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (textBoxList[i, j].Text != "" && textBoxList[i, j].Text != itextBoxList[i, j].Text)
                    {
                        textBoxList[i, j].ForeColor = Color.Red;
                    }
                }
            }
        }
       
        public void CreateButton (Button button, int lo_x,int lo_y,int w,int h,string Text)
        {
            button.Width = w;
            button.Height = h;
            button.Location = new System.Drawing.Point(lo_x, lo_y);
            //button.Anchor = AnchorStyles.None;
            button.Text = Text;
            button.BackColor = System.Drawing.Color.PaleVioletRed;
            button.ForeColor = System.Drawing.Color.LavenderBlush;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            button.Font = new System.Drawing.Font("MV Boli", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Controls.Add(button);
        }
        public void DeleteRelativNummber(TextBox textBox, TextBox[,] tList, List<int>[,] emptyList)
        {
            int a = textBox.TabIndex / 9;//行
            int b = textBox.TabIndex % 9;//列
            //删同行.
            //挨个找到同行不为Null元素的Index.删除
            for (int i = 0; i < 9; i++)
            {
                if (tList[a, i].Text == "")
                {
                    emptyList[a, i].Remove(Convert.ToInt32(textBox.Text));
                    if (IfFilled(tList[a, i],tList,emptyList))
                    {
                        DeleteRelativNummber(tList[a, i], tList, emptyList);
                    }
                }
            }
            //删同列
            for (int i = 0; i < 9; i++)
            {
                if (tList[i, b].Text == "")
                {
                    emptyList[i, b].Remove(Convert.ToInt32(textBox.Text));
                    if (IfFilled(tList[i, b], tList, emptyList))
                    {
                        DeleteRelativNummber(tList[i, b], tList, emptyList);
                    }
                }

            }
            //删3*3
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tList[3 * (a / 3) + i, 3 * (b / 3) + j].Text == "")
                    {
                        emptyList[3 * (a / 3) + i, 3 * (b / 3) + j].Remove(Convert.ToInt32(textBox.Text));
                        if (IfFilled(tList[3 * (a / 3) + i, 3 * (b / 3) + j], tList, emptyList))
                        {
                            DeleteRelativNummber(tList[3 * (a / 3) + i, 3 * (b / 3) + j], tList, emptyList);
                        }
                    }
                }
            }
        }
        //create new empty Textbox
        private TextBox Create1(int i, int j, Color color)
        {
            TextBox textBox = new TextBox()
            {
                MinimumSize = new System.Drawing.Size(80, 80),
                Width = 80,
                Height = 80,
                Location = new Point(this.Width / 2 - 430 + j * 80, this.Height / 2 - 360 + i * 80),//获取当前窗口的大小
                Anchor = AnchorStyles.None, //使控件随窗口大小变化
                TextAlign = HorizontalAlignment.Center,
                TabIndex = 9 * i + j,
                Name = "textBox" + TabIndex,
                MaxLength=1,
            };
            textBox.Font = new System.Drawing.Font("Bahnschrift SemiBold", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            textBox.BackColor = color;
            textBox.KeyPress += textBox_KeyPress;
            textBox.TextChanged += TextBox_TextChanged;
            Controls.Add(textBox);

            return textBox;
        }

        //create virtual iTextbox
        private TextBox Create2(int i, int j)
        {
            TextBox textBox = new TextBox(){TabIndex = 9 * i + j};
            textBox.ReadOnly = false;
            return textBox;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
       {
            TextBox textBox = sender as TextBox;
            int a = textBox.TabIndex / 9;
            int b = textBox.TabIndex % 9;
            if (!IfHandled(textBox, textBox.Text, textBoxList))
            {
                textBox.ForeColor = Color.Red;
            }
            else 
            {
                textBox.ForeColor = SystemColors.WindowText;
            }
        }
        /// <summary>
        /// 设置只能输入1-9
        /// set only input 1-9
        /// 不让重复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="KeyPressEvent"></param>
        public void textBox_KeyPress(object sender, KeyPressEventArgs KeyPressEvent)
        {
            TextBox textBox = (TextBox)sender;
            //handled:什么情况下拦截:不是数字编拦截,为0时拦截
            KeyPressEvent.Handled = (!char.IsDigit(KeyPressEvent.KeyChar) || KeyPressEvent.KeyChar == '0')&&KeyPressEvent.KeyChar!=8;
            
        }
        public bool IfHandled(TextBox textBox, string input, TextBox[,] t_list)
        {
            bool iffilled = true;
            int a = textBox.TabIndex / 9;//行
            int b = textBox.TabIndex % 9;//列

            for (int j = 0; j < 9; j++)
            {
                if (j != b && t_list[a, j].Text == input&& t_list[a, j].Text != "")
                {
                    return iffilled = false;
                }
            }
            //同列
            for (int i = 0; i < 9; i++)
            {
                if (i != a && t_list[i, b].Text == input && t_list[i, b].Text!="")
                {
                    return iffilled = false;
                }
            }
            //3*3
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i != a%3 && j != b%3 && t_list[3 * (a / 3) + i, 3 * (b / 3) + j].Text == input&& t_list[3 * (a / 3) + i, 3 * (b / 3) + j].Text!="")
                    {
                        return iffilled = false;
                    }
                }
            }
            return iffilled;
        }
        public bool IfFinished(TextBox[,] textBoxList)
        {
            foreach (var item in textBoxList)
            {
                if (item.Text == "")
                {
                    return false;
                }
            }
            return true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
