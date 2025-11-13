namespace NextLayer.AdminDesktop
{
    partial class frmDashboard
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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            gbFormFuncionario = new GroupBox();
            btnLimpar = new Button();
            btnSalvar = new Button();
            chkIsAdmin = new CheckBox();
            txtSenhaFunc = new TextBox();
            label4 = new Label();
            txtCargo = new TextBox();
            label3 = new Label();
            txtEmail = new TextBox();
            label2 = new Label();
            txtNome = new TextBox();
            label1 = new Label();
            btnCarregarFuncionarios = new Button();
            dgvFuncionarios = new DataGridView();
            tabPage2 = new TabPage();
            cmbFiltroPrioridade = new ComboBox();
            label6 = new Label();
            cmbFiltroStatus = new ComboBox();
            label5 = new Label();
            btnCarregarChamados = new Button();
            dgvChamados = new DataGridView();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            gbFormFuncionario.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFuncionarios).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChamados).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(784, 561);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(gbFormFuncionario);
            tabPage1.Controls.Add(btnCarregarFuncionarios);
            tabPage1.Controls.Add(dgvFuncionarios);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(776, 533);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Gestão de Funcionários";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // gbFormFuncionario
            // 
            gbFormFuncionario.Controls.Add(btnLimpar);
            gbFormFuncionario.Controls.Add(btnSalvar);
            gbFormFuncionario.Controls.Add(chkIsAdmin);
            gbFormFuncionario.Controls.Add(txtSenhaFunc);
            gbFormFuncionario.Controls.Add(label4);
            gbFormFuncionario.Controls.Add(txtCargo);
            gbFormFuncionario.Controls.Add(label3);
            gbFormFuncionario.Controls.Add(txtEmail);
            gbFormFuncionario.Controls.Add(label2);
            gbFormFuncionario.Controls.Add(txtNome);
            gbFormFuncionario.Controls.Add(label1);
            gbFormFuncionario.Location = new Point(424, 37);
            gbFormFuncionario.Name = "gbFormFuncionario";
            gbFormFuncionario.Size = new Size(344, 488);
            gbFormFuncionario.TabIndex = 2;
            gbFormFuncionario.TabStop = false;
            gbFormFuncionario.Text = "Cadastrar / Editar Funcionário";
            // 
            // btnLimpar
            // 
            btnLimpar.Location = new Point(147, 300);
            btnLimpar.Name = "btnLimpar";
            btnLimpar.Size = new Size(98, 23);
            btnLimpar.TabIndex = 10;
            btnLimpar.Text = "Limpar (Novo)";
            btnLimpar.UseVisualStyleBackColor = true;
            btnLimpar.Click += btnLimpar_Click;
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(14, 300);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(82, 23);
            btnSalvar.TabIndex = 9;
            btnSalvar.Text = "Salvar";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;
            // 
            // chkIsAdmin
            // 
            chkIsAdmin.AutoSize = true;
            chkIsAdmin.Location = new Point(14, 250);
            chkIsAdmin.Name = "chkIsAdmin";
            chkIsAdmin.Size = new Size(71, 19);
            chkIsAdmin.TabIndex = 8;
            chkIsAdmin.Text = "É Admin";
            chkIsAdmin.UseVisualStyleBackColor = true;
            // 
            // txtSenhaFunc
            // 
            txtSenhaFunc.Location = new Point(14, 212);
            txtSenhaFunc.Name = "txtSenhaFunc";
            txtSenhaFunc.Size = new Size(231, 23);
            txtSenhaFunc.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(14, 194);
            label4.Name = "label4";
            label4.Size = new Size(39, 15);
            label4.TabIndex = 6;
            label4.Text = "Senha";
            // 
            // txtCargo
            // 
            txtCargo.Location = new Point(14, 161);
            txtCargo.Name = "txtCargo";
            txtCargo.Size = new Size(231, 23);
            txtCargo.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 143);
            label3.Name = "label3";
            label3.Size = new Size(42, 15);
            label3.TabIndex = 4;
            label3.Text = "Cargo:";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(14, 106);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(231, 23);
            txtEmail.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 88);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 2;
            label2.Text = "E-mail:";
            // 
            // txtNome
            // 
            txtNome.Location = new Point(14, 52);
            txtNome.Name = "txtNome";
            txtNome.Size = new Size(231, 23);
            txtNome.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 34);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 0;
            label1.Text = "Nome:";
            // 
            // btnCarregarFuncionarios
            // 
            btnCarregarFuncionarios.Location = new Point(3, 8);
            btnCarregarFuncionarios.Name = "btnCarregarFuncionarios";
            btnCarregarFuncionarios.Size = new Size(156, 23);
            btnCarregarFuncionarios.TabIndex = 1;
            btnCarregarFuncionarios.Text = "Carregar/Atualizar Lista";
            btnCarregarFuncionarios.UseVisualStyleBackColor = true;
            btnCarregarFuncionarios.Click += btnCarregarFuncionarios_Click;
            // 
            // dgvFuncionarios
            // 
            dgvFuncionarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvFuncionarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFuncionarios.Location = new Point(3, 37);
            dgvFuncionarios.Name = "dgvFuncionarios";
            dgvFuncionarios.Size = new Size(415, 500);
            dgvFuncionarios.TabIndex = 0;
            dgvFuncionarios.CellClick += dgvFuncionarios_CellClick;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(cmbFiltroPrioridade);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(cmbFiltroStatus);
            tabPage2.Controls.Add(label5);
            tabPage2.Controls.Add(btnCarregarChamados);
            tabPage2.Controls.Add(dgvChamados);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(776, 533);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Chamados";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // cmbFiltroPrioridade
            // 
            cmbFiltroPrioridade.FormattingEnabled = true;
            cmbFiltroPrioridade.Location = new Point(127, 54);
            cmbFiltroPrioridade.Name = "cmbFiltroPrioridade";
            cmbFiltroPrioridade.Size = new Size(121, 23);
            cmbFiltroPrioridade.TabIndex = 5;
            //cmbFiltroPrioridade.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 57);
            label6.Name = "label6";
            label6.Size = new Size(118, 15);
            label6.TabIndex = 4;
            label6.Text = "Filtrar por Prioridade:";
            // 
            // cmbFiltroStatus
            // 
            cmbFiltroStatus.FormattingEnabled = true;
            cmbFiltroStatus.Location = new Point(127, 10);
            cmbFiltroStatus.Name = "cmbFiltroStatus";
            cmbFiltroStatus.Size = new Size(121, 23);
            cmbFiltroStatus.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(3, 13);
            label5.Name = "label5";
            label5.Size = new Size(96, 15);
            label5.TabIndex = 2;
            label5.Text = "Filtrar por Status:";
            // 
            // btnCarregarChamados
            // 
            btnCarregarChamados.Location = new Point(0, 112);
            btnCarregarChamados.Name = "btnCarregarChamados";
            btnCarregarChamados.Size = new Size(190, 29);
            btnCarregarChamados.TabIndex = 1;
            btnCarregarChamados.Text = "Carregar Todos os Chamados";
            btnCarregarChamados.UseVisualStyleBackColor = true;
            btnCarregarChamados.Click += btnCarregarChamados_Click;
            // 
            // dgvChamados
            // 
            dgvChamados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvChamados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvChamados.Location = new Point(-4, 147);
            dgvChamados.Name = "dgvChamados";
            dgvChamados.Size = new Size(780, 386);
            dgvChamados.TabIndex = 0;
            // 
            // frmDashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(tabControl1);
            Name = "frmDashboard";
            Text = "frmDashboard";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            gbFormFuncionario.ResumeLayout(false);
            gbFormFuncionario.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFuncionarios).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChamados).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DataGridView dgvFuncionarios;
        private GroupBox gbFormFuncionario;
        private Button btnCarregarFuncionarios;
        private TextBox txtSenhaFunc;
        private Label label4;
        private TextBox txtCargo;
        private Label label3;
        private TextBox txtEmail;
        private Label label2;
        private TextBox txtNome;
        private Label label1;
        private Button btnLimpar;
        private Button btnSalvar;
        private CheckBox chkIsAdmin;
        private DataGridView dgvChamados;
        private Button btnCarregarChamados;
        private ComboBox cmbFiltroPrioridade;
        private Label label6;
        private ComboBox cmbFiltroStatus;
        private Label label5;
    }
}