﻿
namespace FirehoseFinder
{
    partial class Write_Sectors
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox_ws = new System.Windows.Forms.GroupBox();
            this.textBox_disk_ws = new System.Windows.Forms.TextBox();
            this.label_disk_ws = new System.Windows.Forms.Label();
            this.textBox_count_ws = new System.Windows.Forms.TextBox();
            this.label_count1 = new System.Windows.Forms.Label();
            this.label_count_ws = new System.Windows.Forms.Label();
            this.textBox_start_ws = new System.Windows.Forms.TextBox();
            this.label_start_ws = new System.Windows.Forms.Label();
            this.button_ws_ok = new System.Windows.Forms.Button();
            this.button_ws_cancel = new System.Windows.Forms.Button();
            this.label_secsize_ws = new System.Windows.Forms.Label();
            this.textBox_secsize_ws = new System.Windows.Forms.TextBox();
            this.label_ss_ws = new System.Windows.Forms.Label();
            this.groupBox_ws.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_ws
            // 
            this.groupBox_ws.Controls.Add(this.label_ss_ws);
            this.groupBox_ws.Controls.Add(this.textBox_secsize_ws);
            this.groupBox_ws.Controls.Add(this.label_secsize_ws);
            this.groupBox_ws.Controls.Add(this.textBox_disk_ws);
            this.groupBox_ws.Controls.Add(this.label_disk_ws);
            this.groupBox_ws.Controls.Add(this.textBox_count_ws);
            this.groupBox_ws.Controls.Add(this.label_count1);
            this.groupBox_ws.Controls.Add(this.label_count_ws);
            this.groupBox_ws.Controls.Add(this.textBox_start_ws);
            this.groupBox_ws.Controls.Add(this.label_start_ws);
            this.groupBox_ws.Location = new System.Drawing.Point(12, 12);
            this.groupBox_ws.Name = "groupBox_ws";
            this.groupBox_ws.Size = new System.Drawing.Size(443, 87);
            this.groupBox_ws.TabIndex = 4;
            this.groupBox_ws.TabStop = false;
            this.groupBox_ws.Text = "Записать bin-файл в сектора";
            // 
            // textBox_disk_ws
            // 
            this.textBox_disk_ws.Location = new System.Drawing.Point(115, 54);
            this.textBox_disk_ws.Name = "textBox_disk_ws";
            this.textBox_disk_ws.Size = new System.Drawing.Size(63, 22);
            this.textBox_disk_ws.TabIndex = 6;
            this.textBox_disk_ws.Text = "0";
            // 
            // label_disk_ws
            // 
            this.label_disk_ws.AutoSize = true;
            this.label_disk_ws.Location = new System.Drawing.Point(6, 57);
            this.label_disk_ws.Name = "label_disk_ws";
            this.label_disk_ws.Size = new System.Drawing.Size(103, 17);
            this.label_disk_ws.TabIndex = 5;
            this.label_disk_ws.Text = "на диск номер";
            // 
            // textBox_count_ws
            // 
            this.textBox_count_ws.Location = new System.Drawing.Point(311, 21);
            this.textBox_count_ws.Name = "textBox_count_ws";
            this.textBox_count_ws.Size = new System.Drawing.Size(80, 22);
            this.textBox_count_ws.TabIndex = 4;
            this.textBox_count_ws.Text = "34";
            // 
            // label_count1
            // 
            this.label_count1.AutoSize = true;
            this.label_count1.Location = new System.Drawing.Point(397, 24);
            this.label_count1.Name = "label_count1";
            this.label_count1.Size = new System.Drawing.Size(40, 17);
            this.label_count1.TabIndex = 3;
            this.label_count1.Text = "штук";
            // 
            // label_count_ws
            // 
            this.label_count_ws.AutoSize = true;
            this.label_count_ws.Location = new System.Drawing.Point(210, 24);
            this.label_count_ws.Name = "label_count_ws";
            this.label_count_ws.Size = new System.Drawing.Size(95, 17);
            this.label_count_ws.TabIndex = 2;
            this.label_count_ws.Text = "в количестве";
            // 
            // textBox_start_ws
            // 
            this.textBox_start_ws.Location = new System.Drawing.Point(141, 21);
            this.textBox_start_ws.Name = "textBox_start_ws";
            this.textBox_start_ws.Size = new System.Drawing.Size(63, 22);
            this.textBox_start_ws.TabIndex = 1;
            this.textBox_start_ws.Text = "0";
            // 
            // label_start_ws
            // 
            this.label_start_ws.AutoSize = true;
            this.label_start_ws.Location = new System.Drawing.Point(6, 24);
            this.label_start_ws.Name = "label_start_ws";
            this.label_start_ws.Size = new System.Drawing.Size(128, 17);
            this.label_start_ws.TabIndex = 0;
            this.label_start_ws.Text = "начиная с номера";
            // 
            // button_ws_ok
            // 
            this.button_ws_ok.Location = new System.Drawing.Point(12, 105);
            this.button_ws_ok.Name = "button_ws_ok";
            this.button_ws_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ws_ok.TabIndex = 5;
            this.button_ws_ok.Text = "Ok";
            this.button_ws_ok.UseVisualStyleBackColor = true;
            this.button_ws_ok.Click += new System.EventHandler(this.Button_ws_ok_Click);
            // 
            // button_ws_cancel
            // 
            this.button_ws_cancel.Location = new System.Drawing.Point(380, 105);
            this.button_ws_cancel.Name = "button_ws_cancel";
            this.button_ws_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_ws_cancel.TabIndex = 6;
            this.button_ws_cancel.Text = "Отмена";
            this.button_ws_cancel.UseVisualStyleBackColor = true;
            this.button_ws_cancel.Click += new System.EventHandler(this.button_ws_cancel_Click);
            // 
            // label_secsize_ws
            // 
            this.label_secsize_ws.AutoSize = true;
            this.label_secsize_ws.Location = new System.Drawing.Point(184, 59);
            this.label_secsize_ws.Name = "label_secsize_ws";
            this.label_secsize_ws.Size = new System.Drawing.Size(141, 17);
            this.label_secsize_ws.TabIndex = 7;
            this.label_secsize_ws.Text = "с размером сектора";
            // 
            // textBox_secsize_ws
            // 
            this.textBox_secsize_ws.Location = new System.Drawing.Point(331, 54);
            this.textBox_secsize_ws.Name = "textBox_secsize_ws";
            this.textBox_secsize_ws.Size = new System.Drawing.Size(60, 22);
            this.textBox_secsize_ws.TabIndex = 8;
            this.textBox_secsize_ws.Text = "512";
            // 
            // label_ss_ws
            // 
            this.label_ss_ws.AutoSize = true;
            this.label_ss_ws.Location = new System.Drawing.Point(397, 59);
            this.label_ss_ws.Name = "label_ss_ws";
            this.label_ss_ws.Size = new System.Drawing.Size(39, 17);
            this.label_ss_ws.TabIndex = 9;
            this.label_ss_ws.Text = "байт";
            // 
            // Write_Sectors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 136);
            this.Controls.Add(this.button_ws_cancel);
            this.Controls.Add(this.button_ws_ok);
            this.Controls.Add(this.groupBox_ws);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Write_Sectors";
            this.Text = "Записать сектора";
            this.groupBox_ws.ResumeLayout(false);
            this.groupBox_ws.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_ws;
        internal System.Windows.Forms.TextBox textBox_count_ws;
        private System.Windows.Forms.Label label_count1;
        private System.Windows.Forms.Label label_count_ws;
        internal System.Windows.Forms.TextBox textBox_start_ws;
        private System.Windows.Forms.Label label_start_ws;
        private System.Windows.Forms.Button button_ws_ok;
        private System.Windows.Forms.Button button_ws_cancel;
        internal System.Windows.Forms.TextBox textBox_disk_ws;
        private System.Windows.Forms.Label label_disk_ws;
        private System.Windows.Forms.Label label_ss_ws;
        internal System.Windows.Forms.TextBox textBox_secsize_ws;
        private System.Windows.Forms.Label label_secsize_ws;
    }
}