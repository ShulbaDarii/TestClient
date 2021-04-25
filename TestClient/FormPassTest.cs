using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class FormPassTest : Form
    {
        List<Button> buttons;
        List<int> Xs;
        TestDLL.Test test;
        TestDLL.Result testResult;
        List<RadioButton> answer;
        Form form;
        List<int> ans;
        int qust;
        TimeSpan time;
        public FormPassTest(TestDLL.Test test, Form form, TestDLL.Result testResult)
        {
            InitializeComponent();
            label1.Text = test.Time.ToString();
            answer = new List<RadioButton>();
            ans = new List<int>();
            buttons = new List<Button>();
            Xs = new List<int>();
            time = test.Time;
            this.test = test;
            this.testResult = testResult;
            this.form = form;
            foreach(var q in test.Questions)
            {
                ans.Add(-1);
            }
            int x = 5;
            for (int i = 1; i <= test.Questions.Count; i++)
            {
                Xs.Add(x);
                Button button = new Button() { Text = i.ToString(), Location = new Point(x, 15), Size = new Size(30,25) };
                buttons.Add(button);
                button.Click += Button_Click;
                groupBox3.Controls.Add(button);
                x += 29;
            }
            if (test.Questions.Count > 2)
            {
                hScrollBar1.Maximum = 29 * (test.Questions.Count - 2);
            }else
                hScrollBar1.Maximum = 0;
            timer1.Start();
            form.Visible = false;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            groupBox2.Text = $"Qustion {(sender as Button).Text}";
            int i = 1;
            answer.Clear();
            qust = Convert.ToInt32((sender as Button).Text) - 1;
            foreach (var q in test.Questions)
            {
                if (Convert.ToInt32((sender as Button).Text) == i)
                {
                    textBox1.Text = q.Title;
                    int Y = 15;
                    groupBox1.Controls.Clear();
                    int j = 0;
                    foreach (var ans in q.Answers)
                    {
                        Y += 30;
                        RadioButton radioButton = new RadioButton() { Text = ans.Description,Location=new Point(15,Y) };
                        radioButton.CheckedChanged += RadioButton_CheckedChanged;
                        groupBox1.Controls.Add(radioButton);
                        answer.Add(radioButton);

                        if (this.ans[qust] == j++)
                        {
                            radioButton.Checked = true;
                        }
                    }
                    break;
                }
                i++;
            }
            
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            for (int j = 0; j < answer.Count; j++)
                if (answer[j].Checked)
                {
                    ans[qust] = j;
                }
        }


        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int i = 0;
            foreach(var button in buttons)
            {
                //button.Location.X=button.Location.X+1;
                button.Location =new Point( Xs[i++]-hScrollBar1.Value,15);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time -= new TimeSpan(0, 0, 1);
            if(time==new TimeSpan(0,0,0))
            {
                MessageBox.Show("time out");
                timer1.Stop();
                Close();
            }
            label1.Text = time.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                qust += 1;
                Button_Click(buttons[qust], EventArgs.Empty);
            }catch
            { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormPassTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.Visible = true;
            testResult.Date = DateTime.Now;
            testResult.NameTest = test.Title;
            int i = 0;
            int mark = 0;
            foreach(var a in test.Questions)
            {
                int j = 0;
                foreach(var right in a.Answers)
                {
                    
                    if(right.IsRight)
                    {
                        if (j == ans[i])
                        {
                            mark++;
                        }
                    }
                    j++;
                }
                i++;
            }
            int x = (int)((float)mark / (float)test.Questions.Count * 12);
            if (x < 1)
                x = 1;
            testResult.Mark = x;
            testResult.QtyOfRightAnswers = mark;
        MessageBox.Show($"{mark}/{test.Questions.Count} right answer\nmark {x}");
        }
    }
}
