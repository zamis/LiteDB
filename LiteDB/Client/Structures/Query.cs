﻿using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    /// <summary>
    /// Class is a result from optimized QueryBuild. Indicate how engine must run query - there is no more decisions to engine made, must only execute as query was defined
    /// </summary>
    public partial class Query
    {
        /// <summary>
        /// Indicate when a query must execute in ascending order
        /// </summary>
        public const int Ascending = 1;

        /// <summary>
        /// Indicate when a query must execute in descending order
        /// </summary>
        public const int Descending = -1;

        /// <summary>
        /// Returns all documents that value are equals to value (=)
        /// </summary>
        public static BsonExpression EQ(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} = @0", value);
        }

        /// <summary>
        /// Returns all documents that value are less than value (&lt;)
        /// </summary>
        public static BsonExpression LT(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} < @0", value);
        }

        /// <summary>
        /// Returns all documents that value are less than or equals value (&lt;=)
        /// </summary>
        public static BsonExpression LTE(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} <= @0", value);
        }

        /// <summary>
        /// Returns all document that value are greater than value (&gt;)
        /// </summary>
        public static BsonExpression GT(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} > @0", value);

        }

        /// <summary>
        /// Returns all documents that value are greater than or equals value (&gt;=)
        /// </summary>
        public static BsonExpression GTE(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} >= @0", value);
        }

        /// <summary>
        /// Returns all document that values are between "start" and "end" values (BETWEEN)
        /// </summary>
        public static BsonExpression Between(string field, BsonValue start, BsonValue end)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} BETWEEN @0 AND @1", start, end);
        }

        /// <summary>
        /// Returns all documents that starts with value (LIKE)
        /// </summary>
        public static BsonExpression StartsWith(string field, string value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return BsonExpression.Create($"{field} LIKE @0", value + "%");
        }

        /// <summary>
        /// Returns all documents that contains value (CONTAINS)
        /// </summary>
        public static BsonExpression Contains(string field, string value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));
            if (value.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(value));

            return BsonExpression.Create($"{field} LIKE @0", "%" + value + "%");
        }

        /// <summary>
        /// Returns all documents that are not equals to value (not equals)
        /// </summary>
        public static BsonExpression Not(string field, BsonValue value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));

            return BsonExpression.Create($"{field} != @0", value);
        }

        /// <summary>
        /// Returns all documents that has value in values list (IN)
        /// </summary>
        public static BsonExpression In(string field, BsonArray value)
        {
            if (field.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(field));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return BsonExpression.Create($"{field} IN @0", value);
        }

        /// <summary>
        /// Returns all documents that has value in values list (IN)
        /// </summary>
        public static BsonExpression In(string field, params BsonValue[] values)
        {
            return In(field, new BsonArray(values));
        }

        /// <summary>
        /// Returns all documents that has value in values list (IN)
        /// </summary>
        public static BsonExpression In(string field, IEnumerable<BsonValue> values)
        {
            return In(field, new BsonArray(values));
        }

        /// <summary>
        /// Returns document that exists in BOTH queries results. If both queries has indexes, left query has index preference (other side will be run in full scan)
        /// </summary>
        public static BsonExpression And(BsonExpression left, BsonExpression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return BsonExpressionParser.CreateBinaryExpression("AND", left, right);
        }

        /// <summary>
        /// Returns document that exists in ALL queries results.
        /// </summary>
        public static BsonExpression And(params BsonExpression[] queries)
        {
            if (queries == null || queries.Length < 2) throw new ArgumentException("At least two Query should be passed");

            var left = queries[0];

            for (int i = 1; i < queries.Length; i++)
            {
                left = And(left, queries[i]);
            }

            return left;
        }

        /// <summary>
        /// Returns documents that exists in ANY queries results (Union).
        /// </summary>
        public static BsonExpression Or(BsonExpression left, BsonExpression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return BsonExpressionParser.CreateBinaryExpression("OR", left, right);
        }

        /// <summary>
        /// Returns document that exists in ANY queries results (Union).
        /// </summary>
        public static BsonExpression Or(params BsonExpression[] queries)
        {
            if (queries == null || queries.Length < 2) throw new ArgumentException("At least two Query should be passed");

            var left = queries[0];

            for (int i = 1; i < queries.Length; i++)
            {
                left = Or(left, queries[i]);
            }

            return left;
        }
    }
}