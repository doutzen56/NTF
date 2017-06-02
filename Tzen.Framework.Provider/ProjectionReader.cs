using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Tzen.Framework.Provider {

    // implements reading over a data-reader and producing projected objects 
    public class ProjectionReader<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        Enumerator enumerator;

        public ProjectionReader(DbDataReader reader, Func<DbDataReader, T> projector)
        {
            this.enumerator = new Enumerator(reader, projector);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Enumerator e = this.enumerator;
            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            this.enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            if (this.enumerator != null)
            {
                this.enumerator.Dispose();
                this.enumerator = null;
            }
        }

        class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            DbDataReader reader;
            Func<DbDataReader, T> projector;
            T current;

            internal Enumerator(DbDataReader reader, Func<DbDataReader, T> projector)
            {
                this.reader = reader;
                this.projector = projector;
            }

            public T Current
            {
                get { return this.current; }
            }

            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public bool MoveNext()
            {
                if (this.reader.Read())
                {
                    this.current = this.projector(this.reader);
                    return true;
                }
                this.Dispose();
                return false;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                this.reader.Dispose();
            }
        }
    }
}
