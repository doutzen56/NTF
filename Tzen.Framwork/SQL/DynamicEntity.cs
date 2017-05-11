using System.Reflection;
using System.Text;

namespace Tzen.Framwork.SQL
{
    public static class DynamicEntity<T> 
    {
        public static PropertyInfo[] Fields { get; private set; }

        public static string SqlFields { get; private set; }

        public static string InsertedSqlFields { get; private set; }

        static DynamicEntity()
        {
            Fields = typeof(T).GetProperties();
            SetSqlFields();
            SetInsertedFields();
        }

        private static void SetSqlFields()
        {
            var wroted = false;
            var builder = new StringBuilder();
            foreach (var field in Fields)
            {
                if (wroted)
                    builder.Append(",");
                builder.Append(field.Name);
                wroted = true;
            }
            SqlFields = builder.ToString();
        }

        private static void SetInsertedFields()
        {
            var wroted = false;
            var builder = new StringBuilder();
            foreach (var field in Fields)
            {
                if (wroted)
                    builder.Append(",");
                builder.Append("INSERTED.");
                builder.Append(field.Name);
                wroted = true;
            }
            InsertedSqlFields = builder.ToString();
        }
    }
}
