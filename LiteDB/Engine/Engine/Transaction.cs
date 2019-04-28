﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using static LiteDB.Constants;

namespace LiteDB.Engine
{
    public partial class LiteEngine
    {
        private readonly ConcurrentDictionary<uint, TransactionService> _transactions = new ConcurrentDictionary<uint, TransactionService>();
        private LocalDataStoreSlot _slot = Thread.GetNamedDataSlot(Guid.NewGuid().ToString("n"));

        internal TransactionService GetTransaction(bool create, out bool isNew)
        {
            var transaction = Thread.GetData(_slot) as TransactionService;

            if (create && transaction == null)
            {
                isNew = true;

                transaction = new TransactionService(_header, _locker, _disk, _walIndex, _settings.MaxTransactionSize, (id) =>
                {
                    Thread.SetData(_slot, null);

                    _transactions.TryRemove(id, out var t);
                });

                // add transaction to execution transaction dict
                _transactions[transaction.TransactionID] = transaction;

                Thread.SetData(_slot, transaction);
            }
            else
            {
                isNew = false;
            }

            return transaction;
        }

        /// <summary>
        /// Initialize a new transaction. Transaction are created "per-thread". There is only one single transaction per thread.
        /// Return true if transaction was created or false if current thread already in a transaction.
        /// </summary>
        public bool BeginTrans()
        {
            var transacion = this.GetTransaction(true, out var isNew);

            transacion.ExplicitTransaction = true;

            if (transacion.OpenCursors > 0) throw new LiteException(0, "This thread contains an open cursors/query. Close cursors before Begin()");

            return isNew;
        }

        /// <summary>
        /// Persist all dirty pages into LOG file
        /// </summary>
        public bool Commit()
        {
            var transaction = this.GetTransaction(false, out var isNew);

            if (transaction != null)
            {
                // do not accept explicit commit transaction when contains open cursors running
                if (transaction.OpenCursors > 0) throw new LiteException(0, "This thread contains an open query/cursor. Close cursors before run Commit()");

                return transaction.Commit();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Do rollback to current transaction. Clear dirty pages in memory and return new pages to main empty linked-list
        /// </summary>
        public bool Rollback()
        {
            var transaction = this.GetTransaction(false, out var isNew);

            if (transaction != null)
            {
                return transaction.Rollback();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create (or reuse) a transaction an add try/catch block. Commit transaction if is new transaction
        /// </summary>
        private T AutoTransaction<T>(Func<TransactionService, T> fn)
        {
            var transaction = this.GetTransaction(true, out var isNew);

            try
            {
                var result = fn(transaction);

                // if this transaction was auto-created for this operation, commit & dispose now
                if (isNew && transaction.OpenCursors == 0)
                {
                    transaction.Commit();
                }

                return result;
            }
            catch(Exception ex)
            {
                LOG(ex.Message, "ERROR");

                transaction.Rollback();

                throw;
            }
        }
    }
}