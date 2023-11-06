﻿using SqlFormatter.Interfaces;

namespace SqlFormatter.Models;

public class StringNode : ISQLTreeNode
{
    private string value;

    public string Value
    {
        get { return value; }
        set { this.value = value; }
    }

    public object Content
    {
        get
        {
            return value;
        }
    }

    public StringNode( string value )
    {
        Value = value;
    }

    public StringNode() : this( string.Empty ) { }
}