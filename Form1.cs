using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

public class Form1 : Form
{
	public static byte[] ChkLstFile_MagicNumber = new byte[] { 0XFF, 0x42, 0x61, 0x3B, 0x42, 0xFF, 13, 10 };
	
	private static string CheckMark = "√";
	private static string WindowText = "Check List Manager";
	
	private MenuItem FileItem;
	private MenuItem NewItem;
	private MenuItem OpenItem;
	private MenuItem SaveItem;
	private MenuItem SaveAsItem;
	private MenuItem ClearRecentFileListItem;
	private MenuItem ExitItem;
	
	private MenuItem ToolsItem;
	private MenuItem LoadClipboardItem;
	private MenuItem OptionsItem;
	private MenuItem AutoSaveItem;
	//private MenuItem AddCheckListHeaderItem;
	
	private ScrollablePanel PanelBox;
	
	private string SavePath = String.Empty;
	
	private bool _IsDirty = false;
	
	public bool IsDirty
	{
		get
		{
			return _IsDirty;
		}
		set
		{
			_IsDirty = value;
			SaveItem.Enabled = _IsDirty;
			
			if (_IsDirty == true)
			{
				if (this.Text[0].ToString() != "*") this.Text = this.Text.Insert(0, "*");
			}
			else
			{
				this.Text = this.Text.Substring(1, this.Text.Length - 1);
			}
		}
	}
	
	private Settings settings = new Settings();
	
	public Form1()
	{
		InitializeComponent();
		
		if (settings.LastFileOpened != String.Empty && File.Exists(settings.LastFileOpened) == true)
		{
			this.LoadPossibleFile(settings.LastFileOpened, false);
		}
	}
	
	public Form1(string FileName)
	{
		InitializeComponent();
		
		if (FileName != String.Empty && File.Exists(FileName) == true)
		{
			this.LoadPossibleFile(FileName, true);
		}
	}
	
	public Form1(string[] List)
	{
		InitializeComponent();
		
		if (List[0].ToLower() == "list")
		{
			string[] Data = new string[List.Length - 1];
			for (int i = 1; i < List.Length; i++)
			{
				Data[i - 1] = List[i];
			}
			
			this.LoadList(Data);
			this.Text = "Untitled Checklist - " + Form1.WindowText;
			this.IsDirty = true;
		}
	}
	
	private void InitializeComponent()
	{
		this.Text = "Untitled Checklist - Check List Manager";
		//this.Size = new Size(640, 480);
		this.Size = new Size(350, 300);
		this.MinimumSize = new Size(this.Size.Width, 300);
		this.AllowDrop = true;
		this.DragDrop += Form1_DragDrop;
		this.DragOver += Form1_DragOver;
		this.KeyDown += Form1_KeyDown;
		this.KeyUp += Form1_KeyUp;
		this.StartPosition = FormStartPosition.CenterScreen;
		this.FormClosing += Form1_FormClosing;
		
		PanelBox = new ScrollablePanel(this);
		PanelBox.Location = new Point(6, 6);
		PanelBox.BorderStyle = BorderStyle.FixedSingle;
		PanelBox.Size = new Size(this.ClientSize.Width - (PanelBox.Location.X * 2), this.ClientSize.Height - (PanelBox.Location.Y * 2));
		PanelBox.MinimumSize = PanelBox.ClientSize;
		PanelBox.AllowDrop = true;
		PanelBox.DragDrop += Form1_DragDrop;
		PanelBox.DragOver += Form1_DragOver;
		PanelBox.KeyDown += Form1_KeyDown;
		PanelBox.KeyUp += Form1_KeyUp;
		
		this.Controls.Add(PanelBox);
		
		
		MainMenu menu = new MainMenu();
		FileItem = new MenuItem();
		FileItem.Text = "File";
		
		NewItem = new MenuItem();
		NewItem.Text = "New";
		NewItem.Click += NewItem_Click;
		NewItem.Shortcut = Shortcut.CtrlN;
		
		OpenItem = new MenuItem();
		OpenItem.Text = "Open...";
		OpenItem.Click += OpenItem_Click;
		OpenItem.Shortcut = Shortcut.CtrlO;
		
		SaveItem = new MenuItem();
		SaveItem.Text = "Save";
		SaveItem.Click += SaveItem_Click;
		SaveItem.Shortcut = Shortcut.CtrlS;
		SaveItem.Enabled = false;
		
		SaveAsItem = new MenuItem();
		SaveAsItem.Text = "Save As...";
		SaveAsItem.Click += SaveAsItem_Click;
		SaveAsItem.Shortcut = Shortcut.CtrlShiftS;

		ClearRecentFileListItem = new MenuItem();
		ClearRecentFileListItem.Text = "Empty Recent Files";
		ClearRecentFileListItem.Click += (sender, e) =>
		{
			settings.ClearRecentFiles();
			this.LoadRecentFiles();
		};
		
		ExitItem = new MenuItem();
		ExitItem.Text = "Exit";
		ExitItem.Click += ExitItem_Click;
		
		// ToolStripSeparator i = new ToolStripSeparator();
		// //i.BarBreak = true;
		// i.Break = true;
		
		
		ToolsItem = new MenuItem();
		ToolsItem.Text = "Tools";
		
		LoadClipboardItem = new MenuItem();
		LoadClipboardItem.Text = "Load from Clipboard";
		LoadClipboardItem.Click += LoadClipboardItem_Click;
		
		OptionsItem = new MenuItem();
		OptionsItem.Text = "Options";
		
		AutoSaveItem = new MenuItem();
		AutoSaveItem.Text = "Enable Auto Save";
		AutoSaveItem.Checked = settings.EnableAutoSave;
		AutoSaveItem.Click += AutoSaveItem_Click;

		//AddCheckListHeaderItem = new MenuItem();
		//AddCheckListHeaderItem.Text = "Add Check List Header To File";
		//AddCheckListHeaderItem.Click += AddCheckListHeaderItem_Click;
		
		OptionsItem.MenuItems.Add(AutoSaveItem);
		ToolsItem.MenuItems.Add(OptionsItem);
		ToolsItem.MenuItems.Add("-");
		ToolsItem.MenuItems.Add(LoadClipboardItem);
		//ToolsItem.MenuItems.Add(AddCheckListHeaderItem);
		
		this.LoadRecentFiles();
		
		menu.MenuItems.Add(FileItem);
		menu.MenuItems.Add(ToolsItem);
		this.Menu = menu;
	}

	private void AddCheckListHeaderItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog diag = new OpenFileDialog();
		diag.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
		diag.Filter = "All supported files (*.txt; *.ChkLst)|*.txt;*.ChkLst|Check List Manager Save files (*.ChkLst)|*.ChkLst|Text files (*.txt)|*.txt";
		diag.CheckFileExists = true;
		diag.CheckPathExists = true;
		diag.Multiselect = false;
		
		if (diag.ShowDialog() == DialogResult.OK)
		{
			if (Path.GetExtension(diag.FileName) == ".ChkLst" && this.IsChkLstFile(diag.FileName) == false)
			{
				byte[] Data = File.ReadAllBytes(diag.FileName);

				byte[] NewData = new byte[Data.Length + Form1.ChkLstFile_MagicNumber.Length];

				Array.Copy(Form1.ChkLstFile_MagicNumber, 0, NewData, 0, Form1.ChkLstFile_MagicNumber.Length);

				Array.Copy(Data, 0, NewData, Form1.ChkLstFile_MagicNumber.Length, Data.Length);

				File.WriteAllBytes(diag.FileName, NewData);

				Data = new byte[0];
				NewData = new byte[0];

				Data = null;
				NewData = null;
			}
		}
	}

	private void LoadRecentFiles()
	{
		FileItem.MenuItems.Clear();

		FileItem.MenuItems.Add(NewItem);
		FileItem.MenuItems.Add(OpenItem);
		FileItem.MenuItems.Add(SaveItem);
		FileItem.MenuItems.Add(SaveAsItem);
		FileItem.MenuItems.Add("-"); 		// Add a seperator (The - makes it a seperator)
		
		if (settings.RecentFileList.Count > 0)
		{
			for (int i = 0; i < settings.RecentFileList.Count; i++)
			{
				MenuItem item = new MenuItem();
				item.Text = (i + 1).ToString() + ": " + settings.RecentFileList[i];
				item.Click += (sender, e) =>
				{
					MenuItem menuItem = (MenuItem)sender;

					string[] parts = menuItem.Text.Split(':');

					int start = (parts[0].Length + ": ".Length) - 1;

					string FileName = menuItem.Text.Substring(start, menuItem.Text.Length - start).Trim();		
	
					this.LoadPossibleFile(FileName, true);
				};
				
				FileItem.MenuItems.Add(item);
			}
			FileItem.MenuItems.Add(ClearRecentFileListItem);
			FileItem.MenuItems.Add("-"); 		// Add a seperator (The - makes it a seperator)
			FileItem.MenuItems.Add(ExitItem);
		}
		else
		{
			FileItem.MenuItems.Add(ExitItem);
		}

		
	}

	private void LoadPossibleFile(string file, bool AddRecentFile)
	{
		if (Path.GetExtension(file) == ".ChkLst")
		{
			if (this.IsChkLstFile(file) == true)
			{
				this.SavePath = file;
				settings.LastFileOpened = this.SavePath;
				this.Text = Path.GetFileNameWithoutExtension(this.SavePath) +  " - " +  Form1.WindowText;
				
				this.LoadList(File.ReadAllLines(file));

				if (AddRecentFile == true) settings.AddRecentFile(file);
				this.LoadRecentFiles();
			}
			else
			{
				MessageBox.Show("Error: Invalid Check List file!", Form1.WindowText, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		else
		{
			this.Text = "Untitled Checklist - " + Form1.WindowText;
			this.IsDirty = true;
			
			this.LoadList(File.ReadAllLines(file));
		}
	}

	private bool IsChkLstFile(string path)
	{
		FileStream file = File.Open(path, FileMode.Open);
		byte[] fileData = new byte[Form1.ChkLstFile_MagicNumber.Length];

		file.Read(fileData, 0, fileData.Length);

		file.Close();

		for (int i = 0; i < fileData.Length; i++)
		{
			if (fileData[i] != Form1.ChkLstFile_MagicNumber[i]) return false;
		}
		return true;
	}
	
	private void PanelBox_CTRLVPressed(object sender, EventArgs e)
	{
		LoadClipboardItem_Click(null, null);
	}
	
	private bool CtrlDown = false;
	private bool VDown = false;
	public void Form1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			CtrlDown = true;
			VDown = false;
		}
		else if (e.KeyCode == Keys.V)
		{
			VDown = true;
			
			if (CtrlDown == true)
			{
				PanelBox_CTRLVPressed(null, null);
			}
		}
	}
	
	public void Form1_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			CtrlDown = false;
			if (VDown == true) PanelBox_CTRLVPressed(null, null);
			VDown = false;
		}
		else
		{
			if (e.KeyCode == Keys.V)
			{
				VDown = false;
				if (CtrlDown == true) PanelBox_CTRLVPressed(null, null);
			}
		}
	}

	
	private void Form1_DragDrop(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.All;
		
		DataObject Data = (DataObject)e.Data;
		//StringCollection filesDroped = Data.GetFileDropList();
		string[] filesDroped = new string[Data.GetFileDropList().Count];
		
		if (filesDroped.Length == 1)
		{
			Data.GetFileDropList().CopyTo(filesDroped, 0);
			
			if (Path.GetExtension(filesDroped[0]) == ".txt" || Path.GetExtension(filesDroped[0]) == ".ChkLst")
			{
				this.LoadPossibleFile(filesDroped[0], true);

				// if (Path.GetExtension(filesDroped[0]) == ".ChkLst")
				// {
					// this.SavePath = filesDroped[0];
					// settings.LastFileOpened = this.SavePath;
					// this.Text = Path.GetFileNameWithoutExtension(this.SavePath) +  " - " +  Form1.WindowText;
				// }
				// else
				// {
					// this.Text = "Untitled Checklist - " + Form1.WindowText;
					// this.IsDirty = true;
				// }
				// this.LoadList(File.ReadAllLines(filesDroped[0]));
			}
		}
	}
	
	private void Form1_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.All;
	}

	
	protected override void OnSizeChanged(EventArgs e)
	{
		if (PanelBox != null) PanelBox.Size = new Size(this.ClientSize.Width - (PanelBox.Location.X * 2), this.ClientSize.Height - (PanelBox.Location.Y * 2));
		base.OnSizeChanged(e);
	}
	
	private void NewItem_Click(object sender, EventArgs e)
	{
		foreach (Control ctrl in PanelBox.Controls)
		{
			ctrl.Dispose();
		}
		PanelBox.Controls.Clear();
		this.SavePath = String.Empty;
		this.IsDirty = false;
		this.Text = "Untitled Checklist - " + Form1.WindowText;
	}
	
	private void OpenItem_Click(object sender, EventArgs e)
	{
		if (this.IsDirty == true)
		{
			if (this.SavePath == String.Empty)
			{
				DialogResult result = MessageBox.Show("Save changes to Untitled Checklist ?", Form1.WindowText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					this.Save();
				}
				else if (result == DialogResult.Cancel)
				{
					return;
				}
				this.LoadFile();
			}
			else
			{
				DialogResult result = MessageBox.Show("Save file " + '"'.ToString() + this.SavePath, Form1.WindowText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					this.Save();
				}
				else if (result == DialogResult.Cancel)
				{
					return;
				}
				this.LoadFile();
			}
		}
		else
		{
			this.LoadFile();
		}
		
	}
	
	private void LoadFile()
	{
		OpenFileDialog diag = new OpenFileDialog();
		diag.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
		diag.Filter = "All supported files (*.txt; *.ChkLst)|*.txt;*.ChkLst|Check List Manager Save files (*.ChkLst)|*.ChkLst|Text files (*.txt)|*.txt";
		diag.CheckFileExists = true;
		diag.CheckPathExists = true;
		diag.Multiselect = false;
		
		if (diag.ShowDialog() == DialogResult.OK)
		{
			
			if (Path.GetExtension(diag.FileName) == ".txt" || Path.GetExtension(diag.FileName) == ".ChkLst")
			{
				this.LoadPossibleFile(diag.FileName, true);
			}
		}

	}
	
	private void SaveItem_Click(object sender, EventArgs e)
	 {
		if (this.IsDirty == true)
		{
			this.Save();
		}
	}
	
	private void SaveAsItem_Click(object sender, EventArgs e)
	{
		if (PanelBox.Controls.Count == 0) return;
		
		string res = this.GetSaveFilePath();
		if (res != String.Empty)
		{
			this.SavePath = res;
			Save();
		}
	}
	
	private void ExitItem_Click(object sender, EventArgs e)
	{
		this.Close();
	}
	
	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (this.IsDirty == true)
		{
			if (this.SavePath == String.Empty)
			{
				
				DialogResult result = MessageBox.Show("Save changes to Untitled Checklist ?", Form1.WindowText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					this.Save();
				}
				else if (result == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}
			else
			{
				DialogResult result = MessageBox.Show("Save file " + '"'.ToString() + this.SavePath, Form1.WindowText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					this.Save();
				}
				else if (result == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}
			settings.Save();
		}
	}
	
	private void LoadClipboardItem_Click(object sender, EventArgs e)
	{
		string RawData = Clipboard.GetText(TextDataFormat.Text);
		RawData = RawData.Replace("•  ", "").Replace('"'.ToString() + "   ", "").Replace('"'.ToString() + "  ", "").Replace('"'.ToString() + " ", "").Trim();
		
		string[] Data = RawData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		
		if (Data.Length > 0)
		{
			this.LoadList(Data);
			this.Text = "Untitled Checklist - " + Form1.WindowText;
			this.IsDirty = true;
		}
	}
	
	private void AutoSaveItem_Click(object sender, EventArgs e)
	{
		AutoSaveItem.Checked = !AutoSaveItem.Checked;
		settings.EnableAutoSave = AutoSaveItem.Checked;
	}
	
	private string GetSaveFilePath()
	{
		SaveFileDialog diag = new SaveFileDialog();
		diag.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
		diag.Filter = "Check List Manager Save files (*.ChkLst)|*.ChkLst";
		diag.CheckPathExists = true;
		
		if (this.SavePath == String.Empty)
		{
			diag.FileName = "Untitled Checklist";
		}
		else
		{
			diag.InitialDirectory = Path.GetDirectoryName(this.SavePath);
			diag.FileName = Path.GetFileNameWithoutExtension(this.SavePath);
		}
		
		if (diag.ShowDialog() == DialogResult.OK)
		{
			return diag.FileName;
		}
		return String.Empty;
	}
	
	public void Save()
	{
		if (this.SavePath == String.Empty)
		{
			string res = this.GetSaveFilePath();
			if (res != String.Empty)
			{
				this.SavePath = res;
				settings.LastFileOpened = this.SavePath;
			}
			else
			{
				return;
			}
		}
		
		this.IsDirty = false;
		
		this.Text = Path.GetFileNameWithoutExtension(this.SavePath) +  " - " +  Form1.WindowText;
		
		FileStream file = File.Create(this.SavePath);

		file.Write(Form1.ChkLstFile_MagicNumber, 0, Form1.ChkLstFile_MagicNumber.Length);

		StringBuilder str = new StringBuilder();

		foreach (CheckBox box in PanelBox.Controls)
		{
			string Line = box.Text;
			if (box.Checked == true) Line = Form1.CheckMark + Line;
			str.AppendLine(Line);
		}

		byte[] Data = Encoding.UTF8.GetBytes(str.ToString());

		file.Write(Data, 0, Data.Length);
		file.Close();
	}
	
	private void LoadList(string[] data)
	{
		if (PanelBox == null) return;
		
		foreach (Control ctrl in PanelBox.Controls)
		{
			ctrl.Dispose();
		}
		PanelBox.Controls.Clear();
		
		CheckBox lastBox = new CheckBox();
		lastBox.Location = new Point(6, 6);
		lastBox.AutoSize = true;
		lastBox.Text = data[1].Replace("•  ", "").Replace(Form1.CheckMark, "").Trim();
		
		if (data[1].StartsWith(Form1.CheckMark) == true)
		{
			lastBox.Checked = true;
			lastBox.Enabled = false;
			//lastBox.Font = new Font(lastBox.Font, FontStyle.Strikeout);
		}
		else
		{
			lastBox.Checked = false;
			lastBox.Enabled = true;
		}
		lastBox.CheckedChanged += box_CheckedChanged;
		lastBox.KeyDown += Form1_KeyDown;
		lastBox.KeyUp += Form1_KeyUp;
		
		PanelBox.Controls.Add(lastBox);
		
		for (int i = 2; i < data.Length; i++)
		{
			CheckBox box = new CheckBox();
			box.Location = new Point(6, (lastBox.Location.Y + lastBox.Size.Height) + 3);
			box.AutoSize = true;
			box.Text = data[i].Replace("•  ", "").Replace(Form1.CheckMark, "").Trim();
			
			if (data[i].StartsWith(Form1.CheckMark) == true)
			{
				box.Checked = true;
				box.Enabled = false;
				//box.Font = new Font(box.Font, FontStyle.Strikeout);
			}
			else
			{
				box.Checked = false;
				box.Enabled = true;
			}
			box.CheckedChanged += box_CheckedChanged;
			box.KeyDown += Form1_KeyDown;
			box.KeyUp += Form1_KeyUp;
			
			PanelBox.Controls.Add(box);
			
			lastBox = box;
		}
		
		PanelBox.UpdateSize();
	}
	
	private void box_CheckedChanged(object sender, EventArgs e)
	{
		CheckBox sndr = (CheckBox)sender;
		if (sndr.Checked == true)
		{
			if (MessageBox.Show("Are you sure you completed item: " + sndr.Text + " ?", Form1.WindowText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				sndr.Enabled = false;
				if (this.settings.EnableAutoSave == true)
				{
					this.Save();
					return;
				}
				
				this.IsDirty = true;
			}
			else
			{
				sndr.Checked = false;
			}
		}
	}
}

public class ScrollablePanel : Panel
{
	private Form1 ParentFrm;
	
	public ScrollablePanel(Form1 parentFrm)
	{
		this.ParentFrm = parentFrm;
		
		this.AutoScroll = true;
		
		this.HScroll = true;
		this.VScroll = true;
		
		this.VerticalScroll.Enabled = true;
		this.VerticalScroll.Visible = true;
		
		this.HorizontalScroll.Maximum = 0;
		this.HorizontalScroll.Enabled = false;
		this.HorizontalScroll.Visible = false;
	}
	
	public void UpdateSize()
	{
		int biggestSize = 0;
		Control biggestCtrl = null;
		foreach (Control ctrl in this.Controls)
		{
			if (ctrl.Size.Width > biggestSize)
			{
				biggestSize = ctrl.Size.Width;
				biggestCtrl = ctrl;
			}
		}
		
		Size s = new Size((biggestCtrl.Location.X * 5) + biggestCtrl.Size.Width, this.Size.Height);

		if (s.Width >= this.MinimumSize.Width)
		{
			this.Size = s;

			
			this.ParentFrm.ClientSize = new Size(this.Size.Width + (this.Location.X * 2), this.ParentFrm.ClientSize.Height);
		}
	}
}