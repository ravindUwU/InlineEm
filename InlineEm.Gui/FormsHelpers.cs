namespace InlineEm.Gui;

using System.Linq;
using System.Windows.Forms;

/// <summary>
/// Provides helper Windows Forms helpers.
/// </summary>
public static class FormsHelpers
{
	/// <summary>
	/// Shows a <see cref="FolderBrowserDialog">folder browser dialog</see>.
	/// </summary>
	/// <returns>The selected folder, or <c>null</c> if the selection is cancelled.</returns>
	public static string? GetFolder()
	{
		using var dialog = new FolderBrowserDialog();
		return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
	}

	/// <summary>
	/// Shows an <see cref="OpenFileDialog">open file dialog</see> with multiple selection enabled.
	/// </summary>
	/// <param name="filter">
	/// The <see cref="FileDialog.Filter"/> that defines the files that are available for selection.
	/// </param>
	/// <returns>The selected file(s) or <c>null</c> if the selection is empty/cancelled.</returns>
	public static string[]? GetFiles(string filter)
	{
		using var dialog = new OpenFileDialog
		{
			Multiselect = true,
			Filter = filter,
		};

		return dialog.ShowDialog() == DialogResult.OK && dialog.FileNames.Any() ? dialog.FileNames : null;
	}

	/// <summary>
	/// Shows an <see cref="OpenFileDialog">open file dialog</see> for single file selection.
	/// </summary>
	/// <param name="filter">
	/// The <see cref="FileDialog.Filter"/> that defines the files that are available for selection.
	/// </param>
	/// <returns>The selected file or <c>null</c> if the selection is empty/cancelled.</returns>
	public static string? GetFile(string filter)
	{
		using var dialog = new OpenFileDialog
		{
			Filter = filter,
		};

		return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
	}

	/// <summary>
	/// Copies the specified string to the clipboard via a <see cref="TextBox"/>.
	/// </summary>
	public static void SetClipboardText(string s)
	{
		using var t = new TextBox()
		{
			Text = s,
		};
		t.SelectAll();
		t.Copy();
	}
}
