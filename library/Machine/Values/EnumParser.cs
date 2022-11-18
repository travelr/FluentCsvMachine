using System.Net.Quic;
using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Tree;

namespace FluentCsvMachine.Machine.Values
{
    internal class EnumParser<T> : ValueParser
    {
        public new enum States
        {
            Parsing,
            Finished
        }

        public new States State { get; private set; } = States.Parsing;

        private readonly TreeNode _root;
        private TreeNode _currentNode;
        private readonly List<T> _enums;

        public EnumParser(Type type)
        {
            Guard.IsNotNull(type);

            var names = Enum.GetNames(type);
            _root = new TreeNode();
            _enums = Enum.GetValues(type).OfType<object>().Select(x => (T)Convert.ChangeType(x, type)).ToList();

            // Create a tree based on the enum values
            CreateTree(_root, names.Select((x, i) => new StringIndex(i, x)));
            _currentNode = _root;
        }

        internal override void Process(char c)
        {
            // Only allow A-Z or a-z, assume Enum values have been stripped of all other chars
            if (State == States.Finished || c is not ((>= 'a' and <= 'z') or (>= 'A' and <= 'Z')))
            {
                return;
            }

            if (!_currentNode.TryGetChild(c, out TreeNode? node))
            {
                ThrowHelper.ThrowCsvMalformedException($"Cannot parse enum. Unknown char {c} ({_currentNode.GetCurrentString()} for type {typeof(T)}");
            }

            if (node!.Index != null && !node.HasChildren())
            {
                State = States.Finished;
            }

            _currentNode = node;
        }

        internal override ResultValue GetResult()
        {
            if (_currentNode.Index == null)
            {
                ThrowHelper.ThrowCsvMalformedException($"Cannot parse enum. Current string ({_currentNode.GetCurrentString()} does not match {typeof(T)}");
            }

            var resultValue = _enums[_currentNode.Index.Value];
            var result = new ResultValue(typeof(T), resultValue!);

            // Reset variables
            State = States.Parsing;
            _currentNode = _root;

            return result;
        }

        /// <summary>
        /// Creates a tree based on a list of strings
        /// One node per char
        /// </summary>
        /// <param name="parent">Previous char</param>
        /// <param name="names">strings at the group position</param>
        /// <param name="i">index of the string</param>
        /// <exception cref="CsvMachineException">Algorithm error</exception>
        private void CreateTree(TreeNode parent, IEnumerable<StringIndex> names, int i = 0)
        {
            // Group by current position int the car
            var group = names.Where(x => x.Value.Length > i).GroupBy(x => x.Value[i]);

            foreach (var g in group)
            {
                // Create new node for the current char
                var node = new TreeNode(g.Key);
                parent.Add(node);

                // Get current string iterating from Root to the current node
                var currentString = node.GetCurrentString();

                // Check if we found a leave, StartsWith is needed (example RED, RED2)
                var final = names.Where(x => x.Value.StartsWith(currentString)).ToList();
                if (final.Count == 1)
                {
                    // Array Index of the initial names array
                    node.Index = final[0].Index;
                }
                else if (final.Count > 0)
                {
                    // Check if we have a full match (currentString = RED, names[RED, RED2])
                    var fullMatch = final.SingleOrDefault(x => names.Any(y => y.Value == currentString) && x.Value == currentString);
                    if (fullMatch != null)
                    {
                        node.Index = fullMatch.Index;
                    }

                    // Recursively check next char
                    CreateTree(node, g, i + 1);
                }
                else
                {
                    throw new CsvMachineException();
                }
            }
        }
    }
}