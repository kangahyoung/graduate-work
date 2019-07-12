namespace WindowsFormsApp1 {
  partial class Form1 {
    /// <summary>
    /// 필수 디자이너 변수입니다.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 사용 중인 모든 리소스를 정리합니다.
    /// </summary>
    /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form 디자이너에서 생성한 코드

    /// <summary>
    /// 디자이너 지원에 필요한 메서드입니다. 
    /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.button1 = new System.Windows.Forms.Button();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.button2 = new System.Windows.Forms.Button();
      this.listBox2 = new System.Windows.Forms.ListBox();
      this.listBox3 = new System.Windows.Forms.ListBox();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.timer2 = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(12, 12);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(391, 22);
      this.button1.TabIndex = 0;
      this.button1.Text = "Load List";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.ItemHeight = 12;
      this.listBox1.Location = new System.Drawing.Point(437, 12);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(145, 124);
      this.listBox1.TabIndex = 1;
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(12, 66);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(391, 22);
      this.button2.TabIndex = 2;
      this.button2.Text = "Connect";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // listBox2
      // 
      this.listBox2.FormattingEnabled = true;
      this.listBox2.ItemHeight = 12;
      this.listBox2.Location = new System.Drawing.Point(607, 12);
      this.listBox2.Name = "listBox2";
      this.listBox2.Size = new System.Drawing.Size(145, 124);
      this.listBox2.TabIndex = 3;
      // 
      // listBox3
      // 
      this.listBox3.FormattingEnabled = true;
      this.listBox3.ItemHeight = 12;
      this.listBox3.Location = new System.Drawing.Point(12, 150);
      this.listBox3.Name = "listBox3";
      this.listBox3.Size = new System.Drawing.Size(391, 100);
      this.listBox3.TabIndex = 4;
      // 
      // button3
      // 
      this.button3.Enabled = false;
      this.button3.Location = new System.Drawing.Point(12, 94);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(391, 22);
      this.button3.TabIndex = 5;
      this.button3.Text = "Auth";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Enabled = false;
      this.button4.Location = new System.Drawing.Point(12, 122);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(391, 22);
      this.button4.TabIndex = 6;
      this.button4.Text = "StartMonitorHeartrate";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Enabled = false;
      this.button5.Location = new System.Drawing.Point(12, 256);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(391, 22);
      this.button5.TabIndex = 7;
      this.button5.Text = "StopMonitorHeartrate";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new System.Drawing.Point(12, 40);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(391, 20);
      this.comboBox1.TabIndex = 8;
      // 
      // timer1
      // 
      this.timer1.Interval = 2000;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // timer2
      // 
      this.timer2.Interval = 1000;
      this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(407, 283);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.listBox3);
      this.Controls.Add(this.listBox2);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.button1);
      this.Name = "Form1";
      this.Text = "MibandServer";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new System.EventHandler(this.Form1_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.ListBox listBox2;
    private System.Windows.Forms.ListBox listBox3;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.Timer timer2;
  }
}

