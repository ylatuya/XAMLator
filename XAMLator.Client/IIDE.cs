using System;
using System.Threading.Tasks;

namespace XAMLator.Client
{
	/// <summary>
	/// Interface for the IDE specifics (Visual Studio or Visual Studio for macOS aka MonoDevelop).
	/// </summary>
	public interface IIDE
	{
		/// <summary>
		/// Occurs when document changed.
		/// </summary>
		event EventHandler<DocumentChangedEventArgs> DocumentChanged;

		/// <summary>
		/// Start monitoring changes in the IDE.
		/// </summary>
		void MonitorEditorChanges();

		/// <summary>
		/// Shows an error to the user.
		/// </summary>
		/// <param name="error">Error.</param>
		/// <param name="ex">Exception.</param>
		void ShowError(string error, Exception ex = null);

		Task RunTarget(string targetName);
	}
}
