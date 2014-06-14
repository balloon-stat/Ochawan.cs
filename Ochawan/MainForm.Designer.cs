namespace Ochawan
{
    partial class MainForm
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
            m_stopped = true;
            hotkey.Dispose();
            ctokenSrc.Cancel();
            if (bClient != null)
                bClient.Dispose();
            ringin.WriteThemeDict();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.user = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.検索ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開くToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.コテハンToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.プロフィールToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ユーザ名ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button5 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button8 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開く放送設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開くコミュニティ設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開く起動設定ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.コメントToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.コピーToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.スレッド情報ToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.新着放送ToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.登録枠待ちToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.リセットToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.クッキー再取得ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.放送ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.外部ツールToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.全て起動ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.棒読みちゃんToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.nLEToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.oBSToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ささらToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.起動ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.プリセットの登録ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.囲碁ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nicoTacticsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.codeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.ユーザ名の表示ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.スレッド停止ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.no,
            this.comment,
            this.user});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Location = new System.Drawing.Point(3, 64);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(390, 267);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentDoubleClick);
            // 
            // no
            // 
            this.no.FillWeight = 30.89369F;
            this.no.HeaderText = "No.";
            this.no.Name = "no";
            this.no.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.no.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // comment
            // 
            this.comment.FillWeight = 170.1216F;
            this.comment.HeaderText = "コメント";
            this.comment.Name = "comment";
            this.comment.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.comment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // user
            // 
            this.user.FillWeight = 98.98478F;
            this.user.HeaderText = "ユーザー";
            this.user.Name = "user";
            this.user.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.user.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.検索ToolStripMenuItem,
            this.開くToolStripMenuItem,
            this.コテハンToolStripMenuItem,
            this.プロフィールToolStripMenuItem,
            this.ユーザ名ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(128, 114);
            // 
            // 検索ToolStripMenuItem
            // 
            this.検索ToolStripMenuItem.Name = "検索ToolStripMenuItem";
            this.検索ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.検索ToolStripMenuItem.Text = "検索";
            this.検索ToolStripMenuItem.Click += new System.EventHandler(this.検索ToolStripMenuItem_Click);
            // 
            // 開くToolStripMenuItem
            // 
            this.開くToolStripMenuItem.Name = "開くToolStripMenuItem";
            this.開くToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.開くToolStripMenuItem.Text = "開く";
            this.開くToolStripMenuItem.Click += new System.EventHandler(this.開くToolStripMenuItem_Click);
            // 
            // コテハンToolStripMenuItem
            // 
            this.コテハンToolStripMenuItem.Name = "コテハンToolStripMenuItem";
            this.コテハンToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.コテハンToolStripMenuItem.Text = "コテハン";
            this.コテハンToolStripMenuItem.Click += new System.EventHandler(this.コテハンToolStripMenuItem_Click);
            // 
            // プロフィールToolStripMenuItem
            // 
            this.プロフィールToolStripMenuItem.Name = "プロフィールToolStripMenuItem";
            this.プロフィールToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.プロフィールToolStripMenuItem.Text = "プロフィール";
            this.プロフィールToolStripMenuItem.Click += new System.EventHandler(this.プロフィールToolStripMenuItem_Click);
            // 
            // ユーザ名ToolStripMenuItem
            // 
            this.ユーザ名ToolStripMenuItem.Name = "ユーザ名ToolStripMenuItem";
            this.ユーザ名ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.ユーザ名ToolStripMenuItem.Text = "ユーザ名";
            this.ユーザ名ToolStripMenuItem.Click += new System.EventHandler(this.ユーザ名の表示ToolStripMenuItem_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseButton.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BrowseButton.Location = new System.Drawing.Point(432, 64);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(88, 27);
            this.BrowseButton.TabIndex = 4;
            this.BrowseButton.Text = "BROWSE";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(431, 210);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(69, 16);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "読み上げ";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(415, 294);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(107, 37);
            this.button5.TabIndex = 7;
            this.button5.Text = "コメント取得";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.getComment_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(414, 34);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 10;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button8.Location = new System.Drawing.Point(432, 97);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(87, 29);
            this.button8.TabIndex = 13;
            this.button8.Text = "Commnity";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(11, 34);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(381, 19);
            this.textBox2.TabIndex = 16;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyDown);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルToolStripMenuItem,
            this.コメントToolStripMenuItem,
            this.放送ToolStripMenuItem,
            this.外部ツールToolStripMenuItem,
            this.囲碁ToolStripMenuItem,
            this.nicoTacticsToolStripMenuItem1,
            this.codeViewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(547, 24);
            this.menuStrip1.TabIndex = 17;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルToolStripMenuItem
            // 
            this.ファイルToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.開く放送設定ToolStripMenuItem,
            this.開くコミュニティ設定ToolStripMenuItem,
            this.開く起動設定ToolStripMenuItem});
            this.ファイルToolStripMenuItem.Name = "ファイルToolStripMenuItem";
            this.ファイルToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.ファイルToolStripMenuItem.Text = "ファイル";
            // 
            // 開く放送設定ToolStripMenuItem
            // 
            this.開く放送設定ToolStripMenuItem.Name = "開く放送設定ToolStripMenuItem";
            this.開く放送設定ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.開く放送設定ToolStripMenuItem.Text = "開く　放送設定";
            this.開く放送設定ToolStripMenuItem.Click += new System.EventHandler(this.開く放送設定ToolStripMenuItem_Click);
            // 
            // 開くコミュニティ設定ToolStripMenuItem
            // 
            this.開くコミュニティ設定ToolStripMenuItem.Name = "開くコミュニティ設定ToolStripMenuItem";
            this.開くコミュニティ設定ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.開くコミュニティ設定ToolStripMenuItem.Text = "開く　コミュニティ";
            this.開くコミュニティ設定ToolStripMenuItem.Click += new System.EventHandler(this.開くコミュニティ設定ToolStripMenuItem_Click);
            // 
            // 開く起動設定ToolStripMenuItem
            // 
            this.開く起動設定ToolStripMenuItem.Name = "開く起動設定ToolStripMenuItem";
            this.開く起動設定ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.開く起動設定ToolStripMenuItem.Text = "開く　起動設定";
            this.開く起動設定ToolStripMenuItem.Click += new System.EventHandler(this.開く起動設定ToolStripMenuItem_Click);
            // 
            // コメントToolStripMenuItem
            // 
            this.コメントToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StopToolStrip,
            this.コピーToolStrip,
            this.スレッド情報ToolStrip,
            this.新着放送ToolStrip,
            this.登録枠待ちToolStrip,
            this.リセットToolStripMenuItem,
            this.クッキー再取得ToolStripMenuItem1});
            this.コメントToolStripMenuItem.Name = "コメントToolStripMenuItem";
            this.コメントToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.コメントToolStripMenuItem.Text = "コメント";
            // 
            // StopToolStrip
            // 
            this.StopToolStrip.Name = "StopToolStrip";
            this.StopToolStrip.Size = new System.Drawing.Size(145, 22);
            this.StopToolStrip.Text = "取得停止";
            this.StopToolStrip.Click += new System.EventHandler(this.取得停止ToolStrip_Click);
            // 
            // コピーToolStrip
            // 
            this.コピーToolStrip.Name = "コピーToolStrip";
            this.コピーToolStrip.Size = new System.Drawing.Size(145, 22);
            this.コピーToolStrip.Text = "コピー";
            // 
            // スレッド情報ToolStrip
            // 
            this.スレッド情報ToolStrip.Name = "スレッド情報ToolStrip";
            this.スレッド情報ToolStrip.Size = new System.Drawing.Size(145, 22);
            this.スレッド情報ToolStrip.Text = "スレッド情報";
            this.スレッド情報ToolStrip.Click += new System.EventHandler(this.スレッド情報ToolStrip_Click);
            // 
            // 新着放送ToolStrip
            // 
            this.新着放送ToolStrip.Name = "新着放送ToolStrip";
            this.新着放送ToolStrip.Size = new System.Drawing.Size(145, 22);
            this.新着放送ToolStrip.Text = "新着放送";
            this.新着放送ToolStrip.Click += new System.EventHandler(this.新着放送ToolStrip_Click);
            // 
            // 登録枠待ちToolStrip
            // 
            this.登録枠待ちToolStrip.Name = "登録枠待ちToolStrip";
            this.登録枠待ちToolStrip.Size = new System.Drawing.Size(145, 22);
            this.登録枠待ちToolStrip.Text = "登録枠待ち";
            this.登録枠待ちToolStrip.Click += new System.EventHandler(this.登録枠待ちToolStrip_Click);
            // 
            // リセットToolStripMenuItem
            // 
            this.リセットToolStripMenuItem.Name = "リセットToolStripMenuItem";
            this.リセットToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.リセットToolStripMenuItem.Text = "リセット";
            this.リセットToolStripMenuItem.Click += new System.EventHandler(this.リセットToolStripMenuItem_Click);
            // 
            // クッキー再取得ToolStripMenuItem1
            // 
            this.クッキー再取得ToolStripMenuItem1.Name = "クッキー再取得ToolStripMenuItem1";
            this.クッキー再取得ToolStripMenuItem1.Size = new System.Drawing.Size(145, 22);
            this.クッキー再取得ToolStripMenuItem1.Text = "クッキー再取得";
            this.クッキー再取得ToolStripMenuItem1.Click += new System.EventHandler(this.クッキー再取得ToolStripMenuItem_Click);
            // 
            // 放送ToolStripMenuItem
            // 
            this.放送ToolStripMenuItem.Name = "放送ToolStripMenuItem";
            this.放送ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.放送ToolStripMenuItem.Text = "放送";
            this.放送ToolStripMenuItem.Click += new System.EventHandler(this.放送ToolStrip_Click);
            // 
            // 外部ツールToolStripMenuItem
            // 
            this.外部ツールToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.全て起動ToolStripMenuItem,
            this.棒読みちゃんToolStripMenuItem1,
            this.nLEToolStripMenuItem1,
            this.oBSToolStripMenuItem1,
            this.ささらToolStripMenuItem});
            this.外部ツールToolStripMenuItem.Name = "外部ツールToolStripMenuItem";
            this.外部ツールToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.外部ツールToolStripMenuItem.Text = "外部ツール";
            // 
            // 全て起動ToolStripMenuItem
            // 
            this.全て起動ToolStripMenuItem.Name = "全て起動ToolStripMenuItem";
            this.全て起動ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.全て起動ToolStripMenuItem.Text = "全て起動";
            this.全て起動ToolStripMenuItem.Click += new System.EventHandler(this.全て起動ToolStripMenuItem_Click);
            // 
            // 棒読みちゃんToolStripMenuItem1
            // 
            this.棒読みちゃんToolStripMenuItem1.Name = "棒読みちゃんToolStripMenuItem1";
            this.棒読みちゃんToolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.棒読みちゃんToolStripMenuItem1.Text = "棒読みちゃん";
            this.棒読みちゃんToolStripMenuItem1.Click += new System.EventHandler(this.棒読みちゃんToolStrip_Click);
            // 
            // nLEToolStripMenuItem1
            // 
            this.nLEToolStripMenuItem1.Name = "nLEToolStripMenuItem1";
            this.nLEToolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.nLEToolStripMenuItem1.Text = "NLE";
            this.nLEToolStripMenuItem1.Click += new System.EventHandler(this.nLEToolStripMenuItem_Click);
            // 
            // oBSToolStripMenuItem1
            // 
            this.oBSToolStripMenuItem1.Name = "oBSToolStripMenuItem1";
            this.oBSToolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.oBSToolStripMenuItem1.Text = "OBS";
            this.oBSToolStripMenuItem1.Click += new System.EventHandler(this.oBSToolStripMenuItem_Click);
            // 
            // ささらToolStripMenuItem
            // 
            this.ささらToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.起動ToolStripMenuItem,
            this.プリセットの登録ToolStripMenuItem});
            this.ささらToolStripMenuItem.Name = "ささらToolStripMenuItem";
            this.ささらToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.ささらToolStripMenuItem.Text = "ささら";
            // 
            // 起動ToolStripMenuItem
            // 
            this.起動ToolStripMenuItem.Name = "起動ToolStripMenuItem";
            this.起動ToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.起動ToolStripMenuItem.Text = "起動";
            this.起動ToolStripMenuItem.Click += new System.EventHandler(this.sasaraToolStripMenuItem_Click);
            // 
            // プリセットの登録ToolStripMenuItem
            // 
            this.プリセットの登録ToolStripMenuItem.Name = "プリセットの登録ToolStripMenuItem";
            this.プリセットの登録ToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.プリセットの登録ToolStripMenuItem.Text = "プリセットの登録";
            this.プリセットの登録ToolStripMenuItem.Click += new System.EventHandler(this.プリセットの登録ToolStripMenuItem_Click);
            // 
            // 囲碁ToolStripMenuItem
            // 
            this.囲碁ToolStripMenuItem.Name = "囲碁ToolStripMenuItem";
            this.囲碁ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.囲碁ToolStripMenuItem.Text = "囲碁";
            this.囲碁ToolStripMenuItem.Click += new System.EventHandler(this.囲碁ToolStripMenuItem1_Click);
            // 
            // nicoTacticsToolStripMenuItem1
            // 
            this.nicoTacticsToolStripMenuItem1.Name = "nicoTacticsToolStripMenuItem1";
            this.nicoTacticsToolStripMenuItem1.Size = new System.Drawing.Size(84, 20);
            this.nicoTacticsToolStripMenuItem1.Text = "NicoTactics";
            this.nicoTacticsToolStripMenuItem1.Click += new System.EventHandler(this.nicoTacticsToolStripMenuItem_Click);
            // 
            // codeViewToolStripMenuItem
            // 
            this.codeViewToolStripMenuItem.Name = "codeViewToolStripMenuItem";
            this.codeViewToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.codeViewToolStripMenuItem.Text = "CodeView";
            this.codeViewToolStripMenuItem.Click += new System.EventHandler(this.codeViewToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripSplitButton1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 336);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(547, 22);
            this.statusStrip1.TabIndex = 18;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabel1.Text = "Status";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ユーザ名の表示ToolStripMenuItem,
            this.スレッド停止ToolStripMenuItem});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(80, 20);
            this.toolStripSplitButton1.Text = "スレッド情報";
            this.toolStripSplitButton1.ButtonClick += new System.EventHandler(this.toolStripSplitButton1_ButtonClick);
            // 
            // ユーザ名の表示ToolStripMenuItem
            // 
            this.ユーザ名の表示ToolStripMenuItem.Name = "ユーザ名の表示ToolStripMenuItem";
            this.ユーザ名の表示ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.ユーザ名の表示ToolStripMenuItem.Text = "ユーザ名の表示";
            this.ユーザ名の表示ToolStripMenuItem.Click += new System.EventHandler(this.ユーザ名の表示ToolStripMenuItem_Click);
            // 
            // スレッド停止ToolStripMenuItem
            // 
            this.スレッド停止ToolStripMenuItem.Name = "スレッド停止ToolStripMenuItem";
            this.スレッド停止ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.スレッド停止ToolStripMenuItem.Text = "スレッド停止";
            this.スレッド停止ToolStripMenuItem.Click += new System.EventHandler(this.スレッド停止ToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkBox4
            // 
            this.checkBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(431, 235);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(80, 16);
            this.checkBox4.TabIndex = 19;
            this.checkBox4.Text = "自動枠取り";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox5.AutoSize = true;
            this.checkBox5.Checked = true;
            this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox5.Location = new System.Drawing.Point(431, 185);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(97, 16);
            this.checkBox5.TabIndex = 20;
            this.checkBox5.Text = "ささら 読み上げ";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox6.AutoSize = true;
            this.checkBox6.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.checkBox6.Location = new System.Drawing.Point(432, 159);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(50, 19);
            this.checkBox6.TabIndex = 21;
            this.checkBox6.Text = "184";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(491, 132);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(44, 19);
            this.textBox1.TabIndex = 22;
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(431, 262);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(71, 16);
            this.checkBox2.TabIndex = 23;
            this.checkBox2.Text = "OBS放送";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 358);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Ochawan";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Click += new System.EventHandler(this.MainForm_Click);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem コメントToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopToolStrip;
        private System.Windows.Forms.ToolStripMenuItem コピーToolStrip;
        private System.Windows.Forms.ToolStripMenuItem スレッド情報ToolStrip;
        private System.Windows.Forms.ToolStripMenuItem 放送ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新着放送ToolStrip;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.DataGridViewTextBoxColumn no;
        private System.Windows.Forms.DataGridViewTextBoxColumn comment;
        private System.Windows.Forms.DataGridViewTextBoxColumn user;
        private System.Windows.Forms.ToolStripMenuItem 登録枠待ちToolStrip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 検索ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開くToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem コテハンToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem スレッド停止ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ユーザ名の表示ToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.ToolStripMenuItem 開く放送設定ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開くコミュニティ設定ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem プロフィールToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開く起動設定ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ユーザ名ToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem リセットToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.ToolStripMenuItem 外部ツールToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 全て起動ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 棒読みちゃんToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem nLEToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem oBSToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem クッキー再取得ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ささらToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 囲碁ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nicoTacticsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem codeViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 起動ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem プリセットの登録ToolStripMenuItem;

    }
}

