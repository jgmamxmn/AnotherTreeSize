namespace AnotherTreeSize
{
	public partial class Main : Form
	{
		public readonly int MyMargin, MyHeight, MySliceWidth;

		public Main()
		{
			InitializeComponent();

			MyMargin = 10;
			MyHeight = ClientSize.Height - (MyMargin * 2);
			MySliceWidth = 100;

			var FBD = new FolderBrowserDialog();
			if (FBD.ShowDialog() == DialogResult.OK)
			{
				Tree = new Entry(FBD.SelectedPath);
				RenderTree(Tree, MyMargin);
				this.Focus();
			}
			else
			{
				Close();
			}
		}

		Entry Tree;

		List<Button[]> Columns = new List<Button[]>();

		void RenderTree(Entry E, int OffsetX)
		{
			var Col = new List<Button>();
			var ColIdx = Columns.Count;

			long TotalSize = E.SizeBytes;
			int Y = MyMargin;
			foreach(var C in E.Children ?? new List<Entry>())
			{
				var H = (int)(MyHeight * C.SizeBytes / TotalSize);
				if (H > 2)
				{
					var B = new Button2
					{
						Left = OffsetX,
						Top = Y,
						Width = MySliceWidth,
						Height = H,
						Text = C.RelName,
						E = C,
						ColIdx = ColIdx,
					};

					Y += H;

					B.Click += B_Click;

					Controls.Add(B);

					AllTooltips.SetToolTip(B, $"{C.RelName} - {C.SizeUnits()}");

					Col.Add(B);
				}
			}
			Columns.Add(Col.ToArray());
		}

		private void B_Click(object? sender, EventArgs e)
		{
			if(sender is Button2 B)
			{
				while(Columns.Count > (B.ColIdx+1))
				{
					var DeadColumn = Columns.Last();
					foreach (var X in DeadColumn)
						Controls.Remove(X);
					Columns.RemoveAt(Columns.Count - 1);
				}
				RenderTree(B.E, B.Left + B.Width + MyMargin);
			}
		}

		class Button2 : Button
		{
			public Entry E;
			public int ColIdx;
		}

		class Entry
		{
			public Entry(in string _AbsName)
			{
				AbsName = _AbsName;
				RelName = System.IO.Path.GetFileName(_AbsName);
				Children = new List<Entry>();
				if (File.Exists(_AbsName))
				{
					var FI = new FileInfo(_AbsName);
					SizeBytes = FI.Length;
				}
				else
				{
					//Children = new List<Entry>();
					SizeBytes = 0;

					foreach (var SubArr in new[] { Directory.GetFiles(AbsName), Directory.GetDirectories(AbsName) })
					{
						foreach (var F in SubArr)
						{
							var FE = new Entry(F);
							SizeBytes += FE.SizeBytes;
							Children.Add(FE);
						}
					}
					Children.Sort(new Comparison<Entry>((A, B) =>
					{
						var Diff = B.SizeBytes - A.SizeBytes;
						if (Diff < 0) return -1;
						if (Diff > 0) return 1;
						return 0;
					}));
				}
			}

			public List<Entry> Children;


			public readonly string AbsName, RelName;
			public readonly long SizeBytes;

			public static string[] Units = new string[] { "B", "KB", "MB", "GB", "TB" };
			public string SizeUnits()
			{
				int i = 0, mx = Units.Length;
				long size = SizeBytes;
				while(size > 1024 && i<mx)
				{
					++i;
					size /= 1024;
				}
				return string.Format("{0:n0}", size) + " " + Units[i];
			}
		}


	}
}