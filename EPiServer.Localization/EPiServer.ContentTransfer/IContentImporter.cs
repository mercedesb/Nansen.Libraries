using System.IO;

namespace BlueBuffalo.Core.ContentTransfer
{
	public interface IContentImporter
	{
		int Created { get; }
		int Updated { get; }
		int Failed { get; }
		void Run(Stream fileStream);
	}
}
