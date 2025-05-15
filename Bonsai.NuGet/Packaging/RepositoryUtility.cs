using Microsoft.Build.Tasks.Git;
using NuGet.Packaging.Core;

namespace Bonsai.NuGet.Packaging
{
    public class RepositoryUtility
    {
        public static RepositoryMetadata GetRepositoryMetadata(string directory)
        {
            if (!GitRepository.TryFindRepository(directory, out GitRepositoryLocation location))
                return null;

            var repository = GitRepository.OpenRepository(location, new GitEnvironment(null));
            var repositoryUrl = GitOperations.GetRepositoryUrl(repository, null);
            var headCommitSha = repository.GetHeadCommitSha();
            if (string.IsNullOrEmpty(repositoryUrl) || headCommitSha is null)
                return null;

            return new RepositoryMetadata
            {
                Type = "git",
                Url = repositoryUrl,
                Commit = headCommitSha,
                Branch = repository.GetBranchName()
            };
        }
    }
}
