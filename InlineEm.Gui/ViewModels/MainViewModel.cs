namespace InlineEm.Gui.ViewModels;

using InlineEm.Gui.Mvvm;
using InlineEm.Lib;
using MimeKit;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// TODO: Clean this up!

public class MainViewModel : ViewModel
{
	public MainViewModel()
	{
		SetOutputFolderPreferenceCommand = new DelegateCommand<OutputFolderPreference>((dir) =>
		{
			OutputFolderPreference = dir;

			if (dir == OutputFolderPreference.Specific && String.IsNullOrWhiteSpace(OutputFolder))
			{
				PromptForOutputFolder();
			}

			FixJobOutputsFromPreference();
		});

		PromptForOutputFolderCommand = new DelegateCommand(() => PromptForOutputFolder());

		AddFileCommand = new DelegateCommand(() =>
		{
			using var dialog = new OpenFileDialog
			{
				Multiselect = true,
				Filter = "MHTML Files|*.mhtml",
			};

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				var existingFiles = Jobs.Select((t) => t.Input).ToHashSet();

				var jobs = dialog.FileNames
					.Where((n) => !existingFiles.Contains(n))
					.Select((n) => new Job(n));

				foreach (var job in jobs)
				{
					job.FixForOutputFolderPreference(OutputFolderPreference, OutputFolder);
					Jobs.Add(job);
				}

				OnJobsChanged();
			}
		});

		ClearCommand = new DelegateCommand(
			() =>
			{
				Jobs.Clear();
				OnJobsChanged();
			},
			() => Jobs.Count > 0
		);

		ConvertCommand = new DelegateCommand(
			() =>
			{
				IsBusy = true;

				var tasks = Jobs.Select((it) => it.Execute()).ToArray();

				Task.WhenAll(tasks).ContinueWith((_) =>
				{
					IsBusy = false;
				});
			},
			() => Jobs.Count > 0
		);
	}

	public IDelegateCommand SetOutputFolderPreferenceCommand { get; }
	public IDelegateCommand PromptForOutputFolderCommand { get; }
	public IDelegateCommand AddFileCommand { get; }
	public IDelegateCommand ClearCommand { get; }
	public IDelegateCommand ConvertCommand { get; set; }

	public ObservableCollection<Job> Jobs { get; } = new();

	private OutputFolderPreference _OutputFolderPreference = OutputFolderPreference.Same;
	public OutputFolderPreference OutputFolderPreference
	{
		get { return _OutputFolderPreference; }
		set { SetProperty(ref _OutputFolderPreference, value); }
	}

	private string _OutputFolder = String.Empty;
	public string OutputFolder
	{
		get { return _OutputFolder; }
		set { SetProperty(ref _OutputFolder, value); }
	}

	private bool _IsBusy = false;
	public bool IsBusy
	{
		get { return _IsBusy; }
		set { SetProperty(ref _IsBusy, value); }
	}

	private void OnJobsChanged()
	{
		ClearCommand?.RaiseCanExecuteChanged();
		ConvertCommand?.RaiseCanExecuteChanged();
	}

	private void PromptForOutputFolder()
	{
		using var dialog = new FolderBrowserDialog();
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			OutputFolder = dialog.SelectedPath;
		}
	}

	private void FixJobOutputsFromPreference()
	{
		foreach (var job in Jobs)
		{
			job.FixForOutputFolderPreference(OutputFolderPreference, OutputFolder);
		}
	}
}

public enum OutputFolderPreference
{
	Same,
	Specific,
}

public enum JobStatus
{
	Idle,
	Success,
	Warning,
	Error,
}

public class Job : ObservableObject
{
	public Job(string input)
	{
		_Input = input;

		OpenCommand = new DelegateCommand(
			() => Process.Start("explorer.exe", Output!),
			() => Converted
		);

		ShowCommand = new DelegateCommand(
			() => Process.Start("explorer.exe", $"/select, {Output}"),
			() => Converted
		);

		CopyCommand = new DelegateCommand(
			() => File.ReadAllTextAsync(Output!).ContinueWith((task) =>
			{
				using var t = new TextBox()
				{
					Text = task.Result,
				};
				t.SelectAll();
				t.Copy();
			}),
			() => Converted
		);

		FixForStatus();
	}

	public IDelegateCommand OpenCommand { get; set; }
	public IDelegateCommand ShowCommand { get; set; }
	public IDelegateCommand CopyCommand { get; set; }

	private string _Input;
	public string Input
	{
		get { return _Input; }
		set { SetProperty(ref _Input, value); }
	}

	private string? _Output;
	public string? Output
	{
		get { return _Output; }
		set
		{
			if (SetProperty(ref _Output, value))
			{
				OpenCommand.RaiseCanExecuteChanged();
			}
		}
	}

	private JobStatus _Status;
	public JobStatus Status
	{
		get { return _Status; }
		set
		{
			if (SetProperty(ref _Status, value))
			{
				FixForStatus();
			}
		}
	}

	private bool _Converted;
	public bool Converted
	{
		get { return _Converted; }
		set { SetProperty(ref _Converted, value); }
	}

	public Exception? Error { get; private set; }

	private string _Description = String.Empty;
	public string Description
	{
		get { return _Description; }
		set { SetProperty(ref _Description, value); }
	}

	private void FixForStatus()
	{
		Converted = Status == JobStatus.Success || Status == JobStatus.Warning;

		Description = Status switch
		{
			JobStatus.Success => "Converted",
			JobStatus.Warning => "Converted with warnings",
			JobStatus.Error => $"An error occured during conversion: {Error?.Message}",
			JobStatus.Idle => "Awaiting conversion",
			_ => String.Empty,
		};

		OpenCommand.RaiseCanExecuteChanged();
		ShowCommand.RaiseCanExecuteChanged();
		CopyCommand.RaiseCanExecuteChanged();
	}

	public void FixForOutputFolderPreference(OutputFolderPreference preference, string outputFolder)
	{
		switch (preference)
		{
			case OutputFolderPreference.Same:
				{
					Output = $"{Path.GetDirectoryName(_Input)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(_Input)}.html";
					break;
				}

			case OutputFolderPreference.Specific:
				{
					Output = $"{outputFolder}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(_Input)}.html";
					break;
				}
		}
	}

	public async Task Execute()
	{
		try
		{
			Status = JobStatus.Idle;

			var message = await MimeMessage.LoadAsync(Input);
			var doc = await Inliner.InlineAsync(message);
			doc.Save(Output);

			Status = JobStatus.Success;
		}
		catch (Exception ex)
		{
			Error = ex;
			Status = JobStatus.Error;
		}
	}
}
