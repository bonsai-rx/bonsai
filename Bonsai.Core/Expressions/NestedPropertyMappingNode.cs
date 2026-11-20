using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    internal class NestedPropertyMappingNode
    {
        public List<NestedPropertyMappingNode> Children { get; } = new List<NestedPropertyMappingNode>();
        public string Name { get; set; }
        public string Selector { get; set; }

        internal static List<NestedPropertyMappingNode> BuildNodeTree(PropertyMappingCollection propertyMappings)
        {
            List<NestedPropertyMappingNode> tree = new List<NestedPropertyMappingNode>();
            foreach (var mapping in propertyMappings)
            {
                string[] targetPath = mapping.Name.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
                string selector = mapping.Selector;
                BuildNodeBranch(tree, targetPath, selector);

            }
            return tree;
        }

        internal static void BuildNodeBranch(List<NestedPropertyMappingNode> nodes, string[] targetPath, string selector)
        {
            NestedPropertyMappingNode node = nodes.Find(n => n.Name == targetPath[0]);
            if (node == null)
            {
                NestedPropertyMappingNode newNode = new NestedPropertyMappingNode();
                newNode.Name = targetPath[0];
                nodes.Add(newNode);
                if (targetPath.Length > 1)
                {
                    BuildNodeBranch(newNode.Children, targetPath.Skip(1).ToArray(), selector);
                }
                else //Tip of the branch, this is the member we want to update
                {
                    newNode.Selector = selector;
                }

            }
            else
            {
                if (targetPath.Length == 0 || node.Children.Count != 0)
                {
                    //This means that the target member of an assignment has been assigned before
                    //This should not happen, so we throw an error
                    throw new InvalidOperationException(string.Format("Trying to assign member {0} more than once", string.Join(".", targetPath)));
                }
                else
                {
                    BuildNodeBranch(node.Children, targetPath.Skip(1).ToArray(), selector);
                }
            }
        }
    }
}
