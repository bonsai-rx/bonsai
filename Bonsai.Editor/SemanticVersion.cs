using System;

namespace Bonsai.Editor
{
    sealed class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>, IComparable
    {
        public SemanticVersion(Version version, string specialVersion)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            SpecialVersion = specialVersion ?? throw new ArgumentNullException(nameof(specialVersion));
        }

        public Version Version { get; private set; }

        public string SpecialVersion { get; private set; }

        public bool Equals(SemanticVersion other)
        {
            if (other is null) return false;
            return Version == other.Version && SpecialVersion == other.SpecialVersion;
        }

        public override bool Equals(object obj)
        {
            var value = obj as SemanticVersion;
            if (value is null) return false;
            return Equals(value);
        }

        public int CompareTo(SemanticVersion other)
        {
            if (other is null)
            {
                return 1;
            }

            var comparison = Version.CompareTo(other.Version);
            if (comparison != 0) return comparison;

            var empty = string.IsNullOrEmpty(SpecialVersion);
            var otherEmpty = string.IsNullOrEmpty(other.SpecialVersion);
            if (empty && otherEmpty)
            {
                return 0;
            }
            else if (empty)
            {
                return 1;
            }
            else if (otherEmpty)
            {
                return -1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return 1;
            }

            var value = obj as SemanticVersion;
            if (value is null)
            {
                throw new ArgumentException("The specified object is not of the correct type.", nameof(obj));
            }

            return CompareTo(value);
        }

        public override int GetHashCode()
        {
            var hashCode = Version.GetHashCode();
            if (!string.IsNullOrEmpty(SpecialVersion))
            {
                hashCode += 31 * SpecialVersion.GetHashCode();
            }
            return hashCode;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SpecialVersion))
            {
                return string.Format("{0}-{1}", Version, SpecialVersion);
            }
            else return Version.ToString();
        }

        public static SemanticVersion Parse(string version)
        {
            if (!TryParse(version, out SemanticVersion result))
            {
                throw new ArgumentException("The specified version string has an invalid format.", nameof(version));
            }

            return result;
        }

        public static bool TryParse(string version, out SemanticVersion value)
        {
            if (string.IsNullOrEmpty(version))
            {
                value = null;
                return false;
            }

            string specialVersion;
            var hyphen = version.IndexOf('-');
            if (hyphen >= 0)
            {
                specialVersion = version.Substring(hyphen + 1);
                version = version.Substring(0, hyphen);
            }
            else specialVersion = string.Empty;

            if (Version.TryParse(version, out Version baseVersion))
            {
                value = new SemanticVersion(baseVersion, specialVersion);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return left.Equals(right);
            else return right is null;
        }

        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return !left.Equals(right);
            else return right is object;
        }

        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return left.CompareTo(right) < 0;
            else return right is object;
        }

        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return left.CompareTo(right) <= 0;
            else return true;
        }

        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return left.CompareTo(right) > 0;
            else return false;
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            if (left is object) return left.CompareTo(right) >= 0;
            else return right is null;
        }
    }
}
