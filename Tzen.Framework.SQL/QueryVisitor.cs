using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Tzen.Framework.SQL
{
    /// <summary>
    /// SQL条件解析器
    /// </summary>
    /// <remarks>
    /// 引用自：https://blogs.msdn.microsoft.com/mattwar/2007/07/31/linq-building-an-iqueryable-provider-part-ii/
    /// </remarks>
    public class QueryVisitor : ExpressionVisitor
    {
        private StringBuilder builder;
        private BuildCommand cmd;
        private int index;

        public BuildCommand Translate(Expression expression)
        {
            index = 0;
            cmd = new BuildCommand();
            this.builder = new StringBuilder();
            Visit(expression);
            cmd.SQL = builder.ToString();
            return cmd;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                var member = node.Operand as MemberExpression;
                if (member == null)
                    throw new InvalidOperationException("一元运算符只能用在成员上");
                builder.AppendFormat("({0}=0)", member.Member.Name);
                return node;
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            #region 处理恒真恒假

            if (node.NodeType == ExpressionType.AndAlso)
            {
                if (node.Left.NodeType == ExpressionType.Constant && node.Left.Type == typeof(bool))
                {
                    if ((bool)((ConstantExpression)node.Left).Value)
                        Visit(node.Right);
                    else
                        builder.Append("1=0");
                    return node;
                }

                if (node.Right.NodeType == ExpressionType.Constant && node.Right.Type == typeof(bool))
                {
                    if ((bool)((ConstantExpression)node.Right).Value)
                        Visit(node.Left);
                    else
                        builder.Append("1=0");
                    return node;
                }
            }

            if (node.NodeType == ExpressionType.OrElse)
            {
                if (node.Left.NodeType == ExpressionType.Constant && node.Left.Type == typeof(bool))
                {
                    if (!(bool)((ConstantExpression)node.Left).Value)
                        Visit(node.Right);
                    return node;
                }

                if (node.Right.NodeType == ExpressionType.Constant && node.Right.Type == typeof(bool))
                {
                    if (!(bool)((ConstantExpression)node.Right).Value)
                        Visit(node.Left);
                    return node;
                }
            }

            #endregion

            builder.Append("(");
            Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    builder.Append(" AND ");
                    break;

                case ExpressionType.OrElse:
                    builder.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (node.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Right).Value == null)
                        builder.Append(" IS ");
                    else
                        builder.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    if (node.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Right).Value == null)
                        builder.Append(" IS NOT ");
                    else
                        builder.Append(" <> ");
                    break;

                case ExpressionType.LessThan:
                    builder.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    builder.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    builder.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    builder.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported.", node.NodeType));
            }
            Visit(node.Right);
            builder.Append(")");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "StartsWith":
                    if (node.Method.DeclaringType == typeof(string))
                    {
                        var member = (node.Object as MemberExpression);
                        builder.Append(member.Member.Name);
                        var paramName = "@P" + index++;
                        builder.AppendFormat(" LIKE ");
                        builder.Append(paramName);
                        var value = (node.Arguments[0] as ConstantExpression).Value;
                        cmd.Parameters.Add(paramName, value + "%");
                    }
                    break;

                case "EndsWith":
                    if (node.Method.DeclaringType == typeof(string))
                    {
                        var member = (node.Object as MemberExpression);
                        builder.Append(member.Member.Name);
                        var paramName = "@P" + index++;
                        builder.AppendFormat(" LIKE ");
                        builder.Append(paramName);
                        var value = (node.Arguments[0] as ConstantExpression).Value;
                        cmd.Parameters.Add(paramName, "%" + value);
                    }
                    break;

                case "Contains":
                    if (node.Method.DeclaringType == typeof(string))
                    {
                        var member = (node.Object as MemberExpression);
                        builder.Append(member.Member.Name);
                        var paramName = "@P" + index++;
                        builder.AppendFormat(" LIKE ");
                        builder.Append(paramName);
                        var value = (node.Arguments[0] as ConstantExpression).Value;
                        cmd.Parameters.Add(paramName, "%" + value + "%");
                        return node;
                    }
                    else if (node.Method.DeclaringType == typeof(Enumerable))
                    {
                        var source = node.Arguments[0] as ConstantExpression;
                        var match = (MemberExpression)node.Arguments[1];
                        return VisitContains(node, source, match);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(node.Method.DeclaringType))
                    {
                        var source = node.Object as ConstantExpression;
                        var match = (MemberExpression)node.Arguments[0];
                        return VisitContains(node, source, match);
                    }
                    else
                    {
                        throw new InvalidOperationException("Contains方法只能由数组、IEnumerable对象调用");
                    }

                default:
                    throw new NotSupportedException("解析器不支持'{0}'方法".Fmt(node.Method.Name));
            }
            return node;
        }

        private MethodCallExpression VisitContains(MethodCallExpression node, ConstantExpression source, MemberExpression member)
        {
            var isString = member.Type == typeof(string);
            builder.Append(member.Member.Name);
            builder.Append(" IN (");
            bool wrote = false;
            foreach (var item in (IEnumerable)source.Value)
            {
                if (wrote)
                    builder.Append(",");
                var paramName = "@P" + index++;
                builder.Append(paramName);
                cmd.Parameters.Add(paramName, item);
                wrote = true;
            }
            builder.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                builder.Append("NULL");
            }
            else
            {
                var typeCode = Type.GetTypeCode(node.Value.GetType());
                var value = typeCode == TypeCode.Boolean ? (((bool)node.Value) ? 1 : 0) : node.Value;
                switch (typeCode)
                {
                    case TypeCode.Object:
                        throw new NotSupportedException("不支持常量'{0}'".Fmt(node.Value));
                    case TypeCode.Boolean:
                        builder.Append("1={0}".Fmt(value));
                        break;
                    default:
                        var paramName = "@P" + index++;
                        builder.Append(paramName);
                        cmd.Parameters.Add(paramName, value);
                        break;
                }
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            builder.Append(node.Member.Name);
            return base.VisitMember(node);
        }
    }
}
