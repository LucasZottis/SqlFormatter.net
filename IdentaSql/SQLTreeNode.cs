using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLFormater
{
    interface ISQLTreeNode
    {
        object Content { get; }
    }


    class ParenthesisNode : ISQLTreeNode
    {
        private SQLTree tree;

        internal SQLTree Tree
        {
            get { return tree; }
            set { tree = value; }
        }

        #region ISQLTreeNode Membres
        public object Content
        {
            get
            {
                return tree;
            }
        }

        #endregion
    }

    class StringNode : ISQLTreeNode
    {
        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #region ISQLTreeNode Membres
        public object Content
        {
            get
            {
                return value;
            }
        }
        #endregion
    }
}
