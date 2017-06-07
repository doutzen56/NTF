using System.Collections.Generic;

namespace NTF.SQL
{
    public sealed class DefaultCmdTemplate : CmdTemplate
    {
        private static string deleteTmp = "DELETE FROM #TableName WHERE #Where;";
        public override string DeleteTmp
        {
            get
            {
                return deleteTmp;
            }

            set
            {
                deleteTmp = value;
            }
        }

        private static string updateTmp = "UPDATE #TableName SET #NameValues WHERE #Where;";
        public override string UpdateTmp
        {
            get
            {
                return updateTmp;
            }

            set
            {
                updateTmp = value;
            }
        }

        private static string insertTmp = "INSERT INTO #TableName (#Columns) VALUES (#Values);";
        public override string InsertTmp
        {
            get
            {
                return insertTmp;
            }

            set
            {
                insertTmp = value;
            }
        }

        private static string getFirstTmp = "SELECT TOP 1 #Columns FROM #TableName WHERE #Where #OrderBy;";
        public override string GetFirstTmp
        {
            get
            {
                return getFirstTmp;
            }

            set
            {
                getFirstTmp = value;
            }
        }

        private static string getListTmp = "SELECT #Columns FROM #TableName WHERE #Where #OrderBy;";
        public override string GetListTmp
        {
            get
            {
                return getListTmp;
            }

            set
            {
                getListTmp = value;
            }
        }

        private static string getPageListTmp = "SELECT COUNT(1) FROM #TableName WHERE #Where;SELECT #Columns FROM ( SELECT #Columns,ROW_NUMBER() OVER(#OrderBy) AS ROW_NUMBER FROM #TableName WHERE #Where ) T1 WHERE ROW_NUMBER > #LeftThan AND ROW_NUMBER <= #RightThan;";
        public override string GetPageListTmp
        {
            get
            {
                return getPageListTmp;
            }

            set
            {
                getPageListTmp = value;
            }
        }

        public override string Replace(string template, Dictionary<ReplaceArgs, string> parameters)
        {
            return base.Replace(template, parameters);
        }
    }
}
