namespace FluentCsvMachine.Machine.Tree
{
    /// <summary>
    /// Helper class for TreeNode
    /// </summary>
    internal class StringIndex
    {
        public StringIndex(int index, string name)
        {
            Index = index;
            Value = name;
        }

        /// <summary>
        /// Element index in the original array
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Element value
        /// </summary>
        public string Value { get; }
    }
}