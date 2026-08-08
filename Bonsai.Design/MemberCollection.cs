using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bonsai.Design
{
    /// <summary>
    /// Allows to create an emulated type with a custom list of members.
    /// This is useful for editors that show list of members and accept a Type parameter
    /// </summary>
    internal class MemberCollection : TypeDelegator, IExtendedVisitableType
    {
        /// <summary>
        /// Represents a member on the emulated type
        /// </summary>
        public class MemberCollectionElement
        {
            /// <summary>
            /// True if the member should be treated as non public, false otherwise
            /// </summary>
            public bool IsNonPublic { get; }
            /// <summary>
            /// True if the member should be treated as static, false otherwise
            /// </summary>
            public bool IsStatic { get; }
            /// <summary>
            /// The <see cref="MemberInfo"/> that this object represents
            /// </summary>
            public MemberInfo Member { get; }
            /// <summary>
            /// Creates an instance of <see cref="MemberCollectionElement"/>
            /// </summary>
            /// <param name="member">The <see cref="MemberInfo"/> that this object represents</param>
            /// <param name="isNonPublic">True if the member should be treated as non public, false otherwise</param>
            /// <param name="isStatic">True if the member should be treated as static, false otherwise</param>
            public MemberCollectionElement (MemberInfo member, bool isNonPublic = false, bool isStatic = false)
            {
                IsNonPublic = isNonPublic;
                IsStatic = isStatic;    
                Member = member;
            }
        }

        readonly IReadOnlyList<MemberCollectionElement> members;
        readonly string name;

        ///<inheritdoc/>
        public override string Name => name;
        ///<inheritdoc/>
        public override string FullName => name;
        ///<inheritdoc/>
        public override string Namespace => null;


        /// <summary>
        /// Creates an instance of <see cref="MemberCollection"/> from a list of <see cref="MemberCollectionElement"/>
        /// Members must be properties or fields
        /// </summary>
        /// <param name="members">List of <see cref="MemberCollectionElement"/> representing the members of the emulated type</param>
        /// <param name="name">Name of the emulated type</param>
        /// <param name="fullName">Fully qualified name of the emulated type</param>
        /// <exception cref="ArgumentException">Throws if members contain others than proeprties or fields</exception>
        public MemberCollection(IEnumerable<MemberCollectionElement> members, string name) : base(typeof(object))
        {
            if (members.Any((m) => !(m.Member is PropertyInfo || m.Member is FieldInfo || m.Member is IExtendedVisitableMemberInfo)))
            {
                throw new ArgumentException("Only Properties and Fields are supported", nameof(members));
            }
            this.members = members.ToArray();
            this.name = name;
        }

        /// <summary>
        /// Creates an instance of <see cref="MemberCollection"/> from a list of <see cref="MemberInfo"/>
        /// Members must be properties or fields
        /// All memebers are set as instanced and public
        /// </summary>
        /// <param name="members">List of <see cref="MemberInfo"/> representing the members of the emulated type</param>
        /// <param name="name">Name of the emulated type</param>
        /// <param name="fullName">Fully qualified name of the emulated type</param>
        /// <exception cref="ArgumentException">Throws if members contain others than proeprties or fields</exception>
        public MemberCollection(IEnumerable<MemberInfo> members, string name)
            : this(members.Select(m => new MemberCollectionElement(m)), name) { }
        
        ///<inheritdoc/>
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return members.Where(m => m.Member is PropertyInfo && CheckBindingFlags(m, bindingAttr)).Select(m => (PropertyInfo)m.Member).ToArray();
        }

        ///<inheritdoc/>
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return members.Where(m => m.Member is FieldInfo && CheckBindingFlags(m, bindingAttr)).Select(m => (FieldInfo)m.Member).ToArray();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            StringComparer comparer = (bindingAttr & BindingFlags.IgnoreCase) != 0 ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

            return (from member in members where member.Member is PropertyInfo
                   let p = (PropertyInfo)member.Member
                   where comparer.Equals(p.Name, name) && CheckBindingFlags(member, bindingAttr)
                   select p).FirstOrDefault();
        }

        ///<inheritdoc/>
        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            StringComparer comparer = (bindingAttr & BindingFlags.IgnoreCase) != 0 ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

            return (from member in members
                    where member.Member is FieldInfo
                    let f = (FieldInfo)member.Member
                    where comparer.Equals(f.Name, name) && CheckBindingFlags(member, bindingAttr)
                    select f).FirstOrDefault();
        }

        ///<inheritdoc/>
        public override MemberInfo[] GetDefaultMembers()
        {
            return Array.Empty<MemberInfo>();
        }


        static bool CheckBindingFlags(MemberCollectionElement member, BindingFlags flags)
        {
            if ((flags & BindingFlags.Public) != 0 && member.IsNonPublic) return false;
            if ((flags & BindingFlags.NonPublic) != 0 && !member.IsNonPublic) return false;
            if ((flags & BindingFlags.Instance) != 0 && member.IsStatic) return false;
            if ((flags & BindingFlags.Static) != 0 && !member.IsStatic) return false;
            return true;
        }

        public IEnumerable<MemberInfo> GetExtendedMembers()
        {
            return members.Where(m => m.Member is IExtendedVisitableMemberInfo).Select(m => m.Member);
        }

        public MemberInfo GetExtendedMember(string name)
        {
            return members.Where(m => m.Member is IExtendedVisitableMemberInfo && m.Member.Name == name).Select(m => m.Member).FirstOrDefault();
        }
    }
}
