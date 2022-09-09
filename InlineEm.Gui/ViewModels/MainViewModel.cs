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

// TODO: Clean this up!

public class MainViewModel : ViewModel
{
	public MainViewModel()
	{
		SetOutputFolderPreferenceCommand = new DelegateCommand<OutputFolderPreference>((dir) =>
		{
			OutputFolderPreference = dir;

			if (dir == OutputFolderPreference.Specific && !OutputFolderSpecified)
			{
				if (FormsHelpers.GetFolder() is string s)
				{
					OutputFolder = s;
				}
				else if (!OutputFolderSpecified)
				{
					OutputFolderPreference = OutputFolderPreference.Same;
				}
			}

			FixJobOutputsFromPreference();
		});

		PromptForOutputFolderCommand = new DelegateCommand(() =>
		{
			if (FormsHelpers.GetFolder() is string s)
			{
				OutputFolder = s;
			}
		});

		AddFileCommand = new DelegateCommand(() =>
		{
			if (FormsHelpers.GetFiles("MHTML Files|*.mhtml") is string[] files)
			{
				var existingFiles = Jobs.Select((t) => t.Input).ToHashSet();

				var jobs = files
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

		AddFolderCommand = new DelegateCommand(() =>
		{
			if (FormsHelpers.GetFolder() is string folder)
			{
				var existingFiles = Jobs.Select((t) => t.Input).ToHashSet();

				var jobs = Directory.EnumerateFiles(folder, "*.mhtml", new EnumerationOptions()
				{
					RecurseSubdirectories = true,
				})
					.Where((f) => !existingFiles.Contains(f))
					.Select((f) => new Job(f));

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
	public IDelegateCommand AddFolderCommand { get; }
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
		set
		{
			if (SetProperty(ref _OutputFolder, value))
			{
				RaisePropertyChanged(nameof(OutputFolderSpecified));
			}
		}
	}

	public bool OutputFolderSpecified => !String.IsNullOrWhiteSpace(OutputFolder);

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
				FormsHelpers.SetClipboardText(task.Result);
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
