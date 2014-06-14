namespace Ochawan
{
    partial class CommunityForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.名前 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.コミュニティID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ユーザID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.名前,
            this.コミュニティID,
            this.ユーザID});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(342, 309);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // 名前
            // 
            this.名前.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.名前.FillWeight = 70F;
            this.名前.HeaderText = "名前";
            this.名前.Name = "名前";
            // 
            // コミュニティID
            // 
            this.コミュニティID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.コミュニティID.FillWeight = 60F;
            this.コミュニティID.HeaderText = "コミュニティID";
            this.コミュニティID.Name = "コミュニティID";
            // 
            // ユーザID
            // 
            this.ユーザID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ユーザID.FillWeight = 60F;
            this.ユーザID.HeaderText = "ユーザID";
            this.ユーザID.Name = "ユーザID";
            // 
            // CommnityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 309);
            this.Controls.Add(this.dataGridView1);
            this.Name = "CommnityForm";
            this.Text = "CommnityForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CommunityForm_FormClosing);
            this.Load += new System.EventHandler(this.CommunityForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 名前;
        private System.Windows.Forms.DataGridViewTextBoxColumn コミュニティID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ユーザID;
    }
}