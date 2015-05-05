using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Inedo.BuildMasterExtensions.Git.Clients
{
    internal sealed class GitCommit : IEquatable<GitCommit>
    {
        private byte[] rev;

        public GitCommit(string commit)
        {
            if (string.IsNullOrEmpty(commit))
                throw new ArgumentNullException("commit");

            this.rev = new byte[commit.Length / 2];
            for (int i = 0; i < rev.Length; i++)
                rev[i] = byte.Parse(commit.Substring(i * 2, 2), NumberStyles.HexNumber);
        }

        public bool Equals(GitCommit other)
        {
            if(object.ReferenceEquals(other, null))
                return false;

            return StructuralComparisons.StructuralEqualityComparer.Equals(this.rev, other.rev);
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GitCommit);
        }
        public override int GetHashCode()
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(this.rev);
        }
        public override string ToString()
        {
            return string.Join(string.Empty, this.rev.Select(b => b.ToString("x")));
        }
    }
}
