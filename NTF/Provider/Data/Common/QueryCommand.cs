﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NTF.Data.Common
{
    /// <summary>
    /// SQL查询命令，包括参数
    /// </summary>
    public class QueryCommand
    {
        string commandText;
        ReadOnlyCollection<QueryParameter> parameters;

        public QueryCommand(string commandText, IEnumerable<QueryParameter> parameters)
        {
            this.commandText = commandText;
            this.parameters = parameters.ToReadOnly();
        }

        public string CommandText
        {
            get { return this.commandText; }
        }

        public ReadOnlyCollection<QueryParameter> Parameters
        {
            get { return this.parameters; }
        }
    }

    public class QueryParameter
    {
        string name;
        Type type;
        QueryType queryType;

        public QueryParameter(string name, Type type, QueryType queryType)
        {
            this.name = name;
            this.type = type;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Type Type
        {
            get { return this.type; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }
    }
}
