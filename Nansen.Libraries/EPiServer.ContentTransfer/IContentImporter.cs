using System.IO;

namespace EPiServer.ContentTransfer
{
	public interface IContentImporter
	{
		int Created { get; }
		int Updated { get; }
		int Failed { get; }
		void Run(Stream fileStream);
	}
}
