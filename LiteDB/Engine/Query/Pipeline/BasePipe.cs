﻿using System;
using System.Collections.Generic;
using System.Linq;
using static LiteDB.Constants;

namespace LiteDB.Engine
{
    /// <summary>
    /// Abstract class with workflow method to be used in pipeline implementation
    /// </summary>
    internal abstract class BasePipe : IDisposable
    {
        public event EventHandler Disposing = null;

        protected readonly TransactionService _transaction;
        protected readonly IDocumentLookup _lookup;
        protected readonly TempDisk _tempDisk;
        protected readonly bool _utcDate;

        public BasePipe(TransactionService transaction, IDocumentLookup lookup, TempDisk tempDisk, bool utcDate)
        {
            _transaction = transaction;
            _lookup = lookup;
            _tempDisk = tempDisk;
            _utcDate = utcDate;
        }

        /// <summary>
        /// Abstract method to be implement according pipe workflow
        /// </summary>
        public abstract IEnumerable<BsonDocument> Pipe(IEnumerable<IndexNode> nodes, QueryPlan query);

        // load documents from document loader
        protected IEnumerable<BsonDocument> LoadDocument(IEnumerable<IndexNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return _lookup.Load(node);

                // check if transaction all full of pages to clear before continue
                _transaction.Safepoint();
            }
        }

        /// <summary>
        /// INCLUDE: Do include in result document according path expression - Works only with DocumentLookup
        /// </summary>
        protected IEnumerable<BsonDocument> Include(IEnumerable<BsonDocument> source, BsonExpression path)
        {
            // cached services
            string last = null;
            Snapshot snapshot = null;
            IndexService indexer = null;
            DataService data = null;
            CollectionIndex index = null;
            IDocumentLookup lookup = null;

            foreach (var doc in source)
            {
                foreach (var value in path.Execute(doc)
                                        .Where(x => x.IsDocument)
                                        .Select(x => x.AsDocument)
                                        .ToList())
                {
                    // works only if is a document
                    var refId = value["$id"];
                    var refCol = value["$ref"];

                    // if has no reference, just go out
                    if (refId.IsNull || !refCol.IsString) continue;

                    // do some cache re-using when is same $ref (almost always is the same $ref collection)
                    if (last != refCol.AsString)
                    {
                        last = refCol.AsString;

                        // initialize services
                        snapshot = _transaction.CreateSnapshot(LockMode.Read, last, false);
                        indexer = new IndexService(snapshot);
                        data = new DataService(snapshot);

                        lookup = new DatafileLookup(data, _utcDate, null);

                        index = snapshot.CollectionPage?.PK;
                    }

                    // fill only if index and ref node exists
                    if (index != null)
                    {
                        var node = indexer.Find(index, refId, false, Query.Ascending);

                        if (node != null)
                        {
                            // load document based on dataBlock position
                            var refDoc = lookup.Load(node);

                            value.Remove("$id");
                            value.Remove("$ref");
                            
                            refDoc.CopyTo(value);
                        }
                    }
                }

                yield return doc;
            }
        }

        /// <summary>
        /// WHERE: Filter document according expression. Expression must be an Bool result
        /// </summary>
        protected IEnumerable<BsonDocument> Filter(IEnumerable<BsonDocument> source, BsonExpression expr)
        {
            foreach(var doc in source)
            {
                // checks if any result of expression is true
                var result = expr.ExecuteScalar(doc);

                if(result.IsBoolean && result.AsBoolean)
                {
                    yield return doc;
                }
            }
        }

        /// <summary>
        /// ORDER BY: Sort documents according orderby expression and order asc/desc
        /// </summary>
        protected IEnumerable<BsonDocument> OrderBy(IEnumerable<BsonDocument> source, BsonExpression expr, int order, int offset, int limit)
        {
            var keyValues = source
                .Select(x => new KeyValuePair<BsonValue, PageAddress>(expr.ExecuteScalar(x), x.RawId));

            using (var sorter = new SortService(_tempDisk, order))
            {
                sorter.Insert(keyValues);

                LOG($"sort {sorter.Count} keys in {sorter.Containers.Count} containers", "SORT");

                var result = sorter.Sort().Skip(offset).Take(limit);

                foreach (var keyValue in result)
                {
                    var doc = _lookup.Load(keyValue.Value);

                    yield return doc;
                }
            }

        }

        public void Dispose()
        {
            // call disposing event
            this.Disposing?.Invoke(this, EventArgs.Empty);
        }
    }
}