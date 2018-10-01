using Microsoft.Build.Framework;
using System.Collections;

namespace XAMLator.Client
{
	class TaskItem : ITaskItem
	{
		public TaskItem(string filePath)
		{
			ItemSpec = filePath;
		}

		public string ItemSpec { get; set; }
		public int MetadataCount => 0;
		public ICollection MetadataNames => null;
		public IDictionary CloneCustomMetadata() => null;
		public void CopyMetadataTo(ITaskItem destinationItem) { }
		public string GetMetadata(string metadataName) => null;
		public void RemoveMetadata(string metadataName) { }
		public void SetMetadata(string metadataName, string metadataValue) { }
	}
}