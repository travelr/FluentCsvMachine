using System.Collections;

namespace FluentCsvMachine.Machine.Tree
{
    internal class TreeNode
    {
        private readonly Dictionary<char, TreeNode> _children = new();

        /// <summary>
        /// Current char serves as the ID of the node
        /// Null means Root
        /// </summary>
        public char? Character { get; private set; }

        /// <summary>
        /// Node matches a full value, Index of the value in the source array
        /// </summary>
        public int? Index { get; set; }

        public TreeNode? Parent { get; private set; }

        public TreeNode()
        {
            // Root: Id is null
        }

        public TreeNode(char c)
        {
            Character = c;
        }

        public bool HasChildren()
        {
            return _children.Count > 0;
        }

        internal bool TryGetChild(char c, out TreeNode? node)
        {
            if (_children.ContainsKey(c))
            {
                node = _children[c];
                return true;
            }
            node = null;
            return false;
        }

        public void Add(TreeNode item)
        {
            item.Parent = this;
            if (!item.Character.HasValue)
            {
                throw new InvalidOperationException("Cannot add root as a child");
            }

            _children.Add((char)item.Character!, item);
        }

        /// <summary>
        /// Gets the string from root to the current node
        /// </summary>
        /// <returns></returns>
        public string GetCurrentString()
        {
            if (!Character.HasValue)
            {
                throw new InvalidOperationException("Not allowed for root");
            }

            var stack = new Stack<char>();
            var node = this;

            do
            {
                stack.Push((char)node.Character!);
                node = node.Parent;
            } while (node != null && node.Character.HasValue);

            var result = new string(stack.AsEnumerable().ToArray());
            return result;
        }
    }
}